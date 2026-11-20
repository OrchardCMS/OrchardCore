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

    public static bool IsValidSubstitutionPattern(string pattern)
    {
        try
        {
            using var reader = new StringReader($"RewriteRule ^ {pattern}");

            new RewriteOptions().AddApacheModRewrite(reader);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
