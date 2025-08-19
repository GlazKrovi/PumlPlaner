using PumlPlaner.AST;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlanerTester;

public class InheritKeywordsTest
{
    [Test]
    public void ShouldHandleExtendsKeyword()
    {
        const string input = """
                             @startuml
                             class Animal {
                               + makeSound()
                             }

                             class Dog extends Animal {
                               + bark()
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class Animal {
                                                          + makeSound()
                                                        }
                                                        class Dog extends Animal {
                                                          + bark()
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Console.WriteLine("Input:");
        Console.WriteLine(input);
        Console.WriteLine("\nResult:");
        Console.WriteLine(result);
        Console.WriteLine("\nExpected:");
        Console.WriteLine(expected);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleImplementsKeyword()
    {
        const string input = """
                             @startuml
                             interface Drawable {
                               + draw()
                             }

                             class Circle implements Drawable {
                               - radius double
                               + draw()
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        interface Drawable {
                                                          + draw()
                                                        }
                                                        class Circle implements Drawable {
                                                          - radius double
                                                          + draw()
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Console.WriteLine("Input:");
        Console.WriteLine(input);
        Console.WriteLine("\nResult:");
        Console.WriteLine(result);
        Console.WriteLine("\nExpected:");
        Console.WriteLine(expected);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleMultipleInheritance()
    {
        const string input = """
                             @startuml
                             interface Flyable {
                               + fly()
                             }

                             interface Swimmable {
                               + swim()
                             }

                             class Duck extends Animal implements Flyable, Swimmable {
                               + fly()
                               + swim()
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        interface Flyable {
                                                          + fly()
                                                        }
                                                        interface Swimmable {
                                                          + swim()
                                                        }
                                                        class Duck extends Animal implements Flyable, Swimmable {
                                                          + fly()
                                                          + swim()
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Console.WriteLine("Input:");
        Console.WriteLine(input);
        Console.WriteLine("\nResult:");
        Console.WriteLine(result);
        Console.WriteLine("\nExpected:");
        Console.WriteLine(expected);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleAbstractClassWithExtends()
    {
        const string input = """
                             @startuml
                             abstract class Vehicle {
                               + {abstract} move()
                             }

                             class Car extends Vehicle {
                               + move()
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        abstract class Vehicle {
                                                          + {abstract} move()
                                                        }
                                                        class Car extends Vehicle {
                                                          + move()
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Console.WriteLine("Input:");
        Console.WriteLine(input);
        Console.WriteLine("\nResult:");
        Console.WriteLine(result);
        Console.WriteLine("\nExpected:");
        Console.WriteLine(expected);

        Assert.That(result, Is.EqualTo(expected));
    }
}