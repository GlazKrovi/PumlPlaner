using PumlPlaner.AST;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlanerTester;

public class AstSumTests
{
    [Test]
    public void ShouldSumTwoSimpleSchemas()
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

        var expectedResult = StringHelper.NormalizeBreakLines("""
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

                                                              """);

        // process
        var firstAst = new SchemeAst(firstInput);
        var secondAst = new SchemeAst(secondInput);

        var visitor = new PumlSum();
        var merged = visitor.VisitUml(firstAst.Tree, secondAst.Tree);

        Assert.That(merged, Is.EqualTo(expectedResult));
    }
}