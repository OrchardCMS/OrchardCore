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

    /// <summary>Validates the source arguments shared by admin and recipe updates.</summary>
    public override Task ValidatingAsync(ValidatingRewriteRuleContext context)
    {
        if (context.Rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Rule.GetOrCreate<UrlRedirectSourceMetadata>();

        RewriteRuleValidation.Validate(context.Result, metadata.Pattern, metadata.SubstitutionPattern,
            metadata.QueryStringPolicy, S, metadata.RedirectType);

        return Task.CompletedTask;
    }

    private static Task PopulateAsync(RewriteRule rule, JsonNode data)
    {
        if (rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = rule.GetOrCreate<UrlRedirectSourceMetadata>();

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

        var queryStringPolicy = data[nameof(UrlRewriteSourceMetadata.QueryStringPolicy)]?.GetEnumValue<QueryStringPolicy>();

        if (queryStringPolicy.HasValue)
        {
            metadata.QueryStringPolicy = queryStringPolicy.Value;
        }

        var redirectType = data[nameof(UrlRedirectSourceMetadata.RedirectType)]?.GetEnumValue<RedirectType>();

        if (redirectType.HasValue)
        {
            metadata.RedirectType = redirectType.Value;
        }
        else if (!Enum.IsDefined<RedirectType>(metadata.RedirectType))
        {
            metadata.RedirectType = RedirectType.Found;
        }

        rule.Put(metadata);

        return Task.CompletedTask;
    }
}
