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

        Description = S["URL Rewrite Rule"];
    }

    public string Name
        => SourceName;

    public LocalizedString Description { get; }

    public void Configure(RewriteOptions options, RewriteRule rule)
    {
        if (!rule.TryGet<UrlRewriteSourceMetadata>(out var metadata) ||
            string.IsNullOrEmpty(metadata.Pattern) ||
            string.IsNullOrEmpty(metadata.SubstitutionUrl))
        {
            return;
        }

        var builder = new StringBuilder();

        builder.Append("RewriteRule \"");
        builder.Append(metadata.Pattern);
        builder.Append("\" \"");
        builder.Append(metadata.SubstitutionUrl);
        builder.Append("\" [");

        var builderFlags = new StringBuilder();

        if (metadata.IgnoreCase)
        {
            builderFlags.AppendCommaSeparatedValues("NC");
        };

        if (metadata.AppendQueryString)
        {
            builderFlags.AppendCommaSeparatedValues("QSA");
        }
        else
        {
            builderFlags.AppendCommaSeparatedValues("QSD");
        }

        if (metadata.SkipFurtherRules)
        {
            builderFlags.AppendCommaSeparatedValues('L');
        }

        builder.Append(builderFlags);
        builder.Append(']');

        using var reader = new StringReader(builder.ToString());

        options.AddApacheModRewrite(reader);
    }
}
