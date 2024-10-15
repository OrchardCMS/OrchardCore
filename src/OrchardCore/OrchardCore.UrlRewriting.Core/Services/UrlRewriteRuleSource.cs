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

    internal readonly IStringLocalizer S;

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
            string.IsNullOrEmpty(metadata.SubstitutionPattern))
        {
            return;
        }

        var builder = new StringBuilder();

        builder.Append("RewriteRule ");
        builder.Append(metadata.Pattern);
        builder.Append(' ');
        builder.Append(metadata.SubstitutionPattern);
        builder.Append(" [");

        AppendFlags(builder, metadata);

        builder.Append(']');

        using var reader = new StringReader(builder.ToString());

        options.AddApacheModRewrite(reader);
    }

    private static void AppendFlags(StringBuilder builder, UrlRewriteSourceMetadata metadata)
    {
        var initialLength = builder.Length;

        void FlagAppend(StringBuilder builder, string flag)
        {
            if (builder.Length > initialLength)
            {
                builder.AppendComma();
            }

            builder.Append(flag);
        }

        if (metadata.IsCaseInsensitive)
        {
            FlagAppend(builder, "NC");
        };

        if (metadata.IgnoreQueryString)
        {
            FlagAppend(builder, "QSD");
        }
        else
        {
            FlagAppend(builder, "QSA");
        }

        if (metadata.SkipFurtherRules)
        {
            FlagAppend(builder, "L");
        }
    }
}
