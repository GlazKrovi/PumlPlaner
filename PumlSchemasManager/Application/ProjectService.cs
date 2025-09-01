using LiteDB;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManager.Application;

/// <summary>
///     Service for managing projects
/// </summary>
public class ProjectService
{
    private readonly IStorageService _storageService;

    public ProjectService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    /// <summary>
    ///     Gets a project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project if found, null otherwise</returns>
    public async Task<Project?> GetProjectAsync(ObjectId id)
    {
        return await _storageService.LoadProjectAsync(id);
    }

    /// <summary>
    ///     Updates an existing project
    /// </summary>
    /// <param name="project">Project to update</param>
    public async Task UpdateProjectAsync(Project project)
    {
        await _storageService.UpdateProjectAsync(project);
    }

    /// <summary>
    ///     Deletes a project and all its associated data
    /// </summary>
    /// <param name="id">Project ID to delete</param>
    public async Task DeleteProjectAsync(ObjectId id)
    {
        await _storageService.DeleteProjectAsync(id);
    }

    /// <summary>
    ///     Lists all projects
    /// </summary>
    /// <returns>List of all projects</returns>
    public async Task<List<Project>> ListProjectsAsync()
    {
        return await _storageService.ListProjectsAsync();
    }

    /// <summary>
    ///     Gets projects by name (partial match)
    /// </summary>
    /// <param name="name">Project name to search for</param>
    /// <returns>List of matching projects</returns>
    public async Task<List<Project>> GetProjectsByNameAsync(string name)
    {
        var allProjects = await ListProjectsAsync();
        return allProjects
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    ///     Gets project statistics
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Project statistics</returns>
    public async Task<ProjectStatistics> GetProjectStatisticsAsync(ObjectId projectId)
    {
        var project = await GetProjectAsync(projectId);
        if (project == null)
            throw new ArgumentException($"Project with ID {projectId} not found", nameof(projectId));

        var schemas = new List<Schema>();
        var totalGeneratedFiles = 0;
        var totalFileSize = 0L;

        foreach (var schemaId in project.SchemaIds)
        {
            var schema = await _storageService.LoadSchemaAsync(schemaId);
            if (schema != null)
            {
                schemas.Add(schema);
                totalGeneratedFiles += schema.GeneratedFiles.Count;
                totalFileSize += schema.GeneratedFiles.Sum(f => f.FileSize);
            }
        }

        return new ProjectStatistics
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            SchemaCount = schemas.Count,
            GeneratedFileCount = totalGeneratedFiles,
            TotalFileSize = totalFileSize,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }
}

/// <summary>
///     Statistics for a project
/// </summary>
public class ProjectStatistics
{
    /// <summary>
    ///     Project ID
    /// </summary>
    public ObjectId ProjectId { get; set; } = ObjectId.Empty;

    /// <summary>
    ///     Project name
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    ///     Number of schemas in the project
    /// </summary>
    public int SchemaCount { get; set; }

    /// <summary>
    ///     Number of generated files
    /// </summary>
    public int GeneratedFileCount { get; set; }

    /// <summary>
    ///     Total size of generated files in bytes
    /// </summary>
    public long TotalFileSize { get; set; }

    /// <summary>
    ///     When the project was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     When the project was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}