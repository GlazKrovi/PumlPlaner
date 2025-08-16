using PumlPlaner.AST;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlanerTester;

public class ComplexDiagramTests
{
    [Test]
    public void ShouldHandleEnumDeclaration()
    {
        const string input = """
                             @startuml
                             enum Color {
                               RED
                               GREEN
                               BLUE
                             }
                             @enduml
                             """;


        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);


        Assert.That(result, Does.Contain("enum Color"));
        Assert.That(result, Does.Contain("RED"));
        Assert.That(result, Does.Contain("GREEN"));
        Assert.That(result, Does.Contain("BLUE"));
    }

    [Test]
    public void ShouldHandleAbstractClass()
    {
        const string input = """
                             @startuml
                             abstract class Animal {
                               # {abstract} name string
                               + {abstract} makeSound()
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        abstract class Animal {
                                                          # {abstract} name string
                                                          + {abstract} makeSound()
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Console.WriteLine(result);

        Assert.That(result, Is.EqualTo(expected));
    }


    [Test]
    public void ShouldHandleInterface()
    {
        const string input = """
                             @startuml
                             interface Drawable {
                               + draw()
                               + resize(int width, int height)
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        interface Drawable {
                                                          + draw()
                                                          + resize(int width, int height)
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleOverrideMethods()
    {
        const string input = """
                             @startuml
                             class Animal {
                               + {abstract} makeSound()
                             }
                             class Dog {
                               + {override} makeSound()
                               + {override} string toString()
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class Animal {
                                                          + {abstract} makeSound()
                                                        }
                                                        class Dog {
                                                          + {override} makeSound()
                                                          + {override} string toString()
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Console.WriteLine(result);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleSimpleInheritance()
    {
        const string input = """
                             @startuml
                             class Animal {
                               # name string
                               + makeSound()
                             }
                             class Dog {
                               + bark()
                             }
                             Animal <|-- Dog
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class Animal {
                                                          # name string
                                                          + makeSound()
                                                        }
                                                        class Dog {
                                                          + bark()
                                                        }
                                                        Animal <|-- Dog
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleSimpleComposition()
    {
        const string input = """
                             @startuml
                             class Car {
                               - engine Engine
                               + start()
                             }
                             class Engine {
                               - power int
                               + ignite()
                             }
                             Car *-- Engine
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class Car {
                                                          - engine Engine
                                                          + start()
                                                        }
                                                        class Engine {
                                                          - power int
                                                          + ignite()
                                                        }
                                                        Car *-- Engine
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleSimpleAssociation()
    {
        const string input = """
                             @startuml
                             class Student {
                               - name string
                             }
                             class Course {
                               - title string
                             }
                             Student -- Course
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class Student {
                                                          - name string
                                                        }
                                                        class Course {
                                                          - title string
                                                        }
                                                        Student -- Course
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleSimpleStereotype()
    {
        const string input = """
                             @startuml
                             class User <<entity>> {
                               - id int
                               - name string
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class User <<entity>> {
                                                          - id int
                                                          - name string
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleSimpleGenericType()
    {
        const string input = """
                             @startuml
                             class List {
                               - items : string[]
                               + add(item : string)
                               + remove(item : string)
                             }
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        class List {
                                                          - items : string[]
                                                          + add(item : string)
                                                          + remove(item : string)
                                                        }
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldDeduplicateComplexDiagram()
    {
        const string input = """
                             @startuml
                             enum Status {
                               ACTIVE
                               INACTIVE
                             }

                             class User {
                               - name string
                               + getName() string
                             }

                             class User {
                               - email string
                               + getEmail() string
                             }

                             User --> Order
                             @enduml
                             """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        enum Status {
                                                          ACTIVE
                                                          INACTIVE
                                                        }
                                                        class User {
                                                          - name string
                                                          - email string
                                                          + getName() string
                                                          + getEmail() string
                                                        }
                                                        User --> Order
                                                        @enduml

                                                        """);

        var ast = new SchemeAst(input);
        var deduplicator = new PumlDeduplicator();
        var result = deduplicator.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldMergeSimpleComplexDiagrams()
    {
        const string firstInput = """
                                  @startuml
                                  enum Status {
                                    ACTIVE
                                    INACTIVE
                                  }

                                  class User {
                                    - name string
                                    + getName() string
                                  }

                                  User --> Order
                                  @enduml
                                  """;

        const string secondInput = """
                                   @startuml
                                   class Order {
                                     - items List
                                     + addItem(OrderItem item)
                                   }

                                   class Product {
                                     - price decimal
                                     + getPrice() decimal
                                   }

                                   Order --> Product
                                   @enduml
                                   """;

        var expected = StringHelper.NormalizeBreakLines("""
                                                        @startuml
                                                        enum Status {
                                                          ACTIVE
                                                          INACTIVE
                                                        }
                                                        class User {
                                                          - name string
                                                          + getName() string
                                                        }
                                                        class Order {
                                                          - items List
                                                          + addItem(OrderItem item)
                                                        }
                                                        class Product {
                                                          - price decimal
                                                          + getPrice() decimal
                                                        }
                                                        User --> Order
                                                        Order --> Product
                                                        @enduml

                                                        """);

        var firstAst = new SchemeAst(firstInput);
        var secondAst = new SchemeAst(secondInput);

        var visitor = new PumlSum();
        var merged = visitor.VisitUml(firstAst.Tree, secondAst.Tree);

        Assert.That(merged, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleSimpleEnum()
    {
        const string input = """
                             @startuml
                             enum Color {
                               RED
                               GREEN
                               BLUE
                             }
                             @enduml
                             """;

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);


        Assert.That(result, Does.Contain("enum Color"));
        Assert.That(result, Does.Contain("RED"));
        Assert.That(result, Does.Contain("GREEN"));
        Assert.That(result, Does.Contain("BLUE"));
    }
}
