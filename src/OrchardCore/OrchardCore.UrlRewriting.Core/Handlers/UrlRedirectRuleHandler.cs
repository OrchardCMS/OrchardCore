using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.UrlRewriting.Handlers;

public sealed class UrlRedirectRuleHandler : RewriteRuleHandlerBase
{
    internal readonly IStringLocalizer S;

    public UrlRedirectRuleHandler(IStringLocalizer<UrlRedirectRuleHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingRewriteRuleContext context)
        => PopulateAsync(context.Rule, context.Data);

    public override Task UpdatingAsync(UpdatingRewriteRuleContext context)
        => PopulateAsync(context.Rule, context.Data);

    public override Task ValidatingAsync(ValidatingRewriteRuleContext context)
    {
        var metadata = context.Rule.As<UrlRedirectSourceMetadata>();

        if (string.IsNullOrWhiteSpace(metadata.Pattern))
        {
            context.Result.Fail(new ValidationResult(S["The url match pattern is required."], [nameof(UrlRedirectSourceMetadata.Pattern)]));
        }

        if (string.IsNullOrWhiteSpace(metadata.SubstitutionUrl))
        {
            context.Result.Fail(new ValidationResult(S["The Redirect URL is required."], [nameof(UrlRedirectSourceMetadata.SubstitutionUrl)]));
        }
        else if (!Uri.TryCreate(metadata.SubstitutionUrl, UriKind.RelativeOrAbsolute, out var _))
        {
            context.Result.Fail(new ValidationResult(S["The Redirect URL is invalid."], [nameof(UrlRedirectSourceMetadata.SubstitutionUrl)]));
        }

        return Task.CompletedTask;
    }

    private static Task PopulateAsync(RewriteRule rule, JsonNode data)
    {
        if (rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = rule.As<UrlRedirectSourceMetadata>();

        var pattern = data[nameof(UrlRedirectSourceMetadata.Pattern)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(pattern))
        {
            metadata.Pattern = pattern;
        }

        var ignoreCase = data[nameof(UrlRedirectSourceMetadata.IgnoreCase)]?.GetValue<bool>();

        if (ignoreCase.HasValue)
        {
            metadata.IgnoreCase = ignoreCase.Value;
        }

        var url = data[nameof(UrlRedirectSourceMetadata.SubstitutionUrl)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var _))
        {
            metadata.SubstitutionUrl = url;
        }

        var appendQueryString = data[nameof(UrlRedirectSourceMetadata.AppendQueryString)]?.GetValue<bool>();

        if (appendQueryString.HasValue)
        {
            metadata.AppendQueryString = appendQueryString.Value;
        }

        var redirectType = data[nameof(UrlRedirectSourceMetadata.RedirectType)]?.GetEnumValue<RedirectType>();

        if (redirectType.HasValue)
        {
            metadata.RedirectType = redirectType.Value;
        }

        rule.Put(metadata);

        return Task.CompletedTask;
    }
}
