using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite;

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

    public static bool IsValidMatchPattern(string pattern)
        => CanParseRule($"RewriteRule {pattern} -");

    public static bool IsValidSubstitutionPattern(string pattern)
        => CanParseRule($"RewriteRule ^ {pattern}");

    private static bool CanParseRule(string rule)
    {
        try
        {
            using var reader = new StringReader(rule);

            new RewriteOptions().AddApacheModRewrite(reader);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
