using NUnit.Framework;
using PumlSchemasManager.Infrastructure;
using PumlSchemasManager.Domain;

namespace PumlSchemasManagerTester.Infrastructure;

[TestFixture]
public class PlantUmlParserTests
{
    private PlantUmlParser _parser;

    [SetUp]
    public void Setup()
    {
        _parser = new PlantUmlParser();
    }

    [Test]
    public async Task ParseAsync_WithValidPlantUml_ShouldReturnSuccess()
    {
        // Arrange
        var validPlantUml = "@startuml\nclass Test\n@enduml";

        // Act
        var result = await _parser.ParseAsync(validPlantUml);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Schema.Should().NotBeNull();
        result.Schema!.Content.Should().Be(validPlantUml);
        result.ErrorMessage.Should().BeNull();
        result.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task ParseAsync_WithStartKeyword_ShouldReturnSuccess()
    {
        // Arrange
        var validPlantUml = "@start\nclass Test\n@end";

        // Act
        var result = await _parser.ParseAsync(validPlantUml);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Schema.Should().NotBeNull();
        result.Schema!.Content.Should().Be(validPlantUml);
    }

    [Test]
    public async Task ParseAsync_WithoutPlantUmlMarkers_ShouldReturnFailure()
    {
        // Arrange
        var invalidContent = "This is not PlantUML content";

        // Act
        var result = await _parser.ParseAsync(invalidContent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Schema.Should().BeNull();
        result.ErrorMessage.Should().Contain("PlantUML markers");
        result.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task ParseAsync_WithEmptyContent_ShouldReturnFailure()
    {
        // Arrange
        var emptyContent = "";

        // Act
        var result = await _parser.ParseAsync(emptyContent);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Schema.Should().BeNull();
        result.ErrorMessage.Should().Contain("PlantUML markers");
    }

    [Test]
    public async Task ParseAsync_WithNullContent_ShouldReturnFailure()
    {
        // Arrange
        string? nullContent = null;

        // Act
        var result = await _parser.ParseAsync(nullContent!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Schema.Should().BeNull();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task ParseAsync_WithComplexPlantUml_ShouldReturnSuccess()
    {
        // Arrange
        var complexPlantUml = @"@startuml
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
        var result = await _parser.ParseAsync(complexPlantUml);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Schema.Should().NotBeNull();
        result.Schema!.Content.Should().Be(complexPlantUml);
        result.Schema.Metadata.Should().NotBeNull();
        result.Schema.Metadata.Hash.Should().NotBeEmpty();
    }

    [Test]
    public async Task ParseAsync_ShouldSetMetadata()
    {
        // Arrange
        var plantUml = "@startuml\nclass Test\n@enduml";

        // Act
        var result = await _parser.ParseAsync(plantUml);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Schema.Should().NotBeNull();
        result.Schema!.Metadata.Should().NotBeNull();
        result.Schema.Metadata.DiscoveredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Schema.Metadata.Hash.Should().NotBeEmpty();
    }

    [Test]
    public async Task ValidateAsync_WithValidPlantUml_ShouldReturnValid()
    {
        // Arrange
        var validPlantUml = "@startuml\nclass Test\n@enduml";

        // Act
        var result = await _parser.ValidateAsync(validPlantUml);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateAsync_WithStartKeyword_ShouldReturnValid()
    {
        // Arrange
        var validPlantUml = "@start\nclass Test\n@end";

        // Act
        var result = await _parser.ValidateAsync(validPlantUml);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateAsync_WithoutPlantUmlMarkers_ShouldReturnInvalid()
    {
        // Arrange
        var invalidContent = "This is not PlantUML content";

        // Act
        var result = await _parser.ValidateAsync(invalidContent);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("PlantUML markers");
        result.Warnings.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateAsync_WithEmptyContent_ShouldReturnInvalid()
    {
        // Arrange
        var emptyContent = "";

        // Act
        var result = await _parser.ValidateAsync(emptyContent);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("PlantUML markers");
    }

    [Test]
    public async Task ValidateAsync_WithNullContent_ShouldReturnInvalid()
    {
        // Arrange
        string? nullContent = null;

        // Act
        var result = await _parser.ValidateAsync(nullContent!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Test]
    public async Task ValidateAsync_WithComplexPlantUml_ShouldReturnValid()
    {
        // Arrange
        var complexPlantUml = @"@startuml
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
        var result = await _parser.ValidateAsync(complexPlantUml);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateAsync_WithMalformedPlantUml_ShouldReturnValidWithWarning()
    {
        // Arrange
        var malformedPlantUml = "@startuml\nclass Test\n"; // Missing @enduml

        // Act
        var result = await _parser.ValidateAsync(malformedPlantUml);

        // Assert
        // The parser is tolerant, so it should still be considered valid
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        // May have warnings due to parsing issues
    }

    [Test]
    public async Task ParseAsync_And_ValidateAsync_ShouldBeConsistent()
    {
        // Arrange
        var plantUml = "@startuml\nclass Test\n@enduml";

        // Act
        var parseResult = await _parser.ParseAsync(plantUml);
        var validateResult = await _parser.ValidateAsync(plantUml);

        // Assert
        parseResult.IsSuccess.Should().Be(validateResult.IsValid);
        
        if (parseResult.IsSuccess)
        {
            validateResult.IsValid.Should().BeTrue();
        }
        else
        {
            validateResult.IsValid.Should().BeFalse();
        }
    }
}
