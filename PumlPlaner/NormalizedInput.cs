using Antlr4.Runtime;
using PumlPlaner.Helpers;

namespace PumlPlaner;

public class NormalizedInput
{
    private string RawInput { get; }

    public NormalizedInput(string rawInput)
    {
        RawInput = StringHelper.NormalizeBreakLines(rawInput);
        RawInput = StringHelper.RemoveMultipleBreaks(RawInput);
        RawInput = StringHelper.NormalizeEndOfFile(RawInput);
    }

    public override string ToString()
    {
        return RawInput;
    }

    public ICharStream ToCharStream()
    {
        return CharStreams.fromString(RawInput);
    }
}