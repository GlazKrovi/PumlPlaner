using PumlPlaner.AST;
using PumlPlaner.Visitors;

namespace PumlPlaner.Tests;

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

        const string expected = """
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

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldHandleAbstractClass()
    {
        const string input = """
                             @startuml
                             abstract class Animal {
                               # name string
                               + {abstract} makeSound()
                             }
                             @enduml
                             """;

        const string expected = """
                                @startuml
                                abstract class Animal {
                                  # name string
                                  + {abstract} makeSound()
                                }
                                @enduml
                                
                                """;

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

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

        const string expected = """
                                @startuml
                                interface Drawable {
                                  + draw()
                                  + resize(int width, int height)
                                }
                                @enduml
                                
                                """;

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

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

        const string expected = """
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

        const string expected = """
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

        const string expected = """
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

        const string expected = """
                                @startuml
                                class User <<entity>> {
                                  - id int
                                  - name string
                                }
                                @enduml
                                
                                """;

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
                               - items T[]
                               + add(T item)
                               + remove(T item)
                             }
                             @enduml
                             """;

        const string expected = """
                                @startuml
                                class List {
                                  - items T[]
                                  + add(T item)
                                  + remove(T item)
                                }
                                @enduml
                                
                                """;

        var ast = new SchemeAst(input);
        var visitor = new PumlReconstructor();
        var result = visitor.VisitUml(ast.Tree);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ShouldDeduplicateSimpleComplexDiagram()
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

        const string expected = """
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
                                
                                """;

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

        const string expected = """
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
                                
                                """;

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

        // Just check that the result contains the expected elements
        Assert.That(result, Does.Contain("enum Color"));
        Assert.That(result, Does.Contain("RED"));
        Assert.That(result, Does.Contain("GREEN"));
        Assert.That(result, Does.Contain("BLUE"));
    }
}
