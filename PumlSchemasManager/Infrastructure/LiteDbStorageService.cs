using LiteDB;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;
using System.Security.Cryptography;
using System.Text;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// LiteDB implementation of the storage service
/// </summary>
public class LiteDbStorageService : IStorageService, IDisposable
{
    private readonly ILiteDatabase _database;
    private readonly ILiteCollection<Project> _projects;
    private readonly ILiteCollection<Schema> _schemas;
    private readonly ILiteCollection<GeneratedFile> _generatedFiles;
    private readonly ILiteStorage<string> _fileStorage;

    public LiteDbStorageService(string connectionString = "Filename=PumlSchemasManager.db;Mode=Shared")
    {
        _database = new LiteDatabase(connectionString);
        
        // Configure collections
        _projects = _database.GetCollection<Project>("projects");
        _schemas = _database.GetCollection<Schema>("schemas");
        _generatedFiles = _database.GetCollection<GeneratedFile>("generatedFiles");
        
        // Configure FileStorage for binary files
        _fileStorage = _database.FileStorage;
        
        // Create indexes for better performance
        _projects.EnsureIndex(x => x.Name);
        _schemas.EnsureIndex(x => x.ProjectId);
        _schemas.EnsureIndex(x => x.SourcePath);
        _generatedFiles.EnsureIndex(x => x.SchemaId);
        
        // Configure BsonMapper for ObjectId references
        var mapper = _database.Mapper;
        // Disable DbRef for now to avoid mapping issues
        // mapper.Entity<Project>()
        //     .DbRef(x => x.SchemaIds, "schemas");
        // mapper.Entity<Schema>()
        //     .DbRef(x => x.ProjectId, "projects");
        // mapper.Entity<GeneratedFile>()
        //     .DbRef(x => x.SchemaId, "schemas");
    }

    public async Task<Project> SaveProjectAsync(Project project)
    {
        return await Task.Run(() =>
        {
            if (project.Id == ObjectId.Empty)
            {
                project.Id = ObjectId.NewObjectId();
                project.CreatedAt = DateTime.UtcNow;
            }
            project.UpdatedAt = DateTime.UtcNow;
            
            _projects.Insert(project);
            return project;
        });
    }

    public async Task<Project?> LoadProjectAsync(ObjectId id)
    {
        return await Task.Run(() =>
        {
            var project = _projects.FindById(id);
            if (project != null)
            {
                // Load referenced schemas
                var schemas = _schemas.Find(x => x.ProjectId == id).ToList();
                // Note: In a real implementation, you might want to populate the schemas
            }
            return project;
        });
    }

    public async Task UpdateProjectAsync(Project project)
    {
        await Task.Run(() =>
        {
            project.UpdatedAt = DateTime.UtcNow;
            _projects.Update(project);
        });
    }

    public async Task DeleteProjectAsync(ObjectId id)
    {
        await Task.Run(() =>
        {
            // Delete associated schemas
            var schemas = _schemas.Find(x => x.ProjectId == id).ToList();
            foreach (var schema in schemas)
            {
                // Delete generated files
                var generatedFiles = _generatedFiles.Find(x => x.SchemaId == schema.Id).ToList();
                foreach (var file in generatedFiles)
                {
                    try
                    {
                        _fileStorage.Delete(file.FilePath);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue
                        Console.WriteLine($"Error deleting file {file.FilePath}: {ex.Message}");
                    }
                }
                _generatedFiles.DeleteMany(x => x.SchemaId == schema.Id);
            }
            
            _schemas.DeleteMany(x => x.ProjectId == id);
            _projects.Delete(id);
        });
    }

    public async Task<List<Project>> ListProjectsAsync()
    {
        return await Task.Run(() => _projects.FindAll().ToList());
    }

    public async Task<Schema> SaveSchemaAsync(Schema schema)
    {
        return await Task.Run(() =>
        {
            if (schema.Id == ObjectId.Empty)
            {
                schema.Id = ObjectId.NewObjectId();
            }
            
            _schemas.Insert(schema);
            return schema;
        });
    }

    public async Task<Schema?> LoadSchemaAsync(ObjectId id)
    {
        return await Task.Run(() => _schemas.FindById(id));
    }

    public async Task UpdateSchemaAsync(Schema schema)
    {
        await Task.Run(() => _schemas.Update(schema));
    }

    public async Task<string> SaveGeneratedFileAsync(byte[] content, FileMetadata metadata)
    {
        return await Task.Run(() =>
        {
            var fileId = ObjectId.NewObjectId().ToString();
            var fileName = $"{fileId}_{metadata.FileName}";
            
            using var stream = new MemoryStream(content);
            var fileInfo = _fileStorage.Upload(fileName, fileName, stream);
            
            return fileInfo.Id;
        });
    }

    public async Task<(byte[] Content, FileMetadata Metadata)?> GetGeneratedFileAsync(string fileId)
    {
        return await Task.Run<(byte[] Content, FileMetadata Metadata)?>(() =>
        {
            try
            {
                var fileInfo = _fileStorage.FindById(fileId);
                if (fileInfo == null) return null;
                
                using var stream = new MemoryStream();
                _fileStorage.Download(fileId, stream);
                
                var metadata = new FileMetadata
                {
                    FileName = fileInfo.Filename,
                    ContentType = fileInfo.MimeType,
                    FileSize = fileInfo.Length,
                    StoredAt = fileInfo.UploadDate
                };
                
                return (stream.ToArray(), metadata);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving file {fileId}: {ex.Message}");
                return null;
            }
        });
    }

    public async Task<string> SaveDiscoveredFileAsync(string content, string originalPath)
    {
        return await Task.Run(() =>
        {
            var fileId = ObjectId.NewObjectId().ToString();
            var fileName = $"discovered_{fileId}_{Path.GetFileName(originalPath)}";
            
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var fileInfo = _fileStorage.Upload(fileName, fileName, stream);
            
            return fileInfo.Id;
        });
    }

    public async Task<string?> GetDiscoveredFileAsync(string fileId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var fileInfo = _fileStorage.FindById(fileId);
                if (fileInfo == null) return null;
                
                using var stream = new MemoryStream();
                _fileStorage.Download(fileId, stream);
                
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving discovered file {fileId}: {ex.Message}");
                return null;
            }
        });
    }

    /// <summary>
    /// Computes SHA256 hash of content for change detection
    /// </summary>
    /// <param name="content">Content to hash</param>
    /// <returns>SHA256 hash as hex string</returns>
    public static string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public void Dispose()
    {
        _database?.Dispose();
    }
}
