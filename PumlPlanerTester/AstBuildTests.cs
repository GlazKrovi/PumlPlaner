using Antlr4.Runtime;
using PumlPlaner.AST;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlanerTester;

public class AstBuildTests
{
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

        var visitor = new PumlReconstructor();
        var reconstructed = visitor.VisitUml(tree);

        Console.WriteLine("Original:\r" + input);
        Console.WriteLine("Reconstructed:\r" + reconstructed);

        Assert.That(input.ToString() ?? string.Empty, Is.EqualTo(reconstructed));
    }


    [Test]
    public void ShouldRecreateFromSourceWithClass()
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
        var ast = new SchemeAst(input);

        var visitor = new PumlReconstructor();
        var reconstructed = visitor.VisitUml(ast.Tree);

        Assert.That(input.ToString() ?? string.Empty, Is.EqualTo(reconstructed));
    }
}