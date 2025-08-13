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
    public void Test1()
    {
        var input = CharStreams.fromString( StringHelper.NormalizeBreakLines("@startuml\r\rclass Fruit {\r  - vitamins int\r  + eat()\r}\r\r@enduml\r"));

        // process
        var lexer = new PumlgLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PumlgParser(tokens);
        var tree = parser.uml();

        var visitor = new PlantUmlReconstructor();
        var reconstructed = visitor.Visit(tree);

        Console.WriteLine("Original:\r" + input);
        Console.WriteLine("Reconstructed:\r" + reconstructed);

        Assert.That(input.ToString() ?? string.Empty,  Is.EqualTo(reconstructed));
        Assert.Pass();
    }
}