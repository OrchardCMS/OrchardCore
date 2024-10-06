using System.Text;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class UrlRewriteRuleSource : IUrlRewriteRuleSource
{
    public const string SourceName = "Rewrite";

    internal IStringLocalizer S;

    public UrlRewriteRuleSource(IStringLocalizer<UrlRedirectRuleSource> stringLocalizer)
    {
        S = stringLocalizer;

        Description = S["Rewrite Rule"];
    }
    public string Name
        => SourceName;

    public LocalizedString Description { get; }
    public void Configure(RewriteOptions options, RewriteRule rule)
    {
        if (!rule.TryGet<UrlRedirectSourceMetadata>(out var metadata))
        {
            return;
        }

        using var apacheModRewrite = new StringReader(GetRewriteRule(rule, metadata));

        options.AddApacheModRewrite(apacheModRewrite);
    }

    private static string GetRewriteRule(RewriteRule rule, UrlRedirectSourceMetadata metadata)
    {
        var flags = GetFlags(rule, metadata);

        if (flags.Length > 0)
        {
            return $"RewriteRule \"{metadata.Pattern}\" \"{metadata.Url}\" [{flags}]";
        }

        return $"RewriteRule \"{metadata.Pattern}\" \"{metadata.Url}\"";
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
