using LiteDB;
using PumlSchemasManager.Application;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManagerTester.Application;

[TestFixture]
public class ProjectServiceTests
{
    [SetUp]
    public void Setup()
    {
        _mockStorageService = new Mock<IStorageService>();
        _projectService = new ProjectService(_mockStorageService.Object);
    }

    private ProjectService _projectService;
    private Mock<IStorageService> _mockStorageService;

    [Test]
    public async Task GetProjectAsync_WithValidId_ShouldReturnProject()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project"
        };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _projectService.GetProjectAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(projectId);
        result.Name.Should().Be("Test Project");

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
    }

    [Test]
    public async Task GetProjectAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _projectService.GetProjectAsync(projectId);

        // Assert
        result.Should().BeNull();

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
    }

    [Test]
    public async Task UpdateProjectAsync_WithValidProject_ShouldUpdateProject()
    {
        // Arrange
        var project = new Project
        {
            Id = ObjectId.NewObjectId(),
            Name = "Updated Project"
        };

        _mockStorageService.Setup(s => s.UpdateProjectAsync(project))
            .Returns(Task.CompletedTask);

        // Act
        await _projectService.UpdateProjectAsync(project);

        // Assert
        _mockStorageService.Verify(s => s.UpdateProjectAsync(project), Times.Once);
    }

    [Test]
    public async Task DeleteProjectAsync_WithValidId_ShouldDeleteProject()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();

        _mockStorageService.Setup(s => s.DeleteProjectAsync(projectId))
            .Returns(Task.CompletedTask);

        // Act
        await _projectService.DeleteProjectAsync(projectId);

        // Assert
        _mockStorageService.Verify(s => s.DeleteProjectAsync(projectId), Times.Once);
    }

    [Test]
    public async Task ListProjectsAsync_ShouldReturnAllProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Id = ObjectId.NewObjectId(), Name = "Project 1" },
            new() { Id = ObjectId.NewObjectId(), Name = "Project 2" }
        };

        _mockStorageService.Setup(s => s.ListProjectsAsync())
            .ReturnsAsync(projects);

        // Act
        var result = await _projectService.ListProjectsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Project 1");
        result[1].Name.Should().Be("Project 2");

        _mockStorageService.Verify(s => s.ListProjectsAsync(), Times.Once);
    }

    [Test]
    public async Task GetProjectsByNameAsync_WithMatchingName_ShouldReturnMatchingProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Id = ObjectId.NewObjectId(), Name = "Test Project 1" },
            new() { Id = ObjectId.NewObjectId(), Name = "Another Project" },
            new() { Id = ObjectId.NewObjectId(), Name = "Test Project 2" }
        };

        _mockStorageService.Setup(s => s.ListProjectsAsync())
            .ReturnsAsync(projects);

        // Act
        var result = await _projectService.GetProjectsByNameAsync("Test");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Test Project 1");
        result.Should().Contain(p => p.Name == "Test Project 2");

        _mockStorageService.Verify(s => s.ListProjectsAsync(), Times.Once);
    }

    [Test]
    public async Task GetProjectsByNameAsync_WithNoMatchingName_ShouldReturnEmptyList()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Id = ObjectId.NewObjectId(), Name = "Project 1" },
            new() { Id = ObjectId.NewObjectId(), Name = "Project 2" }
        };

        _mockStorageService.Setup(s => s.ListProjectsAsync())
            .ReturnsAsync(projects);

        // Act
        var result = await _projectService.GetProjectsByNameAsync("Nonexistent");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockStorageService.Verify(s => s.ListProjectsAsync(), Times.Once);
    }

    [Test]
    public async Task GetProjectStatisticsAsync_WithValidProject_ShouldReturnStatistics()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var schemaId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            SchemaIds = [schemaId],
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        var schema = new Schema
        {
            Id = schemaId,
            GeneratedFiles =
            [
                new GeneratedFile { FileSize = 1024 },
                new GeneratedFile { FileSize = 2048 }
            ]
        };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);
        _mockStorageService.Setup(s => s.LoadSchemaAsync(schemaId))
            .ReturnsAsync(schema);

        // Act
        var result = await _projectService.GetProjectStatisticsAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result.ProjectId.Should().Be(projectId);
        result.ProjectName.Should().Be("Test Project");
        result.SchemaCount.Should().Be(1);
        result.GeneratedFileCount.Should().Be(2);
        result.TotalFileSize.Should().Be(3072);
        result.CreatedAt.Should().Be(project.CreatedAt);
        result.UpdatedAt.Should().Be(project.UpdatedAt);

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
        _mockStorageService.Verify(s => s.LoadSchemaAsync(schemaId), Times.Once);
    }

    [Test]
    public async Task GetProjectStatisticsAsync_WithInvalidProjectId_ShouldThrowException()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync((Project?)null);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _projectService.GetProjectStatisticsAsync(projectId));

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
    }

    [Test]
    public async Task GetProjectStatisticsAsync_WithProjectHavingNoSchemas_ShouldReturnZeroStatistics()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Empty Project",
            SchemaIds = [],
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);

        // Act
        var result = await _projectService.GetProjectStatisticsAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result.ProjectId.Should().Be(projectId);
        result.ProjectName.Should().Be("Empty Project");
        result.SchemaCount.Should().Be(0);
        result.GeneratedFileCount.Should().Be(0);
        result.TotalFileSize.Should().Be(0);

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
    }
}