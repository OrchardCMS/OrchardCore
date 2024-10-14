using System.ComponentModel.DataAnnotations;
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
    {
        if (context.Rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return Task.CompletedTask;
        }

        var metadata = context.Rule.As<UrlRewriteSourceMetadata>();

        var pattern = context.Data[nameof(UrlRewriteSourceMetadata.Pattern)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(pattern))
        {
            metadata.Pattern = pattern;
        }

        var url = context.Data[nameof(UrlRewriteSourceMetadata.SubstitutionUrl)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var _))
        {
            metadata.SubstitutionUrl = url;
        }

        var ignoreCase = context.Data[nameof(UrlRewriteSourceMetadata.IgnoreCase)]?.GetValue<bool>();

        if (ignoreCase.HasValue)
        {
            metadata.IgnoreCase = ignoreCase.Value;
        }

        var appendQueryString = context.Data[nameof(UrlRewriteSourceMetadata.AppendQueryString)]?.GetValue<bool>();

        if (appendQueryString.HasValue)
        {
            metadata.AppendQueryString = appendQueryString.Value;
        }

        var skipFurtherRules = context.Data[nameof(UrlRewriteSourceMetadata.SkipFurtherRules)]?.GetValue<bool>();

        if (skipFurtherRules.HasValue)
        {
            metadata.SkipFurtherRules = skipFurtherRules.Value;
        }

        context.Rule.Put(metadata);

        return Task.CompletedTask;
    }

    public override Task ValidatingAsync(ValidatingRewriteRuleContext context)
    {
        var metadata = context.Rule.As<UrlRewriteSourceMetadata>();

        if (string.IsNullOrWhiteSpace(metadata.Pattern))
        {
            context.Result.Fail(new ValidationResult(S["The url match pattern is required."], [nameof(UrlRewriteSourceMetadata.Pattern)]));
        }

        if (string.IsNullOrWhiteSpace(metadata.SubstitutionUrl))
        {
            context.Result.Fail(new ValidationResult(S["The Rewrite URL is required."], [nameof(UrlRewriteSourceMetadata.SubstitutionUrl)]));
        }
        else if (!Uri.TryCreate(metadata.SubstitutionUrl, UriKind.RelativeOrAbsolute, out var _))
        {
            context.Result.Fail(new ValidationResult(S["The Rewrite URL is invalid."], [nameof(UrlRewriteSourceMetadata.SubstitutionUrl)]));
        }

        return Task.CompletedTask;
    }
}
