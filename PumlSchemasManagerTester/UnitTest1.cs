using NUnit.Framework;
using PumlSchemasManager;
using System.IO;

namespace PumlSchemasManagerTester;

public class Tests
{
    private string _testGeneratedPath;
    private Parser _parser;
    private Parse _parseCommand;

    [SetUp]
    public void Setup()
    {
        _parser = new Parser();
        _parseCommand = new Parse();
        _testGeneratedPath = _parser.GetGeneratedPath();
    }

    [TearDown]
    public void Cleanup()
    {
        // Nettoyer les fichiers de test générés
        if (Directory.Exists(_testGeneratedPath))
        {
            var files = Directory.GetFiles(_testGeneratedPath, "*.png");
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch { /* Ignore cleanup errors */ }
            }
        }
    }

    [Test]
    public void Parser_Constructor_ShouldCreateGeneratedDirectory()
    {
        // Act
        var parser = new Parser();
        
        // Assert
        Assert.That(Directory.Exists(_testGeneratedPath), Is.True, "Le dossier 'generated' devrait être créé");
    }

    [Test]
    public void Parser_GetGeneratedPath_ShouldReturnValidPath()
    {
        // Act
        string path = _parser.GetGeneratedPath();
        
        // Assert
        Assert.That(path, Is.Not.Null);
        Assert.That(path, Is.Not.Empty);
        Assert.That(path.Contains("generated"), Is.True, "Le chemin devrait contenir 'generated'");
    }

    [Test]
    public void Parser_Parse_ShouldGeneratePngFile()
    {
        // Arrange
        string plantUmlCode = "@startuml\nclass Test\n@enduml";
        
        // Act
        string resultPath = _parser.Parse(plantUmlCode);
        
        // Assert
        Assert.That(resultPath, Is.Not.Null);
        Assert.That(File.Exists(resultPath), Is.True, "Le fichier PNG devrait être créé");
        Assert.That(Path.GetExtension(resultPath), Is.EqualTo(".png"), "L'extension devrait être .png");
        Assert.That(Path.GetDirectoryName(resultPath), Is.EqualTo(_testGeneratedPath), "Le fichier devrait être dans le dossier generated");
    }

    [Test]
    public void Parser_Parse_ShouldGenerateUniqueFileNames()
    {
        // Arrange
        string plantUmlCode = "@startuml\nclass Test\n@enduml";
        
        // Act
        string path1 = _parser.Parse(plantUmlCode);
        string path2 = _parser.Parse(plantUmlCode);
        
        // Assert
        Assert.That(path1, Is.Not.EqualTo(path2), "Les noms de fichiers devraient être uniques");
        Assert.That(File.Exists(path1), Is.True);
        Assert.That(File.Exists(path2), Is.True);
    }

    [Test]
    public void Parser_Parse_ShouldHandleEmptySource()
    {
        // Arrange
        string emptySource = "";
        
        // Act & Assert
        Assert.DoesNotThrow(() => _parser.Parse(emptySource), "Parser devrait gérer une source vide");
    }

    [Test]
    public void Parser_Parse_ShouldHandleInvalidPlantUml()
    {
        // Arrange
        string invalidPlantUml = "invalid plantuml code";
        
        // Act & Assert
        Assert.DoesNotThrow(() => _parser.Parse(invalidPlantUml), "Parser devrait gérer du code PlantUML invalide");
    }

    [Test]
    public void Parse_From_ShouldNotThrowException()
    {
        // Arrange
        string testFile = "test.puml";
        string plantUmlContent = "@startuml\nclass Test\n@enduml";
        File.WriteAllText(testFile, plantUmlContent);
        
        try
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _parseCommand.From(testFile), "Parse.From ne devrait pas lever d'exception");
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Test]
    public void Parse_From_ShouldHandleNonExistentFile()
    {
        // Arrange
        string nonExistentFile = "non_existent_file.puml";
        
        // Act & Assert
        Assert.DoesNotThrow(() => _parseCommand.From(nonExistentFile), "Parse.From devrait gérer un fichier inexistant");
    }

    [Test]
    public void Parser_Parse_ShouldReturnValidPngContent()
    {
        // Arrange
        string plantUmlCode = "@startuml\nclass Test\n@enduml";
        
        // Act
        string resultPath = _parser.Parse(plantUmlCode);
        
        // Assert
        Assert.That(File.Exists(resultPath), Is.True);
        
        // Vérifier que le fichier contient des données PNG
        byte[] fileBytes = File.ReadAllBytes(resultPath);
        Assert.That(fileBytes.Length, Is.GreaterThan(0), "Le fichier PNG ne devrait pas être vide");
        
        // Vérifier la signature PNG (les premiers bytes)
        if (fileBytes.Length >= 8)
        {
            byte[] pngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            for (int i = 0; i < 8; i++)
            {
                Assert.That(fileBytes[i], Is.EqualTo(pngSignature[i]), $"Byte {i} devrait correspondre à la signature PNG");
            }
        }
    }
}
