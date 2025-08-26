using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;
using System.Security.Cryptography;
using System.Text;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// File system discovery service for PlantUML files
/// </summary>
public class FileSystemDiscoveryService : IFileDiscoveryService
{
    private readonly IParser _parser;
    private readonly string[] _plantUmlExtensions = { ".puml", ".plantuml", ".uml" };

    public FileSystemDiscoveryService(IParser parser)
    {
        _parser = parser;
    }

    public async Task<List<Schema>> DiscoverSchemasAsync(string folderPath)
    {
        return await Task.Run(async () =>
        {
            var schemas = new List<Schema>();
            
            if (!Directory.Exists(folderPath))
            {
                return new List<Schema>();
            }

            // Search for PlantUML files recursively
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => _plantUmlExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    var schema = await ProcessFileAsync(file);
                    if (schema != null)
                    {
                        schemas.Add(schema);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }

            return schemas;
        });
    }

    public async Task<ValidationResult> ValidateSchemaAsync(Schema schema)
    {
        return await _parser.ValidateAsync(schema.Content);
    }

    public async Task<bool> IsPlantUmlFileAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            if (!File.Exists(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!_plantUmlExtensions.Contains(extension))
                return false;

            // Check if file contains PlantUML content
            try
            {
                var content = File.ReadAllText(filePath);
                return content.Contains("@startuml") || content.Contains("@start");
            }
            catch
            {
                return false;
            }
        });
    }

    public async Task<FileInfo?> GetFileInfoAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                return new FileInfo(filePath);
            }
            catch
            {
                return null;
            }
        });
    }

    /// <summary>
    /// Processes a single file and creates a schema
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>Schema if valid, null otherwise</returns>
    private async Task<Schema?> ProcessFileAsync(string filePath)
    {
        try
        {
            var fileInfo = await GetFileInfoAsync(filePath);
            if (fileInfo == null)
                return null;

            var content = await File.ReadAllTextAsync(filePath);
            
            // Parse the content using PumlPlaner logic
            Console.WriteLine($"Parsing content from {filePath}: {content.Substring(0, Math.Min(50, content.Length))}...");
            
            if (_parser == null)
            {
                Console.WriteLine($"Parser is null for {filePath}");
                return null;
            }
            
            ParseResult parseResult;
            try
            {
                parseResult = await _parser.ParseAsync(content);
            }
            catch (Exception parseEx)
            {
                Console.WriteLine($"Exception in parser.ParseAsync: {parseEx.Message}");
                Console.WriteLine($"Stack trace: {parseEx.StackTrace}");
                return null;
            }
            
            Console.WriteLine($"Parse result: IsSuccess={parseResult.IsSuccess}, Schema={parseResult.Schema != null}");
            
            if (!parseResult.IsSuccess)
            {
                Console.WriteLine($"Failed to parse {filePath}: {parseResult.ErrorMessage}");
                return null;
            }

            if (parseResult.Schema == null)
            {
                Console.WriteLine($"Parse result is successful but Schema is null for {filePath}");
                return null;
            }

            var schema = parseResult.Schema;
            schema.SourcePath = filePath;
            
            // Update metadata with file information
            schema.Metadata.OriginalPath = filePath;
            schema.Metadata.FileSize = fileInfo.Length;
            schema.Metadata.LastModified = fileInfo.LastWriteTimeUtc;
            schema.Metadata.Hash = LiteDbStorageService.ComputeHash(content);

            return schema;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }
}
