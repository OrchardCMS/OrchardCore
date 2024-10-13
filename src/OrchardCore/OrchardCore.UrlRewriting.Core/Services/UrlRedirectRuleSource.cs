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

    internal IStringLocalizer S;

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

        builderFlags.AppendCommaSeparatedValues("R=");
        builderFlags.Append(RedirectTypeToStatusCode(metadata.RedirectType));

        builder.Append(builderFlags);
        builder.Append(']');

        using var reader = new StringReader(builder.ToString());

        options.AddApacheModRewrite(reader);
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
