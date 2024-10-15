using System.Text;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.Modules.Extensions;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class UrlRedirectRuleSource : IUrlRewriteRuleSource
{
    public const string SourceName = "Redirect";

    internal readonly IStringLocalizer S;

    public UrlRedirectRuleSource(IStringLocalizer<UrlRedirectRuleSource> stringLocalizer)
    {
        S = stringLocalizer;
        Description = S["URL Redirect Rule"];
    }

    public string Name
        => SourceName;

    public LocalizedString Description { get; }

    public void Configure(RewriteOptions options, RewriteRule rule)
    {
        if (!rule.TryGet<UrlRedirectSourceMetadata>(out var metadata) ||
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

    private static void AppendFlags(StringBuilder builder, UrlRedirectSourceMetadata metadata)
    {
        var initialLength = builder.Length;

        void AppendFlag(StringBuilder builder, string flag)
        {
            if (builder.Length > initialLength)
            {
                builder.AppendComma();
            }

            builder.Append(flag);
        }

        if (metadata.IsCaseInsensitive)
        {
            AppendFlag(builder, "NC");
        };

        if (metadata.AppendQueryString)
        {
            AppendFlag(builder, "QSA");
        }
        else
        {
            AppendFlag(builder, "QSD");
        }

        AppendFlag(builder, "R=");
        builder.Append((int)metadata.RedirectType);
    }
}
