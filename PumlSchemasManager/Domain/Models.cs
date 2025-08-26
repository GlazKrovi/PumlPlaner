using System.ComponentModel.DataAnnotations;
using LiteDB;

namespace PumlSchemasManager.Domain;

/// <summary>
/// Represents a PlantUML schema with its content and metadata
/// </summary>
public class Schema
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;
    
    /// <summary>
    /// Original file path where the schema was discovered
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;
    
    /// <summary>
    /// PlantUML content of the schema
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Generated output files from this schema
    /// </summary>
    public List<GeneratedFile> GeneratedFiles { get; set; } = new();
    
    /// <summary>
    /// Metadata about the schema
    /// </summary>
    public SchemaMetadata Metadata { get; set; } = new();
    
    /// <summary>
    /// Project this schema belongs to
    /// </summary>
    public ObjectId ProjectId { get; set; } = ObjectId.Empty;
}

/// <summary>
/// Represents a project containing multiple schemas
/// </summary>
public class Project
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;
    
    /// <summary>
    /// Name of the project
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Schemas contained in this project
    /// </summary>
    public List<ObjectId> SchemaIds { get; set; } = new();
    
    /// <summary>
    /// When the project was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the project was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a generated output file from a schema
/// </summary>
public class GeneratedFile
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.Empty;
    
    /// <summary>
    /// Reference to the schema that generated this file
    /// </summary>
    public ObjectId SchemaId { get; set; } = ObjectId.Empty;
    
    /// <summary>
    /// Output format (PNG, SVG, etc.)
    /// </summary>
    public SchemaOutputFormat Format { get; set; }
    
    /// <summary>
    /// Path where the file is stored in LiteDB FileStorage
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// When the file was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
}

/// <summary>
/// Metadata about a schema
/// </summary>
public class SchemaMetadata
{
    /// <summary>
    /// When the schema was discovered
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Original file path
    /// </summary>
    public string OriginalPath { get; set; } = string.Empty;
    
    /// <summary>
    /// SHA256 hash of the content for change detection
    /// </summary>
    public string Hash { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Last modification time of the original file
    /// </summary>
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Supported output formats for PlantUML rendering
/// </summary>
public enum SchemaOutputFormat
{
    Png,
    Svg,
    Pdf,
    Eps,
    Vdx,
    Xmi,
    Scxml,
    Html,
    Txt,
    Utxt,
    Latex,
    LatexNoPreamble
}

/// <summary>
/// Result of parsing a PlantUML schema
/// </summary>
public class ParseResult
{
    /// <summary>
    /// Whether parsing was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Error message if parsing failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Parsed schema if successful
    /// </summary>
    public Schema? Schema { get; set; }
    
    /// <summary>
    /// Warnings during parsing
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Result of schema validation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether validation was successful
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Validation errors
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Metadata for file storage operations
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Content type/MIME type
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// When the file was stored
    /// </summary>
    public DateTime StoredAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional metadata as key-value pairs
    /// </summary>
    public Dictionary<string, object> AdditionalMetadata { get; set; } = new();
}
