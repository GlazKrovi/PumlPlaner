using PumlPlaner.AST;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlanerTester;

public class PlantUmlDeduplicatorTests
{
    [Test]
    public void ShouldRemoveExactDuplicates()
    {
        const string input = """
                             @startuml
                             class Foo {
                               + bar()
                             }
                             class Foo {
                               + bar()
                             }
                             @enduml

                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                 @startuml
                                 class Foo {
                                   + bar()
                                 }
                                 @enduml

                                 """);

        var ast = new SchemeAst(input);
        var deduplicator = new PumlDeduplicator();
        var result = deduplicator.VisitUml(ast.Tree);

        Console.WriteLine("expected: ");
        Console.WriteLine(expected);
        Console.WriteLine("----------------");
        Console.WriteLine("Visitor's result: ");
        Console.WriteLine(result);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldMergeClassesWithDifferentMethods()
    {
        const string input = """
                             @startuml
                             class Foo {
                               + bar()
                             }
                             class Foo {
                               + baz()
                             }
                             @enduml

                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                 @startuml
                                 class Foo {
                                   + bar()
                                   + baz()
                                 }
                                 @enduml

                                 """);

        var ast = new SchemeAst(input);
        var deduplicator = new PumlDeduplicator();
        var result = deduplicator.VisitUml(ast.Tree);

        Console.WriteLine("expected: ");
        Console.WriteLine(expected);
        Console.WriteLine("----------------");
        Console.WriteLine("Visitor's result: ");
        Console.WriteLine(result);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleMethodOverloadsWithDifferences()
    {
        const string input = """
                             @startuml
                             class Foo {
                               + bar(int a)
                               + bar(string b)
                               + bar(int a, string b)
                             }
                             class Foo {
                               + bar(int a)
                               + bar()
                             }
                             @enduml

                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                 @startuml
                                 class Foo {
                                   + bar(int a)
                                   + bar(string b)
                                   + bar(int a, string b)
                                   + bar()
                                 }
                                 @enduml

                                 """);

        var ast = new SchemeAst(input);
        var deduplicator = new PumlDeduplicator();
        var result = deduplicator.VisitUml(ast.Tree);

        Console.WriteLine("expected: ");
        Console.WriteLine(expected);
        Console.WriteLine("----------------");
        Console.WriteLine("Visitor's result: ");
        Console.WriteLine(result);

        Assert.That(result, Is.EqualTo(expected));
    }
}
