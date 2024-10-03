using System.Text;
using Microsoft.AspNetCore.Rewrite;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public class UrlRedirectRuleSource : IUrlRewriteRuleSource
{
    public const string SourceName = "Redirect";

    public string Name => SourceName;

    public void Configure(RewriteOptions options, RewriteRule rule)
    {
        using var apacheModRewrite = new StringReader(GetRewriteRule(rule));

        options.AddApacheModRewrite(apacheModRewrite);
    }

    private static string GetRewriteRule(RewriteRule rule)
    {
        var metadata = rule.As<UrlRedirectSourceMetadata>();

        var flags = GetFlags(rule, metadata);
        if (flags.Length > 0)
        {
            return $"RewriteRule \"{metadata.Pattern}\" \"{metadata.Url}\"";
        }

        return $"RewriteRule \"{metadata.Pattern}\" \"{metadata.Url}\" [{flags}]";
    }

    private static StringBuilder GetFlags(RewriteRule rule, UrlRedirectSourceMetadata metadata)
    {
        var sbFlags = new StringBuilder();

        if (metadata.IgnoreCase)
        {
            sbFlags.Append("NC");
        };

        if (metadata.AppendQueryString)
        {
            if (sbFlags.Length > 0)
            {
                sbFlags.Append(',');
            }
            sbFlags.Append("QSA,");

            sbFlags.Append($"R=");
            sbFlags.Append(RedirectTypeToStatusCode(metadata.RedirectType));
        }

        if (rule.SkipFurtherRules == true)
        {
            if (sbFlags.Length > 0)
            {
                sbFlags.Append(',');
            }

            sbFlags.Append('L');
        }

        return sbFlags;
    }

    public static RedirectType GetRedirectType(string flag)
    {
        return flag switch
        {
            "R=301" => RedirectType.MovedPermanently301,
            "R=302" => RedirectType.Found302,
            "R=307" => RedirectType.TemporaryRedirect307,
            "R=308" => RedirectType.PermanentRedirect308,
            _ => RedirectType.Found302
        };
    }

    private static int RedirectTypeToStatusCode(RedirectType redirectType)
    {
        return redirectType switch
        {
            RedirectType.MovedPermanently301 => 301,
            RedirectType.Found302 => 302,
            RedirectType.TemporaryRedirect307 => 307,
            RedirectType.PermanentRedirect308 => 308,
            _ => 302,
        };
    }
}
