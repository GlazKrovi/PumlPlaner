using System.Text.RegularExpressions;

namespace PumlPlaner.Helpers;

public static partial class StringHelper
{
    internal static string NormalizeBreakLines(string text)
    {
        return text.Replace("\r\n", "\n")
            .Replace("\r", "\n");
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

    [GeneratedRegex(@"\n{2,}")]
    private static partial Regex MultiBreaksRegex();
}