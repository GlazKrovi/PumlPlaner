using Antlr4.Runtime;
using PumlPlaner.Helpers;

namespace PumlPlaner;

public class NormalizedInput
{
    private string RawInput { get; set;  }

    public NormalizedInput(string rawInput)
    {
        RawInput = StringHelper.NormalizeBreakLines(rawInput);
        RawInput = StringHelper.RemoveMultipleBreaks(rawInput);
        RawInput = StringHelper.NormalizeEndOfFile(rawInput);
        Console.WriteLine("at creation : " + RawInput);
    }

    public override string ToString()
    {
        return RawInput;
    }

    public ICharStream ToCharStream()
    {
        return  CharStreams.fromString(RawInput);
    }
}