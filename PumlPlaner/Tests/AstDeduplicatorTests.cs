using NUnit.Framework;
using PumlPlaner.AST;

namespace PumlPlaner.Tests
{
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

            const string expected = """
            @startuml
            class Foo {
              + bar()
            }
            @enduml
            
            """;

            var ast = new SchemeAst(input);
            var deduplicator = new PlantUmlDeduplicator();
            var result = deduplicator.VisitUml(ast.Tree);

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

            const string expected = """
            @startuml
            class Foo {
              + bar()
              + baz()
            }
            @enduml
            
            """;

            var ast = new SchemeAst(input);
            var deduplicator = new PlantUmlDeduplicator();
            var result = deduplicator.VisitUml(ast.Tree);

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

            const string expected = """
            @startuml
            class Foo {
              + bar(int a)
              + bar(string b)
              + bar(int a, string b)
              + bar()
            }
            @enduml
            
            """;

            var ast = new SchemeAst(input);
            var deduplicator = new PlantUmlDeduplicator();
            var result = deduplicator.VisitUml(ast.Tree);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
