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
    {
        if (context.Rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Rule.As<UrlRedirectSourceMetadata>();

        var pattern = context.Data[nameof(UrlRedirectSourceMetadata.Pattern)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(pattern))
        {
            metadata.Pattern = pattern;
        }

        var ignoreCase = context.Data[nameof(UrlRedirectSourceMetadata.IgnoreCase)]?.GetValue<bool>();

        if (ignoreCase.HasValue)
        {
            metadata.IgnoreCase = ignoreCase.Value;
        }

        var url = context.Data[nameof(UrlRedirectSourceMetadata.SubstitutionUrl)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var _))
        {
            metadata.SubstitutionUrl = url;
        }

        var appendQueryString = context.Data[nameof(UrlRedirectSourceMetadata.AppendQueryString)]?.GetValue<bool>();

        if (appendQueryString.HasValue)
        {
            metadata.AppendQueryString = appendQueryString.Value;
        }

        var redirectType = context.Data[nameof(UrlRedirectSourceMetadata.RedirectType)]?.GetEnumValue<RedirectType>();

        if (redirectType.HasValue)
        {
            metadata.RedirectType = redirectType.Value;
        }

        context.Rule.Put(metadata);

        return Task.CompletedTask;
    }

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
}
