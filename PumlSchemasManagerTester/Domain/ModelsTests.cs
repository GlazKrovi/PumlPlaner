using LiteDB;
using PumlSchemasManager.Domain;

namespace PumlSchemasManagerTester.Domain;

[TestFixture]
public class ModelsTests
{
    [Test]
    public void Schema_Constructor_ShouldInitializeProperties()
    {
        // Act
        var schema = new Schema();

        // Assert
        schema.Id.Should().Be(ObjectId.Empty);
        schema.SourcePath.Should().BeEmpty();
        schema.Content.Should().BeEmpty();
        schema.GeneratedFiles.Should().NotBeNull();
        schema.GeneratedFiles.Should().BeEmpty();
        schema.Metadata.Should().NotBeNull();
        schema.ProjectId.Should().Be(ObjectId.Empty);
    }

    [Test]
    public void Schema_Properties_ShouldBeSettable()
    {
        // Arrange
        var schema = new Schema();
        var projectId = ObjectId.NewObjectId();
        var generatedFile = new GeneratedFile();

        // Act
        schema.SourcePath = "test.puml";
        schema.Content = "@startuml\nclass Test\n@enduml";
        schema.ProjectId = projectId;
        schema.GeneratedFiles.Add(generatedFile);

        // Assert
        schema.SourcePath.Should().Be("test.puml");
        schema.Content.Should().Be("@startuml\nclass Test\n@enduml");
        schema.ProjectId.Should().Be(projectId);
        schema.GeneratedFiles.Should().HaveCount(1);
        schema.GeneratedFiles.Should().Contain(generatedFile);
    }

    [Test]
    public void Project_Constructor_ShouldInitializeProperties()
    {
        // Act
        var project = new Project();

        // Assert
        project.Id.Should().Be(ObjectId.Empty);
        project.Name.Should().BeEmpty();
        project.SchemaIds.Should().NotBeNull();
        project.SchemaIds.Should().BeEmpty();
        project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Project_Properties_ShouldBeSettable()
    {
        // Arrange
        var project = new Project();
        var schemaId = ObjectId.NewObjectId();

        // Act
        project.Name = "Test Project";
        project.SchemaIds.Add(schemaId);

        // Assert
        project.Name.Should().Be("Test Project");
        project.SchemaIds.Should().HaveCount(1);
        project.SchemaIds.Should().Contain(schemaId);
    }

    [Test]
    public void GeneratedFile_Constructor_ShouldInitializeProperties()
    {
        // Act
        var generatedFile = new GeneratedFile();

        // Assert
        generatedFile.Id.Should().Be(ObjectId.Empty);
        generatedFile.SchemaId.Should().Be(ObjectId.Empty);
        generatedFile.FilePath.Should().BeEmpty();
        generatedFile.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        generatedFile.FileSize.Should().Be(0);
    }

    [Test]
    public void GeneratedFile_Properties_ShouldBeSettable()
    {
        // Arrange
        var generatedFile = new GeneratedFile();
        var schemaId = ObjectId.NewObjectId();

        // Act
        generatedFile.SchemaId = schemaId;
        generatedFile.Format = SchemaOutputFormat.Png;
        generatedFile.FilePath = "test.png";
        generatedFile.FileSize = 1024;

        // Assert
        generatedFile.SchemaId.Should().Be(schemaId);
        generatedFile.Format.Should().Be(SchemaOutputFormat.Png);
        generatedFile.FilePath.Should().Be("test.png");
        generatedFile.FileSize.Should().Be(1024);
    }

    [Test]
    public void SchemaMetadata_Constructor_ShouldInitializeProperties()
    {
        // Act
        var metadata = new SchemaMetadata();

        // Assert
        metadata.DiscoveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        metadata.OriginalPath.Should().BeEmpty();
        metadata.Hash.Should().BeEmpty();
        metadata.FileSize.Should().Be(0);
        metadata.LastModified.Should().Be(default);
    }

    [Test]
    public void SchemaMetadata_Properties_ShouldBeSettable()
    {
        // Arrange
        var metadata = new SchemaMetadata();
        var now = DateTime.UtcNow;

        // Act
        metadata.OriginalPath = "original.puml";
        metadata.Hash = "abc123";
        metadata.FileSize = 2048;
        metadata.LastModified = now;

        // Assert
        metadata.OriginalPath.Should().Be("original.puml");
        metadata.Hash.Should().Be("abc123");
        metadata.FileSize.Should().Be(2048);
        metadata.LastModified.Should().Be(now);
    }

    [Test]
    public void FileMetadata_Constructor_ShouldInitializeProperties()
    {
        // Act
        var metadata = new FileMetadata();

        // Assert
        metadata.FileName.Should().BeEmpty();
        metadata.ContentType.Should().BeEmpty();
        metadata.FileSize.Should().Be(0);
        metadata.StoredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        metadata.AdditionalMetadata.Should().NotBeNull();
        metadata.AdditionalMetadata.Should().BeEmpty();
    }

    [Test]
    public void FileMetadata_Properties_ShouldBeSettable()
    {
        // Arrange
        var metadata = new FileMetadata();

        // Act
        metadata.FileName = "test.png";
        metadata.ContentType = "image/png";
        metadata.FileSize = 1024;
        metadata.AdditionalMetadata["key"] = "value";

        // Assert
        metadata.FileName.Should().Be("test.png");
        metadata.ContentType.Should().Be("image/png");
        metadata.FileSize.Should().Be(1024);
        metadata.AdditionalMetadata.Should().HaveCount(1);
        metadata.AdditionalMetadata["key"].Should().Be("value");
    }

    [Test]
    public void ParseResult_Constructor_ShouldInitializeProperties()
    {
        // Act
        var result = new ParseResult();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
        result.Schema.Should().BeNull();
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().BeEmpty();
    }

    [Test]
    public void ValidationResult_Constructor_ShouldInitializeProperties()
    {
        // Act
        var result = new ValidationResult();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().NotBeNull();
        result.Warnings.Should().BeEmpty();
    }

    [Test]
    public void SchemaOutputFormat_ShouldHaveExpectedValues()
    {
        // Assert
        Enum.GetValues<SchemaOutputFormat>().Should().Contain(SchemaOutputFormat.Png);
        Enum.GetValues<SchemaOutputFormat>().Should().Contain(SchemaOutputFormat.Svg);
        Enum.GetValues<SchemaOutputFormat>().Should().Contain(SchemaOutputFormat.Pdf);
        Enum.GetValues<SchemaOutputFormat>().Should().Contain(SchemaOutputFormat.Html);
    }
}