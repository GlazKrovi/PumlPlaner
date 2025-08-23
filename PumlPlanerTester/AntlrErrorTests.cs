using PumlPlaner.AST;
using PumlPlaner.Visitors;

namespace PumlPlanerTester;

[TestFixture]
public class AntlrErrorTests
{
    [Test]
    public void Should_Detect_Syntax_Errors_In_Silent_Mode()
    {
        // Arrange
        const string invalidPuml = """

                                   @startuml
                                   class TestClass {
                                       +invalid syntax here
                                       -missing type
                                   }
                                   @enduml
                                   """;

        // Act
        var ast = new SchemeAst(invalidPuml);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(reconstructor.HasErrors);
            Assert.That(reconstructor.Errors, Is.Not.Empty);
            Assert.That(reconstructor.Errors.Any(e => e.Contains("Erreur de syntaxe")));
            Assert.That(string.IsNullOrEmpty(result), Is.EqualTo(false));
        });
    }

    [Test]
    public void Should_Throw_Exception_In_Strict_Mode()
    {
        // Arrange
        const string invalidPuml = """

                                   @startuml
                                   class BrokenClass {
                                       +invalid syntax here
                                   }
                                   @enduml
                                   """;

        // Act & Assert
        var ast = new SchemeAst(invalidPuml);
        var reconstructor = new PumlReconstructor(true);

        Assert.Throws<PumlReconstructionException>(() => reconstructor.Visit(ast.Tree));
    }

    [Test]
    public void Should_Handle_Valid_Puml_Without_Errors()
    {
        // Arrange
        const string validPuml = """

                                 @startuml
                                 class ValidClass {
                                     +String name
                                     -int age
                                     #calculate(int value) : int
                                 }
                                 @enduml
                                 """;

        // Act
        var ast = new SchemeAst(validPuml);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors, Is.False);
            Assert.That(reconstructor.Errors, Is.Empty);
            Assert.That(result, Does.Contain("class ValidClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
            Assert.That(result, Does.Contain("#calculate(int value) : int"));
        });
    }

    [Test]
    public void Should_Collect_Multiple_Errors()
    {
        // Arrange
        var pumlWithMultipleErrors = """

                                     @startuml
                                     class Class1 {
                                         +invalid syntax
                                     }

                                     class Class2 {
                                         -missing type
                                         #broken method syntax
                                     }

                                     Class1 --> Class2 : uses
                                     @enduml
                                     """;

        // Act
        var ast = new SchemeAst(pumlWithMultipleErrors);
        var reconstructor = new PumlReconstructor(false);
        reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(reconstructor.Errors, Has.Count.GreaterThanOrEqualTo(2));
            Assert.That(reconstructor.Errors.Any(e => e.Contains("Erreur de syntaxe")));
        });
    }

    [Test]
    public void Should_Handle_Missing_Class_Members()
    {
        // Arrange
        const string pumlWithMissingMembers = """

                                              @startuml
                                              class TestClass {
                                                  +String name
                                                  -int age
                                                  #calculate() : int
                                                  +invalid member
                                              }
                                              @enduml
                                              """;

        // Act
        var ast = new SchemeAst(pumlWithMissingMembers);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
            Assert.That(result, Does.Contain("#calculate() : int"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Connections()
    {
        // Arrange
        var pumlWithInvalidConnection = """

                                        @startuml
                                        class Class1 {
                                            +String name
                                        }

                                        class Class2 {
                                            +int value
                                        }

                                        Class1 --> Class2 : invalid connection syntax
                                        @enduml
                                        """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidConnection);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class Class1"));
            Assert.That(result, Does.Contain("class Class2"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Enum_Declaration()
    {
        // Arrange
        var pumlWithInvalidEnum = """

                                  @startuml
                                  enum TestEnum {
                                      VALID_ITEM
                                      invalid item syntax
                                      ANOTHER_VALID
                                  }
                                  @enduml
                                  """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidEnum);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("enum TestEnum"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Inheritance()
    {
        // Arrange
        var pumlWithInvalidInheritance = """

                                         @startuml
                                         class BaseClass {
                                             +String name
                                         }

                                         class DerivedClass extends BaseClass implements InvalidInterface {
                                             +int value
                                         }
                                         @enduml
                                         """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidInheritance);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class BaseClass"));
            Assert.That(result, Does.Contain("class DerivedClass"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Template_Parameters()
    {
        // Arrange
        var pumlWithInvalidTemplates = """

                                       @startuml
                                       class GenericClass<T, U> {
                                           +T item
                                           +U value
                                           +invalid template syntax
                                       }
                                       @enduml
                                       """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidTemplates);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class GenericClass"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Stereotypes()
    {
        // Arrange
        var pumlWithInvalidStereotype = """

                                        @startuml
                                        class TestClass <<invalid stereotype syntax>> {
                                            +String name
                                        }
                                        @enduml
                                        """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidStereotype);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Method_Arguments()
    {
        // Arrange
        var pumlWithInvalidMethod = """

                                    @startuml
                                    class TestClass {
                                        +String name
                                        +calculate(invalid argument syntax) : int
                                    }
                                    @enduml
                                    """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidMethod);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String name"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Attribute_Declaration()
    {
        // Arrange
        var pumlWithInvalidAttribute = """

                                       @startuml
                                       class TestClass {
                                           +String name
                                           +invalid attribute syntax
                                           -int age
                                       }
                                       @enduml
                                       """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidAttribute);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Visibility_Modifiers()
    {
        // Arrange
        var pumlWithInvalidVisibility = """

                                        @startuml
                                        class TestClass {
                                            *String invalidVisibility
                                            +String validVisibility
                                        }
                                        @enduml
                                        """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidVisibility);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String validVisibility"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Type_Declarations()
    {
        // Arrange
        var pumlWithInvalidTypes = """

                                   @startuml
                                   class TestClass {
                                       +String name
                                       +invalid type declaration
                                       -int age
                                   }
                                   @enduml
                                   """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidTypes);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
        });
    }

    [Test]
    public void Should_Handle_Invalid_Hide_Declaration()
    {
        // Arrange
        var pumlWithInvalidHide = """

                                  @startuml
                                  class TestClass {
                                      +String name
                                  }

                                  hide invalid hide syntax
                                  @enduml
                                  """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidHide);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(result, Does.Contain("class TestClass"));
        });
    }

    [Test]
    public void Should_Handle_Complex_Mixed_Errors()
    {
        // Arrange
        var complexPumlWithErrors = """

                                    @startuml
                                    class ValidClass {
                                        +String name
                                        -int age
                                        #calculate(int value) : int
                                    }

                                    class InvalidClass {
                                        +invalid syntax
                                        -missing type
                                        #broken method
                                    }

                                    enum ValidEnum {
                                        ACTIVE
                                        INACTIVE
                                    }

                                    enum InvalidEnum {
                                        VALID_ITEM
                                        invalid item
                                    }

                                    ValidClass --> InvalidClass : uses
                                    ValidClass --> ValidEnum : has
                                    @enduml
                                    """;

        // Act
        var ast = new SchemeAst(complexPumlWithErrors);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            Assert.That(reconstructor.Errors.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(result, Does.Contain("class ValidClass"));
            Assert.That(result, Does.Contain("class InvalidClass"));
            Assert.That(result, Does.Contain("enum ValidEnum"));
            Assert.That(result, Does.Contain("enum InvalidEnum"));
        });
    }

    [Test]
    public void Should_Provide_Detailed_Error_Messages()
    {
        // Arrange
        var invalidPuml = """

                          @startuml
                          class TestClass {
                              +invalid syntax here
                          }
                          @enduml
                          """;

        // Act
        var ast = new SchemeAst(invalidPuml);
        var reconstructor = new PumlReconstructor(false);
        reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors);
            foreach (var error in reconstructor.Errors)
            {
                Assert.That(error.Contains("Erreur") || error.Contains("Error"));
                Assert.That(string.IsNullOrEmpty(error), Is.False);
            }
        });
    }

    [Test]
    public void Should_Handle_Null_Context_Gracefully()
    {
        // Arrange
        var reconstructor = new PumlReconstructor(false);

        // Act & Assert - Should not throw when visiting null contexts
        Assert.That(() => reconstructor.Visit(null), Throws.Nothing);
    }

    [Test]
    public void Should_Handle_Empty_Puml()
    {
        // Arrange
        const string emptyPuml = """

                                 @startuml
                                 @enduml
                                 """;

        // Act
        var ast = new SchemeAst(emptyPuml);
        var reconstructor = new PumlReconstructor(false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors, Is.False);
            Assert.That(reconstructor.Errors, Is.Empty);
            Assert.That(result, Does.Contain("@startuml"));
            Assert.That(result, Does.Contain("@enduml"));
        });
    }

    [Test]
    public void Should_Ignore_Non_Fatal_Errors_By_Default()
    {
        // Arrange
        const string pumlWithMinorErrors = """

                                           @startuml
                                           class TestClass {
                                               +String name
                                               -int age
                                               #method with minor syntax issue
                                           }
                                           @enduml
                                           """;

        // Act
        var ast = new SchemeAst(pumlWithMinorErrors);
        var reconstructor = new PumlReconstructor(false, true); // ignoreNonFatalErrors = true
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
            // Les erreurs non-fatales ne devraient pas empêcher la reconstruction
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Format_Error_Messages_Correctly()
    {
        // Arrange
        const string invalidPuml = """

                                   @startuml
                                   class BrokenClass {
                                       +String name
                                       -int age
                                   }
                                   @enduml
                                   """;

        // Act
        var ast = new SchemeAst(invalidPuml);
        var reconstructor = new PumlReconstructor(false, false); // Collecter toutes les erreurs
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class BrokenClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
            // Le reconstructor devrait fonctionner même avec des erreurs de syntaxe mineures
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Visitor_Errors_Gracefully()
    {
        // Arrange
        const string validPuml = """

                                 @startuml
                                 class TestClass {
                                     +String name
                                     -int age
                                 }
                                 @enduml
                                 """;

        // Act
        var ast = new SchemeAst(validPuml);
        var reconstructor = new PumlReconstructor(false, false); // Collecter toutes les erreurs
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(result, Does.Contain("-int age"));
            Assert.That(reconstructor.HasErrors, Is.False);
            Assert.That(reconstructor.Errors, Is.Empty);
        });
    }

    [Test]
    public void Should_Format_Error_Messages_With_Proper_Prefix()
    {
        // Arrange
        const string validPuml = """

                                 @startuml
                                 class TestClass {
                                     +String name
                                 }
                                 @enduml
                                 """;

        // Act
        var ast = new SchemeAst(validPuml);
        var reconstructor = new PumlReconstructor(false, false);
        var result = reconstructor.Visit(ast.Tree);

        // Simuler une erreur pour tester le formatage
        reconstructor.AddError("Test error");

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(reconstructor.HasErrors);
            Assert.That(reconstructor.Errors, Is.Not.Empty);
            
            // Vérifier que les messages d'erreur suivent le format demandé
            foreach (var error in reconstructor.Errors)
            {
                Assert.That(error, Does.StartWith("PlantUML Syntax error:"));
            }
        });
    }

    [Test]
    public void Should_Distinguish_Fatal_And_Non_Fatal_Errors()
    {
        // Arrange
        const string pumlWithMixedErrors = """

                                           @startuml
                                           class ValidClass {
                                               +String name
                                           }
                                           
                                           class BrokenClass {
                                               +invalid syntax
                                           }
                                           @enduml
                                           """;

        // Act
        var ast = new SchemeAst(pumlWithMixedErrors);
        var reconstructor = new PumlReconstructor(false, false); // Collecter toutes les erreurs
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class ValidClass"));
            Assert.That(result, Does.Contain("+String name"));
            Assert.That(reconstructor.HasErrors);
            
            // Vérifier qu'on peut distinguer les erreurs fatales
            var fatalErrors = reconstructor.FatalErrors;
            Assert.That(fatalErrors, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Clear_Errors_When_Requested()
    {
        // Arrange
        const string invalidPuml = """

                                   @startuml
                                   class BrokenClass {
                                       +invalid syntax
                                   }
                                   @enduml
                                   """;

        // Act
        var ast = new SchemeAst(invalidPuml);
        var reconstructor = new PumlReconstructor(false, false);
        reconstructor.Visit(ast.Tree);

        Assert.That(reconstructor.HasErrors);

        // Clear errors
        reconstructor.ClearErrors();

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors, Is.False);
            Assert.That(reconstructor.Errors, Is.Empty);
        });
    }
}
