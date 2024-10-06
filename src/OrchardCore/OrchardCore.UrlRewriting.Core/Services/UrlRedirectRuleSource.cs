using System.Text;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class UrlRedirectRuleSource : IUrlRewriteRuleSource
{
    public const string SourceName = "Redirect";

    internal IStringLocalizer S;

    public UrlRedirectRuleSource(IStringLocalizer<UrlRedirectRuleSource> stringLocalizer)
    {
        S = stringLocalizer;

        Description = S["Redirect Rule"];
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
            sbFlags.Append("QSA,");

            sbFlags.Append($"R=");
            sbFlags.Append(RedirectTypeToStatusCode(metadata.RedirectType));
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
