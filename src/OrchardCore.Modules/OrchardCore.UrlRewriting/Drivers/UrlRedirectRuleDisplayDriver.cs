using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Drivers;

public sealed class UrlRedirectRuleDisplayDriver : DisplayDriver<RewriteRule>
{
    internal readonly IStringLocalizer S;

    public UrlRedirectRuleDisplayDriver(IStringLocalizer<UrlRedirectRuleDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RewriteRule rule, BuildEditorContext context)
    {
        if (rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return null;
        }

        return Initialize<UrlRedirectRuleViewModel>("UrlRedirectRule_Edit", model =>
        {
            var metadata = rule.As<UrlRedirectSourceMetadata>();

            model.Pattern = metadata.Pattern;
            model.SubstitutionPattern = metadata.SubstitutionPattern;
            model.IsCaseInsensitive = metadata.IsCaseInsensitive;
            model.QueryStringPolicy = metadata.QueryStringPolicy;
            model.RedirectType = Enum.IsDefined<RedirectType>(metadata.RedirectType)
            ? metadata.RedirectType
            : RedirectType.Found;

            model.QueryStringPolicies =
            [
                new(S["Append Query String from Original Request"], nameof(QueryStringPolicy.Append)),
                new(S["Exclude Query String from Original Request"], nameof(QueryStringPolicy.Drop)),
            ];
            model.RedirectTypes =
            [
                new(S["Found (302)"], nameof(RedirectType.Found)),
                new(S["Moved Permanently (301)"], nameof(RedirectType.MovedPermanently)),
                new(S["Temporary Redirect (307)"], nameof(RedirectType.TemporaryRedirect)),
                new(S["Permanent Redirect (308)"], nameof(RedirectType.PermanentRedirect)),
            ];
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(RewriteRule rule, UpdateEditorContext context)
    {
        if (rule.Source != UrlRedirectRuleSource.SourceName)
        {
            return null;
        }

        var model = new UrlRedirectRuleViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Pattern,
            m => m.SubstitutionPattern,
            m => m.IsCaseInsensitive,
            m => m.QueryStringPolicy,
            m => m.RedirectType);

        if (string.IsNullOrWhiteSpace(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), S["The Match URL Pattern is required."]);
        }
        else if (!PatternHelper.IsValidRegex(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), S["A valid Match URL Pattern is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.SubstitutionPattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SubstitutionPattern), S["The Substitution URL Pattern is required."]);
        }
        else if (!PatternHelper.IsValidRegex(model.SubstitutionPattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SubstitutionPattern), S["A valid Substitution URL Pattern is required."]);
        }

        rule.Put(new UrlRedirectSourceMetadata()
        {
            Pattern = model.Pattern?.Trim(),
            SubstitutionPattern = model.SubstitutionPattern?.Trim(),
            IsCaseInsensitive = model.IsCaseInsensitive,
            QueryStringPolicy = model.QueryStringPolicy,
            RedirectType = model.RedirectType,
        });

        return Edit(rule, context);
    }
}
