using Antlr4.Runtime;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlaner;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ShouldRecreateFromSource()
    {
        var rawInput = """
                       @startuml

                       class Fruit {
                         - vitamins int
                         + eat()
                       }

                       @enduml
                       """;
        rawInput = StringHelper.NormalizeBreakLines(rawInput);
        rawInput = StringHelper.RemoveMultipleBreaks(rawInput);
        rawInput = StringHelper.NormalizeEndOfFile(rawInput);
        var input = CharStreams.fromString(rawInput);

        // process
        PumlgParser.UmlContext tree;
        try
        {
            var lexer = new PumlgLexer(input);
            var tokens = new CommonTokenStream(lexer);
            var parser = new PumlgParser(tokens);
            tree = parser.uml();
        }
        catch (Exception e)
        {
            Console.WriteLine("AST build error: " + e);
            throw;
        }

        var visitor = new PlantUmlReconstructor();
        var reconstructed = visitor.VisitUml(tree);

        Console.WriteLine("Original:\r" + input);
        Console.WriteLine("Reconstructed:\r" + reconstructed);

        Assert.That(input.ToString() ?? string.Empty, Is.EqualTo(reconstructed));
    }
}