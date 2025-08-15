using Antlr4.Runtime;

namespace PumlPlaner.AST;

public class SchemeAst
{
    public SchemeAst(string input)
    {
        try
        {
            Lexer = new PumlgLexer(CharStreams.fromString(input));
            Tokens = new CommonTokenStream(Lexer);
            Parser = new PumlgParser(Tokens);
            Tree = Parser.uml();
        }
        catch (Exception e)
        {
            Console.WriteLine("AST build error: " + e);
            throw new Exception("AST build error: " + e);
        }
    }

    public SchemeAst(ICharStream input)
    {
        try
        {
            Lexer = new PumlgLexer(input);
            Tokens = new CommonTokenStream(Lexer);
            Parser = new PumlgParser(Tokens);
            Tree = Parser.uml();
        }
        catch (Exception e)
        {
            Console.WriteLine("AST build error: " + e);
            throw new Exception("AST build error: " + e);
        }
    }

    private PumlgLexer Lexer { get; }
    private CommonTokenStream Tokens { get; }
    private PumlgParser Parser { get; }
    public PumlgParser.UmlContext Tree { get; private set; }
}