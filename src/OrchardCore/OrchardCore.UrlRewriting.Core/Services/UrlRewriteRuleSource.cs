using System.Text;
using Microsoft.AspNetCore.Rewrite;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public class UrlRewriteRuleSource : IUrlRewriteRuleSource
{
    public const string SourceName = "Rewrite";

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
            sbFlags.Append("QSA");
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

}
