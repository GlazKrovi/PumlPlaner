using NUnit.Framework;
using PumlSchemasManager.Infrastructure;
using PumlSchemasManager.Domain;
using PumlSchemasManager.Core;
using LiteDB;
using System.Text;

namespace PumlSchemasManagerTester.Infrastructure;

[TestFixture]
public class LiteDbStorageServiceTests : IDisposable
{
    private LiteDbStorageService _storageService;
    private string _testDbPath;

    [SetUp]
    public void Setup()
    {
        _testDbPath = $"test_db_{Guid.NewGuid()}.db";
        _storageService = new LiteDbStorageService($"Filename={_testDbPath};Mode=Exclusive");
    }

    [TearDown]
    public void Cleanup()
    {
        _storageService?.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    public void Dispose()
    {
        _storageService?.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [Test]
    public async Task SaveProjectAsync_ShouldCreateNewProject()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };

        // Act
        var savedProject = await _storageService.SaveProjectAsync(project);

        // Assert
        savedProject.Id.Should().NotBe(ObjectId.Empty);
        savedProject.Name.Should().Be("Test Project");
        savedProject.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        savedProject.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task LoadProjectAsync_ShouldReturnProject()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var savedProject = await _storageService.SaveProjectAsync(project);

        // Act
        var loadedProject = await _storageService.LoadProjectAsync(savedProject.Id);

        // Assert
        loadedProject.Should().NotBeNull();
        loadedProject!.Id.Should().Be(savedProject.Id);
        loadedProject.Name.Should().Be("Test Project");
    }

    [Test]
    public async Task LoadProjectAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = ObjectId.NewObjectId();

        // Act
        var loadedProject = await _storageService.LoadProjectAsync(nonExistentId);

        // Assert
        loadedProject.Should().BeNull();
    }

    [Test]
    public async Task UpdateProjectAsync_ShouldUpdateProject()
    {
        // Arrange
        var project = new Project { Name = "Original Name" };
        var savedProject = await _storageService.SaveProjectAsync(project);
        savedProject.Name = "Updated Name";

        // Act
        var originalUpdatedAt = savedProject.UpdatedAt;
        await _storageService.UpdateProjectAsync(savedProject);

        // Assert
        var updatedProject = await _storageService.LoadProjectAsync(savedProject.Id);
        updatedProject.Should().NotBeNull();
        updatedProject!.Name.Should().Be("Updated Name");
        // Check that UpdatedAt has been updated (should be different from original)
        updatedProject.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Test]
    public async Task DeleteProjectAsync_ShouldDeleteProject()
    {
        // Arrange
        var project = new Project { Name = "Test Project" };
        var savedProject = await _storageService.SaveProjectAsync(project);

        // Act
        await _storageService.DeleteProjectAsync(savedProject.Id);

        // Assert
        var deletedProject = await _storageService.LoadProjectAsync(savedProject.Id);
        deletedProject.Should().BeNull();
    }

    [Test]
    public async Task ListProjectsAsync_ShouldReturnAllProjects()
    {
        // Arrange
        var project1 = new Project { Name = "Project 1" };
        var project2 = new Project { Name = "Project 2" };
        await _storageService.SaveProjectAsync(project1);
        await _storageService.SaveProjectAsync(project2);

        // Act
        var projects = await _storageService.ListProjectsAsync();

        // Assert
        projects.Should().HaveCount(2);
        projects.Should().Contain(p => p.Name == "Project 1");
        projects.Should().Contain(p => p.Name == "Project 2");
    }

    [Test]
    public async Task SaveSchemaAsync_ShouldCreateNewSchema()
    {
        // Arrange
        var schema = new Schema 
        { 
            SourcePath = "test.puml",
            Content = "@startuml\nclass Test\n@enduml"
        };

        // Act
        var savedSchema = await _storageService.SaveSchemaAsync(schema);

        // Assert
        savedSchema.Id.Should().NotBe(ObjectId.Empty);
        savedSchema.SourcePath.Should().Be("test.puml");
        savedSchema.Content.Should().Be("@startuml\nclass Test\n@enduml");
    }

    [Test]
    public async Task LoadSchemaAsync_ShouldReturnSchema()
    {
        // Arrange
        var schema = new Schema 
        { 
            SourcePath = "test.puml",
            Content = "@startuml\nclass Test\n@enduml"
        };
        var savedSchema = await _storageService.SaveSchemaAsync(schema);

        // Act
        var loadedSchema = await _storageService.LoadSchemaAsync(savedSchema.Id);

        // Assert
        loadedSchema.Should().NotBeNull();
        loadedSchema!.Id.Should().Be(savedSchema.Id);
        loadedSchema.SourcePath.Should().Be("test.puml");
        loadedSchema.Content.Should().Be("@startuml\nclass Test\n@enduml");
    }

    [Test]
    public async Task UpdateSchemaAsync_ShouldUpdateSchema()
    {
        // Arrange
        var schema = new Schema 
        { 
            SourcePath = "original.puml",
            Content = "original content"
        };
        var savedSchema = await _storageService.SaveSchemaAsync(schema);
        savedSchema.Content = "updated content";

        // Act
        await _storageService.UpdateSchemaAsync(savedSchema);

        // Assert
        var updatedSchema = await _storageService.LoadSchemaAsync(savedSchema.Id);
        updatedSchema.Should().NotBeNull();
        updatedSchema!.Content.Should().Be("updated content");
    }

    [Test]
    public async Task SaveGeneratedFileAsync_ShouldStoreFile()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("test content");
        var metadata = new FileMetadata
        {
            FileName = "test.png",
            ContentType = "image/png",
            FileSize = content.Length
        };

        // Act
        var fileId = await _storageService.SaveGeneratedFileAsync(content, metadata);

        // Assert
        fileId.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetGeneratedFileAsync_ShouldRetrieveFile()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("test content");
        var metadata = new FileMetadata
        {
            FileName = "test.png",
            ContentType = "image/png",
            FileSize = content.Length
        };
        var fileId = await _storageService.SaveGeneratedFileAsync(content, metadata);

        // Act
        var result = await _storageService.GetGeneratedFileAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Content.Should().BeEquivalentTo(content);
        result.Value.Metadata.FileName.Should().Contain("test.png");
        result.Value.Metadata.ContentType.Should().Be("image/png");
        result.Value.Metadata.FileSize.Should().Be(content.Length);
    }

    [Test]
    public async Task GetGeneratedFileAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = "non_existent_id";

        // Act
        var result = await _storageService.GetGeneratedFileAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SaveDiscoveredFileAsync_ShouldStoreFile()
    {
        // Arrange
        var content = "@startuml\nclass Test\n@enduml";
        var originalPath = "test.puml";

        // Act
        var fileId = await _storageService.SaveDiscoveredFileAsync(content, originalPath);

        // Assert
        fileId.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetDiscoveredFileAsync_ShouldRetrieveFile()
    {
        // Arrange
        var content = "@startuml\nclass Test\n@enduml";
        var originalPath = "test.puml";
        var fileId = await _storageService.SaveDiscoveredFileAsync(content, originalPath);

        // Act
        var retrievedContent = await _storageService.GetDiscoveredFileAsync(fileId);

        // Assert
        retrievedContent.Should().Be(content);
    }

    [Test]
    public async Task GetDiscoveredFileAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = "non_existent_id";

        // Act
        var result = await _storageService.GetDiscoveredFileAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void ComputeHash_ShouldReturnConsistentHash()
    {
        // Arrange
        var content = "test content";

        // Act
        var hash1 = LiteDbStorageService.ComputeHash(content);
        var hash2 = LiteDbStorageService.ComputeHash(content);

        // Assert
        hash1.Should().Be(hash2);
        hash1.Should().NotBeEmpty();
        hash1.Should().HaveLength(64); // SHA256 hash length
    }

    [Test]
    public void ComputeHash_WithDifferentContent_ShouldReturnDifferentHashes()
    {
        // Arrange
        var content1 = "test content 1";
        var content2 = "test content 2";

        // Act
        var hash1 = LiteDbStorageService.ComputeHash(content1);
        var hash2 = LiteDbStorageService.ComputeHash(content2);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
