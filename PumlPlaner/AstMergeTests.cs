using Antlr4.Runtime;
using PumlPlaner.AST;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlaner;

public class AstMergeTests
{
    [Test]
    public void ShouldMergeTwoSimpleSchemas()
    {
        const string firstInput = """
                                  @startuml
                                  class Charachter {
                                    - string name
                                    + attack()
                                  }
                                  @enduml
                                  """;

        const string secondInput = """
                         @startuml
                         class Weapon {
                           - float power
                           + hit()
                         }
                         @enduml
                         """;

        const string expectedResult = """
                                   @startuml
                                   class Charachter {
                                     - string name
                                     + attack()
                                   }
                                   class Weapon {
                                     - float power
                                     + hit()
                                   }
                                   @enduml
                                   """;

        // process
        var firstAst = new SchemeAst(firstInput);
        var secondAst = new SchemeAst(secondInput);

        var visitor = new PlantUmlMerger();
        var merged = visitor.VisitUml(firstAst.Tree, secondAst.Tree);

        Assert.That(merged.ToString() ?? string.Empty, Is.EqualTo(expectedResult));
    }
}