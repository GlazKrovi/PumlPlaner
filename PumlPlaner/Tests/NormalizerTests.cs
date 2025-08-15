using PumlPlaner.Helpers;

namespace PumlPlaner.Tests;

public class StringHelperTests
{
    [Test]
    public void NormalizeBreakLines_ShouldReplaceWindowsAndMacBreaksWithUnix()
    {
        // Arrange
        const string input = "Line1\rLine2\rLine3";
        const string expected = "Line1\nLine2\nLine3";

        // Act
        var result = StringHelper.NormalizeBreakLines(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void RemoveMultipleBreaks_ShouldReduceToSingleBreak()
    {
        // Arrange
        const string input = "Line1\n\n\nLine2\n\nLine3";
        const string expected = "Line1\nLine2\nLine3";

        // Act
        var result = StringHelper.RemoveMultipleBreaks(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void NormalizeEndOfFile_ShouldAppendNewlineIfMissing()
    {
        // Arrange
        const string inputWithoutNewline = "Last line";
        const string inputWithNewline = "Last line\n";

        // Act
        var result1 = StringHelper.NormalizeEndOfFile(inputWithoutNewline);
        var result2 = StringHelper.NormalizeEndOfFile(inputWithNewline);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result1, Is.EqualTo("Last line\n"), "Doit ajouter un saut de ligne si absent");
            Assert.That(result2, Is.EqualTo(inputWithNewline), "Ne doit pas doubler le saut de ligne");
        });
    }

    [Test]
    public void ShouldNormalizeEntries()
    {
        const string rawInput = """
                                @startuml






                                class Fruit {
                                \r
                                  - vitamins int
                                  
                                  
                                  
                                  
                                  + eat()
                                }

                                @enduml
                                """;

        var normalized = new NormalizedInput(rawInput).ToCharStream();

        const string expectedInput = """
                                     @startuml
                                     class Fruit {
                                       - vitamins int
                                       + eat()
                                     }
                                     @enduml
                                     
                                     """;

        Assert.That(normalized.ToString(), Is.Not.EqualTo(rawInput));
        Assert.That(normalized.ToString(), Is.EqualTo(expectedInput));
    }
}