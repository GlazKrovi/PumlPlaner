namespace PumlPlaner.Helpers;

public static class StringHelper
{
    internal static string NormalizeBreakLines(string text)
    {
        return text.Replace("\r\n", "\\n")
            .Replace("\r", "\\n")
            .Replace("\n", "\\n");
    }
}