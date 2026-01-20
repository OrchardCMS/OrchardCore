using System.Text.RegularExpressions;

namespace OrchardCore.UrlRewriting;

public static class PatternHelper
{
    public static bool IsValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern);

            // If the pattern is invalid, an exception is thrown.
            return true;
        }
        catch
        {
            // Any exception indicates an invalid pattern.
            return false;
        }
    }
}
