using LiteDB;
using PumlSchemasManager.Application;
using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;

namespace PumlSchemasManagerTester.Application;

[TestFixture]
public class SchemaManagerTests
{
    private SchemaManager _schemaManager;
    private Mock<IStorageService> _mockStorageService;
    private Mock<IParser> _mockParser;
    private Mock<IRendererService> _mockRendererService;
    private Mock<IFileDiscoveryService> _mockDiscoveryService;

    [SetUp]
    public void Setup()
    {
        _mockStorageService = new Mock<IStorageService>();
        _mockParser = new Mock<IParser>();
        _mockRendererService = new Mock<IRendererService>();
        _mockDiscoveryService = new Mock<IFileDiscoveryService>();
        
        _schemaManager = new SchemaManager(
            _mockStorageService.Object,
            _mockDiscoveryService.Object,
            _mockRendererService.Object,
            _mockParser.Object
        );
    }

    [Test]
    public async Task CreateProjectAsync_WithValidName_ShouldCreateProject()
    {
        // Arrange
        var projectName = "Test Project";
        var project = new Project
        {
            Id = ObjectId.NewObjectId(),
            Name = projectName
        };

        _mockStorageService.Setup(s => s.SaveProjectAsync(It.IsAny<Project>()))
            .ReturnsAsync(project);

        // Act
        var result = await _schemaManager.CreateProjectAsync(projectName);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(projectName);
        result.Id.Should().NotBe(ObjectId.Empty);

        _mockStorageService.Verify(s => s.SaveProjectAsync(It.IsAny<Project>()), Times.Once);
    }

    [Test]
    public async Task CreateProjectAsync_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var projectName = "";

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => 
            _schemaManager.CreateProjectAsync(projectName));
    }

    [Test]
    public async Task AddSchemasToProjectAsync_WithValidData_ShouldAddSchemas()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            SchemaIds = []
        };
        var schemas = new List<Schema>
        {
            new Schema { Content = "@startuml\nclass Test\n@enduml" }
        };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);
        _mockStorageService.Setup(s => s.SaveSchemaAsync(It.IsAny<Schema>()))
            .ReturnsAsync((Schema s) => { s.Id = ObjectId.NewObjectId(); return s; });
        _mockStorageService.Setup(s => s.UpdateProjectAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _schemaManager.AddSchemasToProjectAsync(projectId, schemas);

        // Assert
        result.Should().NotBeNull();
        result.SchemaIds.Should().HaveCount(1);

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
        _mockStorageService.Verify(s => s.SaveSchemaAsync(It.IsAny<Schema>()), Times.Once);
        _mockStorageService.Verify(s => s.UpdateProjectAsync(It.IsAny<Project>()), Times.Once);
    }

    [Test]
    public async Task GenerateOutputsAsync_WithValidProject_ShouldGenerateFiles()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var schemaId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            SchemaIds = [schemaId]
        };
        var schema = new Schema
        {
            Id = schemaId,
            Content = "@startuml\nclass Test\n@enduml"
        };
        var formats = new List<SchemaOutputFormat> { SchemaOutputFormat.Png, SchemaOutputFormat.Svg };
        var renderedContent = new byte[] { 1, 2, 3, 4 };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);
        _mockStorageService.Setup(s => s.LoadSchemaAsync(schemaId))
            .ReturnsAsync(schema);
        _mockRendererService.Setup(r => r.RenderAsync(schema, It.IsAny<SchemaOutputFormat>()))
            .ReturnsAsync(renderedContent);
        _mockRendererService.Setup(r => r.GetContentType(It.IsAny<SchemaOutputFormat>()))
            .Returns("image/png");
        _mockRendererService.Setup(r => r.GetFileExtension(It.IsAny<SchemaOutputFormat>()))
            .Returns(".png");
        _mockStorageService.Setup(s => s.SaveGeneratedFileAsync(It.IsAny<byte[]>(), It.IsAny<FileMetadata>()))
            .ReturnsAsync("file_id");
        _mockStorageService.Setup(s => s.UpdateSchemaAsync(It.IsAny<Schema>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _schemaManager.GenerateOutputsAsync(projectId, formats);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
        _mockRendererService.Verify(r => r.RenderAsync(schema, It.IsAny<SchemaOutputFormat>()), Times.Exactly(2));
        _mockStorageService.Verify(s => s.SaveGeneratedFileAsync(It.IsAny<byte[]>(), It.IsAny<FileMetadata>()), Times.Exactly(2));
    }

    [Test]
    public async Task GenerateOutputsAsync_WithRenderingFailure_ShouldHandleError()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var schemaId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            SchemaIds = [schemaId]
        };
        var schema = new Schema
        {
            Id = schemaId,
            Content = "@startuml\nclass Test\n@enduml"
        };
        var formats = new List<SchemaOutputFormat> { SchemaOutputFormat.Png };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);
        _mockStorageService.Setup(s => s.LoadSchemaAsync(schemaId))
            .ReturnsAsync(schema);
        _mockRendererService.Setup(r => r.RenderAsync(schema, It.IsAny<SchemaOutputFormat>()))
            .ThrowsAsync(new Exception("Rendering failed"));

        // Act
        var result = await _schemaManager.GenerateOutputsAsync(projectId, formats);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
        _mockRendererService.Verify(r => r.RenderAsync(schema, It.IsAny<SchemaOutputFormat>()), Times.Once);
        _mockStorageService.Verify(s => s.SaveGeneratedFileAsync(It.IsAny<byte[]>(), It.IsAny<FileMetadata>()), Times.Never);
    }

    [Test]
    public async Task DiscoverAndAddSchemasAsync_WithValidPath_ShouldDiscoverSchemas()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var folderPath = "C:\\test";
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            SchemaIds = []
        };
        var discoveredSchemas = new List<Schema>
        {
            new Schema { Content = "@startuml\nclass Test\n@enduml" }
        };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);
        _mockDiscoveryService.Setup(d => d.DiscoverSchemasAsync(folderPath))
            .ReturnsAsync(discoveredSchemas);
        _mockStorageService.Setup(s => s.SaveSchemaAsync(It.IsAny<Schema>()))
            .ReturnsAsync((Schema s) => { s.Id = ObjectId.NewObjectId(); return s; });
        _mockStorageService.Setup(s => s.UpdateProjectAsync(It.IsAny<Project>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _schemaManager.DiscoverAndAddSchemasAsync(projectId, folderPath);

        // Assert
        result.Should().NotBeNull();
        result.SchemaIds.Should().HaveCount(1);

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.AtLeastOnce);
        _mockDiscoveryService.Verify(d => d.DiscoverSchemasAsync(folderPath), Times.Once);
        _mockStorageService.Verify(s => s.SaveSchemaAsync(It.IsAny<Schema>()), Times.Once);
        _mockStorageService.Verify(s => s.UpdateProjectAsync(It.IsAny<Project>()), Times.Once);
    }

    [Test]
    public async Task ParseFileAsync_WithValidFile_ShouldParseSchema()
    {
        // Arrange
        var filePath = "test.puml";
        var content = "@startuml\nclass Test\n@enduml";
        var schema = new Schema
        {
            Content = content,
            SourcePath = filePath
        };
        var parseResult = new ParseResult
        {
            IsSuccess = true,
            Schema = schema
        };

        _mockParser.Setup(p => p.ParseAsync(content))
            .ReturnsAsync(parseResult);

        // Create a temporary file for testing
        await File.WriteAllTextAsync(filePath, content);

        try
        {
            // Act
            var result = await _schemaManager.ParseFileAsync(filePath);

            // Assert
            result.Should().NotBeNull();
            result.Content.Should().Be(content);
            result.SourcePath.Should().Be(filePath);

            _mockParser.Verify(p => p.ParseAsync(content), Times.Once);
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Test]
    public async Task ParseFileAsync_WithNonExistentFile_ShouldThrowException()
    {
        // Arrange
        var filePath = "nonexistent.puml";

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(() => 
            _schemaManager.ParseFileAsync(filePath));
    }

    [Test]
    public async Task MergeSchemasAsync_WithMultipleSchemas_ShouldMergeContent()
    {
        // Arrange
        var schemas = new List<Schema>
        {
            new Schema { Content = "@startuml\nclass A\n@enduml" },
            new Schema { Content = "@startuml\nclass B\n@enduml" }
        };
        var mergedSchema = new Schema
        {
            Content = "@startuml\nclass A\n@enduml\n\n@startuml\nclass B\n@enduml",
            SourcePath = "merged"
        };
        var parseResult = new ParseResult
        {
            IsSuccess = true,
            Schema = mergedSchema
        };

        _mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync(parseResult);

        // Act
        var result = await _schemaManager.MergeSchemasAsync(schemas);

        // Assert
        result.Should().NotBeNull();
        result.SourcePath.Should().Be("merged");
        result.Content.Should().Contain("class A");
        result.Content.Should().Contain("class B");

        _mockParser.Verify(p => p.ParseAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GetProjectSchemasAsync_WithValidProject_ShouldReturnSchemas()
    {
        // Arrange
        var projectId = ObjectId.NewObjectId();
        var schemaId = ObjectId.NewObjectId();
        var project = new Project
        {
            Id = projectId,
            Name = "Test Project",
            SchemaIds = [schemaId]
        };
        var schema = new Schema
        {
            Id = schemaId,
            Content = "@startuml\nclass Test\n@enduml"
        };

        _mockStorageService.Setup(s => s.LoadProjectAsync(projectId))
            .ReturnsAsync(project);
        _mockStorageService.Setup(s => s.LoadSchemaAsync(schemaId))
            .ReturnsAsync(schema);

        // Act
        var result = await _schemaManager.GetProjectSchemasAsync(projectId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(schemaId);

        _mockStorageService.Verify(s => s.LoadProjectAsync(projectId), Times.Once);
        _mockStorageService.Verify(s => s.LoadSchemaAsync(schemaId), Times.Once);
    }
}
