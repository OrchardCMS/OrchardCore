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
            context.Result.Fail(new ValidationResult(S["The Match URL Pattern is required."], [nameof(UrlRedirectSourceMetadata.Pattern)]));
        }

        if (string.IsNullOrWhiteSpace(metadata.SubstitutionPattern))
        {
            context.Result.Fail(new ValidationResult(S["The Redirect URL Pattern is required."], [nameof(UrlRedirectSourceMetadata.SubstitutionPattern)]));
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

        var ignoreCase = data[nameof(UrlRedirectSourceMetadata.IsCaseInsensitive)]?.GetValue<bool>();

        if (ignoreCase.HasValue)
        {
            metadata.IsCaseInsensitive = ignoreCase.Value;
        }

        var substitutionPattern = data[nameof(UrlRedirectSourceMetadata.SubstitutionPattern)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(substitutionPattern))
        {
            metadata.SubstitutionPattern = substitutionPattern;
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
        else if (!Enum.IsDefined(typeof(RedirectType), metadata.RedirectType))
        {
            metadata.RedirectType = RedirectType.Found;
        }

        rule.Put(metadata);

        return Task.CompletedTask;
    }
}
