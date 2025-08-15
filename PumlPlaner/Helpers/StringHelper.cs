using System.Text.RegularExpressions;

namespace PumlPlaner.Helpers;

public static partial class StringHelper
{
    internal static string NormalizeBreakLines(string text)
    {
        return text
            .Replace("\r", "\n")
            .Replace("\\r", "\n")
            .Replace("\\n", "\n");
    }

    internal static string RemoveMultipleBreaks(string text)
    {
        return MultiBreaksRegex().Replace(text, "\n");
    }

    internal static string NormalizeEndOfFile(string text)
    {
        if (text.EndsWith('\n')) return text;
        return text + "\n";
    }

    [GeneratedRegex(@"\n\s*\n")]
    private static partial Regex MultiBreaksRegex();
}