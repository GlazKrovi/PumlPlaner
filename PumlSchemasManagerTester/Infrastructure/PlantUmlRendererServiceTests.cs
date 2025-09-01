using PumlSchemasManager.Domain;
using PumlSchemasManager.Infrastructure;

namespace PumlSchemasManagerTester.Infrastructure;

[TestFixture]
public class PlantUmlRendererServiceTests
{
    [SetUp]
    public void Setup()
    {
        _renderer = new PlantUmlRendererService();
    }

    private PlantUmlRendererService _renderer;

    [Test]
    public async Task RenderAsync_WithValidSchema_ShouldReturnContent()
    {
        // Arrange
        var schema = new Schema
        {
            Content = "@startuml\nclass Test\n@enduml"
        };

        // Act
        var result = await _renderer.RenderAsync(schema, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task RenderContentAsync_WithValidContent_ShouldReturnContent()
    {
        // Arrange
        var content = "@startuml\nclass Test\n@enduml";

        // Act
        var result = await _renderer.RenderContentAsync(content, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task RenderContentAsync_WithDifferentFormats_ShouldReturnContent()
    {
        // Arrange
        var content = "@startuml\nclass Test\n@enduml";
        var formats = new[]
        {
            SchemaOutputFormat.Png,
            SchemaOutputFormat.Svg,
            SchemaOutputFormat.Pdf,
            SchemaOutputFormat.Html
        };

        // Act & Assert
        foreach (var format in formats)
        {
            var result = await _renderer.RenderContentAsync(content, format);
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task RenderContentAsync_WithEmptyContent_ShouldReturnContent()
    {
        // Arrange
        var emptyContent = "";

        // Act
        var result = await _renderer.RenderContentAsync(emptyContent, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task RenderContentAsync_WithNullContent_ShouldReturnContent()
    {
        // Arrange
        string? nullContent = null;

        // Act
        var result = await _renderer.RenderContentAsync(nullContent!, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Test]
    public void GetContentType_ShouldReturnCorrectTypes()
    {
        // Act & Assert
        _renderer.GetContentType(SchemaOutputFormat.Png).Should().Be("image/png");
        _renderer.GetContentType(SchemaOutputFormat.Svg).Should().Be("image/svg+xml");
        _renderer.GetContentType(SchemaOutputFormat.Pdf).Should().Be("application/pdf");
        _renderer.GetContentType(SchemaOutputFormat.Html).Should().Be("text/html");
        _renderer.GetContentType(SchemaOutputFormat.Txt).Should().Be("text/plain");
    }

    [Test]
    public void GetFileExtension_ShouldReturnCorrectExtensions()
    {
        // Act & Assert
        _renderer.GetFileExtension(SchemaOutputFormat.Png).Should().Be("png");
        _renderer.GetFileExtension(SchemaOutputFormat.Svg).Should().Be("svg");
        _renderer.GetFileExtension(SchemaOutputFormat.Pdf).Should().Be("pdf");
        _renderer.GetFileExtension(SchemaOutputFormat.Html).Should().Be("html");
        _renderer.GetFileExtension(SchemaOutputFormat.Txt).Should().Be("txt");
    }

    [Test]
    public void GetContentType_WithAllFormats_ShouldNotBeEmpty()
    {
        // Arrange
        var formats = Enum.GetValues<SchemaOutputFormat>();

        // Act & Assert
        foreach (var format in formats)
        {
            var contentType = _renderer.GetContentType(format);
            contentType.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void GetFileExtension_WithAllFormats_ShouldNotBeEmpty()
    {
        // Arrange
        var formats = Enum.GetValues<SchemaOutputFormat>();

        // Act & Assert
        foreach (var format in formats)
        {
            var extension = _renderer.GetFileExtension(format);
            extension.Should().NotBeNullOrEmpty();
            extension.Should().NotBeEmpty();
        }
    }

    [Test]
    public async Task RenderAsync_WithNullSchema_ShouldReturnContent()
    {
        // Arrange
        Schema? nullSchema = null;

        // Act
        var result = await _renderer.RenderAsync(nullSchema!, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().BeEquivalentTo(await _renderer.RenderContentAsync("", SchemaOutputFormat.Png));
    }

    [Test]
    public async Task RenderAsync_WithSchemaWithoutContent_ShouldReturnContent()
    {
        // Arrange
        var schema = new Schema { Content = "" };

        // Act
        var result = await _renderer.RenderAsync(schema, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task RenderContentAsync_WithComplexPlantUml_ShouldReturnContent()
    {
        // Arrange
        var complexContent = @"@startuml
title Complex Diagram

class User {
  +String name
  +String email
  +void login()
  +void logout()
}

class Order {
  +int id
  +DateTime date
  +double total
  +void process()
}

User --> Order : places
@enduml";

        // Act
        var result = await _renderer.RenderContentAsync(complexContent, SchemaOutputFormat.Png);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task RenderContentAsync_WithSameContentAndFormat_ShouldReturnConsistentResult()
    {
        // Arrange
        var content = "@startuml\nclass Test\n@enduml";
        var format = SchemaOutputFormat.Png;

        // Act
        var result1 = await _renderer.RenderContentAsync(content, format);
        var result2 = await _renderer.RenderContentAsync(content, format);

        // Assert
        result1.Should().BeEquivalentTo(result2);
    }

    [Test]
    public async Task RenderAsync_And_RenderContentAsync_ShouldBeConsistent()
    {
        // Arrange
        var schema = new Schema
        {
            Content = "@startuml\nclass Test\n@enduml"
        };
        var format = SchemaOutputFormat.Png;

        // Act
        var renderResult = await _renderer.RenderAsync(schema, format);
        var renderContentResult = await _renderer.RenderContentAsync(schema.Content, format);

        // Assert
        renderResult.Should().BeEquivalentTo(renderContentResult);
    }
}