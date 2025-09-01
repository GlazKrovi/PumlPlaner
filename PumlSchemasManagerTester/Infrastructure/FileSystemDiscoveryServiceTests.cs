using PumlSchemasManager.Core;
using PumlSchemasManager.Domain;
using PumlSchemasManager.Infrastructure;

namespace PumlSchemasManagerTester.Infrastructure;

[TestFixture]
public class FileSystemDiscoveryServiceTests : IDisposable
{
    private FileSystemDiscoveryService _discoveryService;
    private Mock<IParser> _mockParser;
    private string _testDirectory;

    [SetUp]
    public void Setup()
    {
        _mockParser = new Mock<IParser>();
        
        // Setup the mock parser to return successful results for PlantUML content
        _mockParser.Setup(p => p.ParseAsync(It.IsAny<string>()))
            .Returns<string>(content =>
            {
                if (content.Contains("@startuml") || content.Contains("@start"))
                {
                    var schema = new Schema
                    {
                        Content = content,
                        Metadata = new SchemaMetadata
                        {
                            DiscoveredAt = DateTime.UtcNow,
                            Hash = "test-hash"
                        }
                    };
                    
                    return Task.FromResult(new ParseResult
                    {
                        IsSuccess = true,
                        Schema = schema,
                        Warnings = []
                    });
                }

                return Task.FromResult(new ParseResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Content does not contain PlantUML markers",
                    Warnings = []
                });
            });
        
        _discoveryService = new FileSystemDiscoveryService(_mockParser.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_discovery_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithPlantUmlFiles_ShouldReturnSchemas()
    {
        // Arrange
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var filePath1 = Path.Combine(_testDirectory, "test1.puml");
        var filePath2 = Path.Combine(_testDirectory, "test2.puml");
        
        await File.WriteAllTextAsync(filePath1, plantUmlContent);
        await File.WriteAllTextAsync(filePath2, plantUmlContent);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(2);
        schemas.Should().Contain(s => s.SourcePath == filePath1);
        schemas.Should().Contain(s => s.SourcePath == filePath2);
        schemas.Should().OnlyContain(s => s.Content == plantUmlContent);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithMixedFiles_ShouldOnlyReturnPlantUmlFiles()
    {
        // Arrange
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var otherContent = "This is not PlantUML";
        
        var pumlFile = Path.Combine(_testDirectory, "test.puml");
        var txtFile = Path.Combine(_testDirectory, "test.txt");
        var otherFile = Path.Combine(_testDirectory, "test.other");
        
        await File.WriteAllTextAsync(pumlFile, plantUmlContent);
        await File.WriteAllTextAsync(txtFile, otherContent);
        await File.WriteAllTextAsync(otherFile, otherContent);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(1);
        schemas.Should().Contain(s => s.SourcePath == pumlFile);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithSubdirectories_ShouldSearchRecursively()
    {
        // Arrange
        var subDir = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(subDir);
        
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var filePath1 = Path.Combine(_testDirectory, "test1.puml");
        var filePath2 = Path.Combine(subDir, "test2.puml");
        
        await File.WriteAllTextAsync(filePath1, plantUmlContent);
        await File.WriteAllTextAsync(filePath2, plantUmlContent);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(2);
        schemas.Should().Contain(s => s.SourcePath == filePath1);
        schemas.Should().Contain(s => s.SourcePath == filePath2);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithEmptyDirectory_ShouldReturnEmptyList()
    {
        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().BeEmpty();
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithNonExistentDirectory_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "non_existent");

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(nonExistentDir);

        // Assert
        schemas.Should().BeEmpty();
    }

    [Test]
    public async Task DiscoverSchemasAsync_ShouldSetMetadata()
    {
        // Arrange
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var filePath = Path.Combine(_testDirectory, "test.puml");
        await File.WriteAllTextAsync(filePath, plantUmlContent);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(1);
        var schema = schemas.First();
        schema.Metadata.Should().NotBeNull();
        schema.Metadata.OriginalPath.Should().Be(filePath);
        schema.Metadata.DiscoveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        schema.Metadata.Hash.Should().NotBeEmpty();
        schema.Metadata.FileSize.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithLargeFile_ShouldHandleCorrectly()
    {
        // Arrange
        var largeContent = new string('x', 10000) + "\n@startuml\nclass Test\n@enduml";
        var filePath = Path.Combine(_testDirectory, "large.puml");
        await File.WriteAllTextAsync(filePath, largeContent);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(1);
        var schema = schemas.First();
        schema.Content.Should().Be(largeContent);
        schema.Metadata.FileSize.Should().Be(largeContent.Length);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithFileContainingPlantUmlMarkers_ShouldBeDetected()
    {
        // Arrange
        var contentWithMarkers = "Some text\n@startuml\nclass Test\n@enduml\nMore text";
        var filePath = Path.Combine(_testDirectory, "mixed.puml");
        await File.WriteAllTextAsync(filePath, contentWithMarkers);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(1);
        var schema = schemas.First();
        schema.Content.Should().Be(contentWithMarkers);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithFileWithoutPlantUmlMarkers_ShouldNotBeDetected()
    {
        // Arrange
        var contentWithoutMarkers = "This file has .puml extension but no PlantUML content";
        var filePath = Path.Combine(_testDirectory, "fake.puml");
        await File.WriteAllTextAsync(filePath, contentWithoutMarkers);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().BeEmpty();
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithStartKeyword_ShouldBeDetected()
    {
        // Arrange
        var contentWithStart = "@start\nclass Test\n@end";
        var filePath = Path.Combine(_testDirectory, "test.puml");
        await File.WriteAllTextAsync(filePath, contentWithStart);

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        schemas.Should().HaveCount(1);
        var schema = schemas.First();
        schema.Content.Should().Be(contentWithStart);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithUnreadableFile_ShouldSkipFile()
    {
        // Arrange
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var readableFile = Path.Combine(_testDirectory, "readable.puml");
        await File.WriteAllTextAsync(readableFile, plantUmlContent);

        // Create a file that will be unreadable by using a directory path instead
        var unreadableFile = Path.Combine(_testDirectory, "unreadable.puml");
        Directory.CreateDirectory(unreadableFile); // This creates a directory with the same name as the file

        // Act
        var schemas = await _discoveryService.DiscoverSchemasAsync(_testDirectory);

        // Assert
        // Should at least find the readable file
        schemas.Should().Contain(s => s.SourcePath == readableFile);
    }

    [Test]
    public async Task DiscoverSchemasAsync_WithConcurrentAccess_ShouldHandleCorrectly()
    {
        // Arrange
        var plantUmlContent = "@startuml\nclass Test\n@enduml";
        var files = new List<string>();
        
        // Create multiple files
        for (int i = 0; i < 10; i++)
        {
            var filePath = Path.Combine(_testDirectory, $"test{i}.puml");
            await File.WriteAllTextAsync(filePath, plantUmlContent);
            files.Add(filePath);
        }

        // Act
        var tasks = new List<Task<List<Schema>>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_discoveryService.DiscoverSchemasAsync(_testDirectory));
        }
        
        var results = await Task.WhenAll(tasks);

        // Assert
        foreach (var result in results)
        {
            result.Should().HaveCount(10);
            result.Should().OnlyContain(s => s.Content == plantUmlContent);
        }
    }
}
