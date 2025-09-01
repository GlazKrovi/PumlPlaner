using LiteDB;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Core;

/// <summary>
///     Interface for parsing PlantUML schemas
/// </summary>
public interface IParser
{
    /// <summary>
    ///     Parses a PlantUML schema from source content
    /// </summary>
    /// <param name="source">PlantUML source content</param>
    /// <returns>Parse result with success status and parsed schema</returns>
    Task<ParseResult> ParseAsync(string source);

    /// <summary>
    ///     Validates if the content is valid PlantUML
    /// </summary>
    /// <param name="content">Content to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAsync(string content);
    
    /// <summary>
    ///     Gets the current parsing mode
    /// </summary>
    ParsingMode Mode { get; }
    
    /// <summary>
    ///     Gets parser capabilities
    /// </summary>
    ParserCapabilities Capabilities { get; }
}

/// <summary>
///     Interface for storage operations using LiteDB
/// </summary>
public interface IStorageService
{
    /// <summary>
    ///     Saves a project to the database
    /// </summary>
    /// <param name="project">Project to save</param>
    /// <returns>Saved project with generated ID</returns>
    Task<Project> SaveProjectAsync(Project project);

    /// <summary>
    ///     Loads a project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project if found, null otherwise</returns>
    Task<Project?> LoadProjectAsync(ObjectId id);

    /// <summary>
    ///     Updates an existing project
    /// </summary>
    /// <param name="project">Project to update</param>
    Task UpdateProjectAsync(Project project);

    /// <summary>
    ///     Deletes a project
    /// </summary>
    /// <param name="id">Project ID to delete</param>
    Task DeleteProjectAsync(ObjectId id);

    /// <summary>
    ///     Lists all projects
    /// </summary>
    /// <returns>List of all projects</returns>
    Task<List<Project>> ListProjectsAsync();

    /// <summary>
    ///     Saves a schema to the database
    /// </summary>
    /// <param name="schema">Schema to save</param>
    /// <returns>Saved schema with generated ID</returns>
    Task<Schema> SaveSchemaAsync(Schema schema);

    /// <summary>
    ///     Loads a schema by ID
    /// </summary>
    /// <param name="id">Schema ID</param>
    /// <returns>Schema if found, null otherwise</returns>
    Task<Schema?> LoadSchemaAsync(ObjectId id);

    /// <summary>
    ///     Updates an existing schema
    /// </summary>
    /// <param name="schema">Schema to update</param>
    Task UpdateSchemaAsync(Schema schema);

    /// <summary>
    ///     Saves a generated file to LiteDB FileStorage
    /// </summary>
    /// <param name="content">File content</param>
    /// <param name="metadata">File metadata</param>
    /// <returns>File ID in FileStorage</returns>
    Task<string> SaveGeneratedFileAsync(byte[] content, FileMetadata metadata);

    /// <summary>
    ///     Retrieves a generated file from FileStorage
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <returns>File content and metadata</returns>
    Task<(byte[] Content, FileMetadata Metadata)?> GetGeneratedFileAsync(string fileId);

    /// <summary>
    ///     Saves a discovered file content to FileStorage
    /// </summary>
    /// <param name="content">File content</param>
    /// <param name="originalPath">Original file path</param>
    /// <returns>File ID in FileStorage</returns>
    Task<string> SaveDiscoveredFileAsync(string content, string originalPath);

    /// <summary>
    ///     Retrieves a discovered file from FileStorage
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <returns>File content</returns>
    Task<string?> GetDiscoveredFileAsync(string fileId);
}

/// <summary>
///     Interface for discovering PlantUML files in the file system
/// </summary>
public interface IFileDiscoveryService
{
    /// <summary>
    ///     Discovers PlantUML schemas in a folder recursively
    /// </summary>
    /// <param name="folderPath">Path to search in</param>
    /// <returns>List of discovered schemas</returns>
    Task<List<Schema>> DiscoverSchemasAsync(string folderPath);

    /// <summary>
    ///     Validates a schema file
    /// </summary>
    /// <param name="schema">Schema to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateSchemaAsync(Schema schema);

    /// <summary>
    ///     Checks if a file is a valid PlantUML file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if it's a valid PlantUML file</returns>
    Task<bool> IsPlantUmlFileAsync(string filePath);

    /// <summary>
    ///     Gets file information for a schema
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>File information</returns>
    Task<FileInfo?> GetFileInfoAsync(string filePath);
}

/// <summary>
///     Interface for rendering PlantUML schemas to various formats
/// </summary>
public interface IRendererService
{
    /// <summary>
    ///     Renders a schema to the specified format
    /// </summary>
    /// <param name="schema">Schema to render</param>
    /// <param name="format">Output format</param>
    /// <returns>Rendered content as bytes</returns>
    Task<byte[]> RenderAsync(Schema schema, SchemaOutputFormat format);

    /// <summary>
    ///     Renders a schema content to the specified format
    /// </summary>
    /// <param name="content">PlantUML content</param>
    /// <param name="format">Output format</param>
    /// <returns>Rendered content as bytes</returns>
    Task<byte[]> RenderContentAsync(string content, SchemaOutputFormat format);

    /// <summary>
    ///     Gets the content type for a specific output format
    /// </summary>
    /// <param name="format">Output format</param>
    /// <returns>MIME content type</returns>
    string GetContentType(SchemaOutputFormat format);

    /// <summary>
    ///     Gets the file extension for a specific output format
    /// </summary>
    /// <param name="format">Output format</param>
    /// <returns>File extension (without dot)</returns>
    string GetFileExtension(SchemaOutputFormat format);
}

/// <summary>
/// Parsing modes available
/// </summary>
public enum ParsingMode
{
    /// <summary>
    /// Parse using PlantUML online service
    /// </summary>
    Remote,
    
    /// <summary>
    /// Parse using local PlantUML installation
    /// </summary>
    Local,
    
    /// <summary>
    /// Parse using embedded PlantUML
    /// </summary>
    Embedded
}

/// <summary>
/// Parser capabilities
/// </summary>
public class ParserCapabilities
{
    /// <summary>
    /// Whether the parser can generate images
    /// </summary>
    public bool CanGenerateImages { get; set; }
    
    /// <summary>
    /// Whether the parser can validate syntax
    /// </summary>
    public bool CanValidateSyntax { get; set; }
    
    /// <summary>
    /// Whether the parser requires internet connection
    /// </summary>
    public bool RequiresInternet { get; set; }
    
    /// <summary>
    /// Supported output formats
    /// </summary>
    public List<string> SupportedFormats { get; set; } = new();
    
    /// <summary>
    /// Whether the parser is available
    /// </summary>
    public bool IsAvailable { get; set; }
}
