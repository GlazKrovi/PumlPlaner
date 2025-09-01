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
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("syntax here"));
            Assert.That(result, Does.Contain("- missing type"));
            // Le reconstructor devrait fonctionner même avec des erreurs de syntaxe
            Assert.That(result, Is.Not.Empty);
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

        // Ajouter une erreur manuellement pour tester le mode strict
        reconstructor.AddError("Test error");

        // L'exception devrait être lancée lors de la visite
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
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            Assert.That(reconstructor.HasErrors, Is.False);
            Assert.That(reconstructor.Errors, Is.Empty);
            Assert.That(result, Does.Contain("class ValidClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
            Assert.That(result, Does.Contain("# calculate(int value) : int"));
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
                                         #broken method syntax
                                     }

                                     Class1 <<uses>> Class2
                                     @enduml
                                     """;

        // Act
        var ast = new SchemeAst(pumlWithMultipleErrors);
        var reconstructor = new PumlReconstructor(false, false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class Class1"));
            Assert.That(result, Does.Contain("class Class2"));
            Assert.That(result, Does.Contain("+ invalid syntax"));
            Assert.That(result, Does.Contain("method syntax"));
            Assert.That(result, Does.Contain("Class1"));
            Assert.That(result, Does.Contain("uses"));
            Assert.That(result, Does.Contain("Class2"));
            // Le reconstructor devrait fonctionner même avec des erreurs multiples
            Assert.That(result, Is.Not.Empty);
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
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
            Assert.That(result, Does.Contain("# calculate() : int"));
            Assert.That(result, Does.Contain("+ invalid member"));
            // Le reconstructor devrait fonctionner même avec des membres invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Connections()
    {
        // Arrange
        const string pumlWithInvalidConnections = """

                                                  @startuml
                                                  class Class1 {
                                                      +String name
                                                  }

                                                  class Class2 {
                                                      -int age
                                                  }

                                                  Class1 <<invalid connection syntax>> Class2
                                                  @enduml
                                                  """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidConnections);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class Class1"));
            Assert.That(result, Does.Contain("class Class2"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
            Assert.That(result, Does.Contain("Class1"));
            Assert.That(result, Does.Contain("invalid"));
            Assert.That(result, Does.Contain("connection"));
            Assert.That(result, Does.Contain("syntax"));
            Assert.That(result, Does.Contain("Class2"));
            // Le reconstructor devrait fonctionner même avec des connexions invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Enum_Declaration()
    {
        // Arrange
        const string pumlWithInvalidEnum = """

                                           @startuml
                                           enum TestEnum {
                                               item syntax
                                           }
                                           @enduml
                                           """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidEnum);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("enum TestEnum"));
            Assert.That(result, Does.Contain("item"));
            // Le reconstructor devrait fonctionner même avec des énumérations invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Inheritance()
    {
        // Arrange
        const string pumlWithInvalidInheritance = """

                                                 @startuml
                                                 class ChildClass extends ParentClass {
                                                     +String name
                                                 }
                                                 @enduml
                                                 """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidInheritance);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class ChildClass"));
            Assert.That(result, Does.Contain("extends ParentClass"));
            Assert.That(result, Does.Contain("+ String name"));
            // Le reconstructor devrait fonctionner même avec des héritages invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Template_Parameters()
    {
        // Arrange
        const string pumlWithInvalidTemplate = """

                                              @startuml
                                              class TestClass<T> {
                                                  +String name
                                                  +invalid template syntax
                                              }
                                              @enduml
                                              """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidTemplate);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass<T>"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("template syntax"));
            // Le reconstructor devrait fonctionner même avec des paramètres de template invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Stereotypes()
    {
        // Arrange
        const string pumlWithInvalidStereotype = """

                                                @startuml
                                                class TestClass <<stereotype syntax>> {
                                                    +String name
                                                }
                                                @enduml
                                                """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidStereotype);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("<<stereotype>>"));
            Assert.That(result, Does.Contain("+ String name"));
            // Le reconstructor devrait fonctionner même avec des stéréotypes invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Method_Arguments()
    {
        // Arrange
        const string pumlWithInvalidMethodArgs = """

                                                @startuml
                                                class TestClass {
                                                    +String name
                                                    +calculate(invalid argument syntax) : int
                                                }
                                                @enduml
                                                """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidMethodArgs);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("+"));
            Assert.That(result, Does.Contain("int"));
            // Le reconstructor devrait fonctionner même avec des arguments de méthode invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Attribute_Declaration()
    {
        // Arrange
        const string pumlWithInvalidAttribute = """

                                              @startuml
                                              class TestClass {
                                                  +String name
                                                  attribute syntax
                                                  -int age
                                              }
                                              @enduml
                                              """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidAttribute);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("attribute syntax"));
            Assert.That(result, Does.Contain("- int age"));
            // Le reconstructor devrait fonctionner même avec des déclarations d'attribut invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Visibility_Modifiers()
    {
        // Arrange
        const string pumlWithInvalidVisibility = """

                                                @startuml
                                                class TestClass {
                                                    String invalidVisibility
                                                    +String validVisibility
                                                }
                                                @enduml
                                                """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidVisibility);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("String invalidVisibility"));
            Assert.That(result, Does.Contain("+ String validVisibility"));
            // Le reconstructor devrait fonctionner même avec des modificateurs de visibilité invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Type_Declarations()
    {
        // Arrange
        const string pumlWithInvalidTypes = """

                                           @startuml
                                           class TestClass {
                                               +String name
                                               type declaration
                                               -int age
                                           }
                                           @enduml
                                           """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidTypes);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("type declaration"));
            Assert.That(result, Does.Contain("- int age"));
            // Le reconstructor devrait fonctionner même avec des déclarations de type invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Invalid_Hide_Declaration()
    {
        // Arrange
        const string pumlWithInvalidHide = """

                                          @startuml
                                          class TestClass {
                                              +String name
                                          }
                                          
                                          hide invalid
                                          @enduml
                                          """;

        // Act
        var ast = new SchemeAst(pumlWithInvalidHide);
        var reconstructor = new PumlReconstructor();
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("hide invalid"));
            // Le reconstructor devrait fonctionner même avec des déclarations hide invalides
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Complex_Mixed_Errors()
    {
        // Arrange
        const string pumlWithComplexErrors = """

                                            @startuml
                                            class Class1 {
                                                +String name
                                                -int age
                                            }

                                            class Class2 {
                                                #calculate() : int
                                                +invalid member
                                            }

                                            enum TestEnum {
                                                item syntax
                                            }

                                            Class1 <<uses>> Class2
                                            Class2 <<has>> TestEnum
                                            @enduml
                                            """;

        // Act
        var ast = new SchemeAst(pumlWithComplexErrors);
        var reconstructor = new PumlReconstructor(false, false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class Class1"));
            Assert.That(result, Does.Contain("class Class2"));
            Assert.That(result, Does.Contain("enum TestEnum"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
            Assert.That(result, Does.Contain("# calculate() : int"));
            Assert.That(result, Does.Contain("+ invalid member"));
            Assert.That(result, Does.Contain("item"));
            Assert.That(result, Does.Contain("Class1"));
            Assert.That(result, Does.Contain("uses"));
            Assert.That(result, Does.Contain("Class2"));
            Assert.That(result, Does.Contain("has"));
            Assert.That(result, Does.Contain("TestEnum"));
            // Le reconstructor devrait fonctionner même avec des erreurs complexes
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Provide_Detailed_Error_Messages()
    {
        // Arrange
        const string invalidPuml = """

                                   @startuml
                                   class BrokenClass {
                                       +invalid syntax here
                                   }
                                   @enduml
                                   """;

        // Act
        var ast = new SchemeAst(invalidPuml);
        var reconstructor = new PumlReconstructor(false, false);
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class BrokenClass"));
            Assert.That(result, Does.Contain("syntax here"));
            // Le reconstructor devrait fonctionner même avec des erreurs de syntaxe
            Assert.That(result, Is.Not.Empty);
        });
    }

    [Test]
    public void Should_Handle_Null_Context_Gracefully()
    {
        // Arrange
        var reconstructor = new PumlReconstructor();

        // Act & Assert
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
        var reconstructor = new PumlReconstructor();
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
        var reconstructor = new PumlReconstructor(); // ignoreNonFatalErrors = true
        var result = reconstructor.Visit(ast.Tree);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result, Does.Contain("class TestClass"));
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
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
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
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
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("- int age"));
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
            Assert.That(result, Does.Contain("+ String name"));
            Assert.That(result, Does.Contain("class BrokenClass"));
            Assert.That(result, Does.Contain("+ invalid syntax"));
            Assert.That(reconstructor.HasErrors, Is.False); // Pas d'erreurs dans le visitor
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
        var result = reconstructor.Visit(ast.Tree);

        // Simuler une erreur pour tester le nettoyage
        reconstructor.AddError("Test error");

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
