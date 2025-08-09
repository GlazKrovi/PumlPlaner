using Antlr4.Runtime;

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
        var input = CharStreams.fromString("@startuml\n\nclass Fruit {\n  - vitamins int\n  + eat()\n}\n\n@enduml\n");
        var lexer = new PumlgLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PumlgParser(tokens);
        var tree = parser.uml(); // 'diagram' = point d'entrée dans la grammaire

        Console.WriteLine(tree.ToStringTree());

        Assert.Pass();
    }
}