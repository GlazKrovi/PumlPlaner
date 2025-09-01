using LiteDB;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Application;

/// <summary>
///     Main service for managing PlantUML schemas and projects
/// </summary>
public class SchemaManager
{
    private readonly IFileDiscoveryService _discoveryService;
    private readonly IParser _parser;
    private readonly IRendererService _rendererService;
    private readonly IStorageService _storageService;

    public SchemaManager(
        IStorageService storageService,
        IFileDiscoveryService discoveryService,
        IRendererService rendererService,
        IParser parser)
    {
        _storageService = storageService;
        _discoveryService = discoveryService;
        _rendererService = rendererService;
        _parser = parser;
    }

    /// <summary>
    ///     Creates a new project
    /// </summary>
    /// <param name="name">Project name</param>
    /// <returns>Created project</returns>
    public async Task<Project> CreateProjectAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty", nameof(name));

        var project = new Project
        {
            Name = name.Trim(),
            SchemaIds = []
        };

        return await _storageService.SaveProjectAsync(project);
    }

    /// <summary>
    ///     Adds schemas to an existing project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="schemas">Schemas to add</param>
    /// <returns>Updated project</returns>
    public async Task<Project> AddSchemasToProjectAsync(ObjectId projectId, List<Schema> schemas)
    {
        var project = await _storageService.LoadProjectAsync(projectId);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found", nameof(projectId));

        foreach (var schema in schemas)
        {
            schema.ProjectId = projectId;
            var savedSchema = await _storageService.SaveSchemaAsync(schema);
            project.SchemaIds.Add(savedSchema.Id);
        }

        await _storageService.UpdateProjectAsync(project);
        return project;
    }

    /// <summary>
    ///     Generates output files for all schemas in a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="formats">Output formats to generate</param>
    /// <returns>List of generated files</returns>
    public async Task<List<GeneratedFile>> GenerateOutputsAsync(ObjectId projectId, List<SchemaOutputFormat> formats)
    {
        var project = await _storageService.LoadProjectAsync(projectId);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found", nameof(projectId));

        var generatedFiles = new List<GeneratedFile>();

        foreach (var schemaId in project.SchemaIds)
        {
            var schema = await _storageService.LoadSchemaAsync(schemaId);
            if (schema == null) continue;

            foreach (var format in formats)
                try
                {
                    var renderedContent = await _rendererService.RenderAsync(schema, format);

                    var metadata = new FileMetadata
                    {
                        FileName = $"{schema.Id}_{format}.{_rendererService.GetFileExtension(format)}",
                        ContentType = _rendererService.GetContentType(format),
                        FileSize = renderedContent.Length,
                        AdditionalMetadata = new Dictionary<string, object>
                        {
                            ["schemaId"] = schema.Id.ToString(),
                            ["format"] = format.ToString()
                        }
                    };

                    var fileId = await _storageService.SaveGeneratedFileAsync(renderedContent, metadata);

                    var generatedFile = new GeneratedFile
                    {
                        SchemaId = schema.Id,
                        Format = format,
                        FilePath = fileId,
                        FileSize = renderedContent.Length
                    };

                    generatedFiles.Add(generatedFile);

                    // Update schema with generated file
                    schema.GeneratedFiles.Add(generatedFile);
                    await _storageService.UpdateSchemaAsync(schema);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to generate {format} for schema {schema.Id}: {ex.Message}");
                }
        }

        return generatedFiles;
    }

    /// <summary>
    ///     Discovers PlantUML files in a folder and adds them to a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="folderPath">Folder path to search</param>
    /// <returns>Updated project with discovered schemas</returns>
    public async Task<Project> DiscoverAndAddSchemasAsync(ObjectId projectId, string folderPath)
    {
        var project = await _storageService.LoadProjectAsync(projectId);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found", nameof(projectId));

        var discoveredSchemas = await _discoveryService.DiscoverSchemasAsync(folderPath);

        if (discoveredSchemas.Any()) return await AddSchemasToProjectAsync(projectId, discoveredSchemas);

        return project;
    }

    /// <summary>
    ///     Parses a single PlantUML file and creates a schema
    /// </summary>
    /// <param name="filePath">Path to the PlantUML file</param>
    /// <returns>Parsed schema</returns>
    public async Task<Schema> ParseFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var content = await File.ReadAllTextAsync(filePath);
        var parseResult = await _parser.ParseAsync(content);

        if (!parseResult.IsSuccess)
            throw new InvalidOperationException($"Failed to parse file: {parseResult.ErrorMessage}");

        var schema = parseResult.Schema!;
        schema.SourcePath = filePath;

        // Add file metadata
        var fileInfo = new FileInfo(filePath);
        schema.Metadata.OriginalPath = filePath;
        schema.Metadata.FileSize = fileInfo.Length;
        schema.Metadata.LastModified = fileInfo.LastWriteTimeUtc;

        return schema;
    }

    /// <summary>
    ///     Merges multiple schemas into a single schema
    /// </summary>
    /// <param name="schemas">Schemas to merge</param>
    /// <returns>Merged schema</returns>
    public async Task<Schema> MergeSchemasAsync(List<Schema> schemas)
    {
        if (!schemas.Any())
            throw new ArgumentException("At least one schema is required", nameof(schemas));

        if (schemas.Count == 1)
            return schemas.First();

        // Simple merge strategy: combine all content with separators
        var mergedContent = string.Join("\n\n", schemas.Select(s => s.Content));

        var parseResult = await _parser.ParseAsync(mergedContent);
        if (!parseResult.IsSuccess)
            throw new InvalidOperationException($"Failed to merge schemas: {parseResult.ErrorMessage}");

        var mergedSchema = parseResult.Schema!;
        mergedSchema.SourcePath = "merged";
        mergedSchema.Metadata.DiscoveredAt = DateTime.UtcNow;
        mergedSchema.Metadata.OriginalPath = "merged";

        return mergedSchema;
    }

    /// <summary>
    ///     Gets all schemas for a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>List of schemas</returns>
    public async Task<List<Schema>> GetProjectSchemasAsync(ObjectId projectId)
    {
        var project = await _storageService.LoadProjectAsync(projectId);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found", nameof(projectId));

        var schemas = new List<Schema>();
        foreach (var schemaId in project.SchemaIds)
        {
            var schema = await _storageService.LoadSchemaAsync(schemaId);
            if (schema != null) schemas.Add(schema);
        }

        return schemas;
    }
}