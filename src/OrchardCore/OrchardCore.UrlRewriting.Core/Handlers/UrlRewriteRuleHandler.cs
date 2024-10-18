using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.UrlRewriting.Handlers;

public sealed class UrlRewriteRuleHandler : RewriteRuleHandlerBase
{
    internal readonly IStringLocalizer S;

    public UrlRewriteRuleHandler(IStringLocalizer<UrlRewriteRuleHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingRewriteRuleContext context)
        => PopulateAsync(context.Rule, context.Data);

    public override Task UpdatingAsync(UpdatingRewriteRuleContext context)
        => PopulateAsync(context.Rule, context.Data);

    public override Task ValidatingAsync(ValidatingRewriteRuleContext context)
    {
        if (context.Rule.Source != UrlRewriteRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Rule.As<UrlRewriteSourceMetadata>();

        if (string.IsNullOrWhiteSpace(metadata.Pattern))
        {
            context.Result.Fail(new ValidationResult(S["The Match URL Pattern is required."], [nameof(UrlRedirectSourceMetadata.Pattern)]));
        }
        else if (!PatternHelper.IsValidRegex(metadata.Pattern))
        {
            context.Result.Fail(new ValidationResult(S["A valid Match URL Pattern is required."], [nameof(UrlRedirectSourceMetadata.Pattern)]));
        }

        if (string.IsNullOrWhiteSpace(metadata.SubstitutionPattern))
        {
            context.Result.Fail(new ValidationResult(S["The Substitution URL Pattern is required."], [nameof(UrlRedirectSourceMetadata.SubstitutionPattern)]));
        }
        else if (!PatternHelper.IsValidRegex(metadata.SubstitutionPattern))
        {
            context.Result.Fail(new ValidationResult(S["A valid Substitution URL Pattern is required."], [nameof(UrlRedirectSourceMetadata.SubstitutionPattern)]));
        }

        return Task.CompletedTask;
    }

    private static Task PopulateAsync(RewriteRule rule, JsonNode data)
    {
        if (rule.Source != UrlRewriteRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = rule.As<UrlRewriteSourceMetadata>();

        var pattern = data[nameof(UrlRewriteSourceMetadata.Pattern)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(pattern))
        {
            metadata.Pattern = pattern;
        }

        var substitutionPattern = data[nameof(UrlRewriteSourceMetadata.SubstitutionPattern)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(substitutionPattern))
        {
            metadata.SubstitutionPattern = substitutionPattern;
        }

        var ignoreCase = data[nameof(UrlRewriteSourceMetadata.IsCaseInsensitive)]?.GetValue<bool>();

        if (ignoreCase.HasValue)
        {
            metadata.IsCaseInsensitive = ignoreCase.Value;
        }

        var queryStringPolicy = data[nameof(UrlRewriteSourceMetadata.QueryStringPolicy)]?.GetEnumValue<QueryStringPolicy>();

        if (queryStringPolicy.HasValue)
        {
            metadata.QueryStringPolicy = queryStringPolicy.Value;
        }

        var skipFurtherRules = data[nameof(UrlRewriteSourceMetadata.SkipFurtherRules)]?.GetValue<bool>();

        if (skipFurtherRules.HasValue)
        {
            metadata.SkipFurtherRules = skipFurtherRules.Value;
        }

        rule.Put(metadata);

        return Task.CompletedTask;
    }
}
