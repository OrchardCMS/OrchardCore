using System.Text;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Modules.Extensions;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class UrlRewriteRuleSource : IUrlRewriteRuleSource
{
    public const string SourceName = "Rewrite";

    internal IStringLocalizer S;

    public UrlRewriteRuleSource(IStringLocalizer<UrlRewriteRuleSource> stringLocalizer)
    {
        S = stringLocalizer;

        Description = S["Rewrite Rule"];
    }
    public string Name
        => SourceName;

    public LocalizedString Description { get; }
    public void Configure(RewriteOptions options, RewriteRule rule)
    {
        if (!rule.TryGet<UrlRewriteSourceMetadata>(out var metadata))
        {
            return;
        }

        using var reader = new StringReader(GetRewriteRule(metadata));

        options.AddApacheModRewrite(reader);
    }

    private static string GetRewriteRule(UrlRewriteSourceMetadata metadata)
    {
        var flags = GetFlags(metadata);

        if (flags.Length > 0)
        {
            return $"RewriteRule \"{metadata.Pattern}\" \"{metadata.Url}\" [{flags}]";
        }

        return $"RewriteRule \"{metadata.Pattern}\" \"{metadata.Url}\"";
    }

    private static StringBuilder GetFlags(UrlRewriteSourceMetadata metadata)
    {
        var builder = new StringBuilder();

        if (metadata.IgnoreCase)
        {
            builder.AppendCommaSeparatedValues("NC");
        };

        if (metadata.AppendQueryString)
        {
            builder.AppendCommaSeparatedValues("QSA");
        }

        if (metadata.SkipFurtherRules)
        {
            builder.AppendCommaSeparatedValues('L');
        }

        return builder;
    }

}
