using System.Text;
using PumlSchemasManager.Application;
using PumlSchemasManager.Domain;
using PumlSchemasManager.Infrastructure;

namespace PumlSchemasManagerTester.Integration;

[TestFixture]
public class IntegrationTests : IDisposable
{
    [SetUp]
    public void Setup()
    {
        _testDbPath = $"integration_test_{Guid.NewGuid()}.db";
        _testDirectory = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}");

        Directory.CreateDirectory(_testDirectory);

        _storageService = new LiteDbStorageService($"Filename={_testDbPath};Mode=Exclusive");
        _parser = new PlantUmlParser();
        _renderer = new PlantUmlRendererService();
        _discoveryService = new FileSystemDiscoveryService(_parser);

        _projectService = new ProjectService(_storageService);
        _schemaManager = new SchemaManager(_storageService, _discoveryService, _renderer, _parser);
    }

    [TearDown]
    public void Cleanup()
    {
        _storageService?.Dispose();
        if (File.Exists(_testDbPath)) File.Delete(_testDbPath);
        if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
    }

    private ProjectService _projectService;
    private SchemaManager _schemaManager;
    private LiteDbStorageService _storageService;
    private PlantUmlParser _parser;
    private PlantUmlRendererService _renderer;
    private FileSystemDiscoveryService _discoveryService;
    private string _testDbPath;
    private string _testDirectory;

    public void Dispose()
    {
        _storageService?.Dispose();
        if (File.Exists(_testDbPath)) File.Delete(_testDbPath);
        if (Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory, true);
    }

    [Test]
    public async Task FullWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var projectName = "Integration Test Project";
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var testFilePath = Path.Combine(_testDirectory, "test.puml");
        await File.WriteAllTextAsync(testFilePath, plantUmlContent);

        // Act 1: Create project
        var project = await _schemaManager.CreateProjectAsync(projectName);
        project.Should().NotBeNull();
        project.Name.Should().Be(projectName);

        // Act 2: Discover schemas
        var updatedProject = await _schemaManager.DiscoverAndAddSchemasAsync(project.Id, _testDirectory);
        updatedProject.Should().NotBeNull();
        updatedProject.SchemaIds.Should().HaveCount(1);

        // Act 3: Get project schemas
        var schemas = await _schemaManager.GetProjectSchemasAsync(project.Id);
        schemas.Should().HaveCount(1);
        var schema = schemas.First();
        schema.Content.Should().Be(plantUmlContent);

        // Act 4: Generate outputs
        var formats = new List<SchemaOutputFormat> { SchemaOutputFormat.Png, SchemaOutputFormat.Svg };
        var generatedFiles = await _schemaManager.GenerateOutputsAsync(project.Id, formats);
        generatedFiles.Should().HaveCount(2);

        // Act 5: Get project statistics
        var stats = await _projectService.GetProjectStatisticsAsync(project.Id);
        stats.Should().NotBeNull();
        stats!.SchemaCount.Should().Be(1);
        stats.GeneratedFileCount.Should().Be(2);

        // Assert
        project.Name.Should().Be(projectName);
        schema.Content.Should().Be(plantUmlContent);
        schema.ProjectId.Should().Be(project.Id);
        generatedFiles.Should().Contain(f => f.Format == SchemaOutputFormat.Png);
        generatedFiles.Should().Contain(f => f.Format == SchemaOutputFormat.Svg);
    }

    [Test]
    public async Task MultipleProjects_ShouldWorkIndependently()
    {
        // Arrange
        var project1Name = "Project 1";
        var project2Name = "Project 2";

        // Act: Create two projects
        var project1 = await _schemaManager.CreateProjectAsync(project1Name);
        var project2 = await _schemaManager.CreateProjectAsync(project2Name);

        // Assert
        project1.Should().NotBeNull();
        project2.Should().NotBeNull();
        project1.Id.Should().NotBe(project2.Id);
        project1.Name.Should().Be(project1Name);
        project2.Name.Should().Be(project2Name);

        // Act: List projects
        var projects = await _projectService.ListProjectsAsync();
        projects.Should().HaveCount(2);
        projects.Should().Contain(p => p.Name == project1Name);
        projects.Should().Contain(p => p.Name == project2Name);
    }

    [Test]
    public async Task FileStorage_ShouldPersistData()
    {
        // Arrange
        var content = "test content";
        var metadata = new FileMetadata
        {
            FileName = "test.txt",
            ContentType = "text/plain",
            FileSize = content.Length
        };

        // Act
        var fileId = await _storageService.SaveGeneratedFileAsync(Encoding.UTF8.GetBytes(content), metadata);
        var retrievedFile = await _storageService.GetGeneratedFileAsync(fileId);

        // Assert
        fileId.Should().NotBeNullOrEmpty();
        retrievedFile.Should().NotBeNull();
        retrievedFile!.Value.Content.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(content));
        retrievedFile.Value.Metadata.FileName.Should().Contain("test.txt");
        retrievedFile.Value.Metadata.ContentType.Should().Be("text/plain");
        retrievedFile.Value.Metadata.FileSize.Should().Be(content.Length);
    }

    [Test]
    public async Task ProjectStatistics_ShouldCalculateCorrectly()
    {
        // Arrange
        var project = await _schemaManager.CreateProjectAsync("Statistics Test Project");
        var schema = new Schema
        {
            Content = "@startuml\nclass Test\n@enduml",
            GeneratedFiles =
            [
                new GeneratedFile { FileSize = 1024 },
                new GeneratedFile { FileSize = 2048 }
            ]
        };

        // Act
        await _schemaManager.AddSchemasToProjectAsync(project.Id, [schema]);
        var stats = await _projectService.GetProjectStatisticsAsync(project.Id);

        // Assert
        stats.Should().NotBeNull();
        stats!.ProjectId.Should().Be(project.Id);
        stats.ProjectName.Should().Be("Statistics Test Project");
        stats.SchemaCount.Should().Be(1);
        stats.GeneratedFileCount.Should().Be(2);
        stats.TotalFileSize.Should().Be(3072);
    }

    [Test]
    public async Task SchemaParsing_ShouldWorkCorrectly()
    {
        // Arrange
        var validContent = "@startuml\nclass Test\n@enduml";
        var testFilePath = Path.Combine(_testDirectory, "test.puml");
        await File.WriteAllTextAsync(testFilePath, validContent);

        // Act
        var schema = await _schemaManager.ParseFileAsync(testFilePath);

        // Assert
        schema.Should().NotBeNull();
        schema.Content.Should().Be(validContent);
        schema.SourcePath.Should().Be(testFilePath);
        schema.Metadata.OriginalPath.Should().Be(testFilePath);
        schema.Metadata.FileSize.Should().Be(validContent.Length);
    }

    [Test]
    public async Task SchemaMerging_ShouldWorkCorrectly()
    {
        // Arrange
        var schemas = new List<Schema>
        {
            new() { Content = "@startuml\nclass A\n@enduml" },
            new() { Content = "@startuml\nclass B\n@enduml" }
        };

        // Act
        var mergedSchema = await _schemaManager.MergeSchemasAsync(schemas);

        // Assert
        mergedSchema.Should().NotBeNull();
        mergedSchema.SourcePath.Should().Be("merged");
        mergedSchema.Content.Should().Contain("class A");
        mergedSchema.Content.Should().Contain("class B");
    }

    [Test]
    public async Task ProjectSearch_ShouldFindProjectsByName()
    {
        // Arrange
        var project1 = await _schemaManager.CreateProjectAsync("Test Project Alpha");
        var project2 = await _schemaManager.CreateProjectAsync("Another Project");
        var project3 = await _schemaManager.CreateProjectAsync("Test Project Beta");

        // Act
        var testProjects = await _projectService.GetProjectsByNameAsync("Test");

        // Assert
        testProjects.Should().HaveCount(2);
        testProjects.Should().Contain(p => p.Name == "Test Project Alpha");
        testProjects.Should().Contain(p => p.Name == "Test Project Beta");
    }

    [Test]
    public async Task ProjectDeletion_ShouldWorkCorrectly()
    {
        // Arrange
        var project = await _schemaManager.CreateProjectAsync("Delete Test Project");

        // Act
        await _projectService.DeleteProjectAsync(project.Id);

        // Assert
        var retrievedProject = await _projectService.GetProjectAsync(project.Id);
        retrievedProject.Should().BeNull();
    }
}