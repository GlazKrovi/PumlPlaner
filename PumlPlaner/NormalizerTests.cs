using Antlr4.Runtime;
using PumlPlaner.Helpers;
using PumlPlaner.Visitors;

namespace PumlPlaner;

public class NormalizerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ShouldNormalizeEntries()
    {
        const string rawInput = """
                                @startuml






                                class Fruit {
                                \r
                                  - vitamins int
                                  
                                  
                                  
                                  
                                  + eat()
                                }

                                @enduml
                                """;

        var normalized = new NormalizedInput(rawInput);

        const string expectedInput = """
                                     @startuml

                                     class Fruit {
                                       - vitamins int
                                       + eat()
                                     }

                                     @enduml
                                     """;

        Assert.That(normalized.ToString(), Is.Not.EqualTo(rawInput));
        Assert.That(normalized.ToString(), Is.EqualTo(expectedInput));
    }
}