using LiteDB;
using PumlSchemasManager.Application;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Commands;

/// <summary>
/// Command for parsing a single PlantUML file
/// </summary>
public class ParseCommand
{
    private readonly IParser _parser;

    public ParseCommand(IParser parser)
    {
        _parser = parser;
    }

    /// <summary>
    /// Executes the parse command
    /// </summary>
    /// <param name="sourceFile">Path to the source file</param>
    /// <returns>Parse result</returns>
    public async Task<ParseResult> ExecuteAsync(string sourceFile)
    {
        if (!File.Exists(sourceFile))
        {
            return new ParseResult
            {
                IsSuccess = false,
                ErrorMessage = $"File not found: {sourceFile}"
            };
        }

        try
        {
            var content = await File.ReadAllTextAsync(sourceFile);
            return await _parser.ParseAsync(content);
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                IsSuccess = false,
                ErrorMessage = $"Error reading file: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Command for merging multiple schemas
/// </summary>
public class MergeCommand
{
    private readonly SchemaManager _schemaManager;

    public MergeCommand(SchemaManager schemaManager)
    {
        _schemaManager = schemaManager;
    }

    /// <summary>
    /// Executes the merge command
    /// </summary>
    /// <param name="schemas">Schemas to merge</param>
    /// <returns>Merged schema</returns>
    public async Task<Schema> ExecuteAsync(List<Schema> schemas)
    {
        return await _schemaManager.MergeSchemasAsync(schemas);
    }
}

/// <summary>
/// Command for discovering PlantUML files in a folder
/// </summary>
public class DiscoverCommand
{
    private readonly IFileDiscoveryService _discoveryService;

    public DiscoverCommand(IFileDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    /// <summary>
    /// Executes the discover command
    /// </summary>
    /// <param name="folderPath">Folder path to search</param>
    /// <returns>List of discovered schemas</returns>
    public async Task<List<Schema>> ExecuteAsync(string folderPath)
    {
        return await _discoveryService.DiscoverSchemasAsync(folderPath);
    }
}

/// <summary>
/// Command for generating output files
/// </summary>
public class GenerateCommand
{
    private readonly SchemaManager _schemaManager;

    public GenerateCommand(SchemaManager schemaManager)
    {
        _schemaManager = schemaManager;
    }

    /// <summary>
    /// Executes the generate command
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="formats">Output formats to generate</param>
    /// <returns>List of generated files</returns>
    public async Task<List<GeneratedFile>> ExecuteAsync(ObjectId projectId, List<SchemaOutputFormat> formats)
    {
        return await _schemaManager.GenerateOutputsAsync(projectId, formats);
    }
}

/// <summary>
/// Command for creating a new project
/// </summary>
public class CreateProjectCommand
{
    private readonly SchemaManager _schemaManager;

    public CreateProjectCommand(SchemaManager schemaManager)
    {
        _schemaManager = schemaManager;
    }

    /// <summary>
    /// Executes the create project command
    /// </summary>
    /// <param name="name">Project name</param>
    /// <returns>Created project</returns>
    public async Task<Project> ExecuteAsync(string name)
    {
        return await _schemaManager.CreateProjectAsync(name);
    }
}

/// <summary>
/// Command for adding schemas to a project
/// </summary>
public class AddSchemasCommand
{
    private readonly SchemaManager _schemaManager;

    public AddSchemasCommand(SchemaManager schemaManager)
    {
        _schemaManager = schemaManager;
    }

    /// <summary>
    /// Executes the add schemas command
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="schemas">Schemas to add</param>
    /// <returns>Updated project</returns>
    public async Task<Project> ExecuteAsync(ObjectId projectId, List<Schema> schemas)
    {
        return await _schemaManager.AddSchemasToProjectAsync(projectId, schemas);
    }
}

/// <summary>
/// Command for discovering and adding schemas to a project
/// </summary>
public class DiscoverAndAddCommand
{
    private readonly SchemaManager _schemaManager;

    public DiscoverAndAddCommand(SchemaManager schemaManager)
    {
        _schemaManager = schemaManager;
    }

    /// <summary>
    /// Executes the discover and add command
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="folderPath">Folder path to search</param>
    /// <returns>Updated project</returns>
    public async Task<Project> ExecuteAsync(ObjectId projectId, string folderPath)
    {
        return await _schemaManager.DiscoverAndAddSchemasAsync(projectId, folderPath);
    }
}
