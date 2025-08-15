using Antlr4.Runtime;

namespace PumlPlaner.Helpers;

public class NormalizedInput
{
    public NormalizedInput(string rawInput)
    {
        RawInput = StringHelper.NormalizeBreakLines(rawInput);
        RawInput = StringHelper.RemoveMultipleBreaks(RawInput);
        RawInput = StringHelper.NormalizeEndOfFile(RawInput);
    }

    private string RawInput { get; }

    public override string ToString()
    {
        return RawInput;
    }

    public ICharStream ToCharStream()
    {
        return CharStreams.fromString(RawInput);
    }
}