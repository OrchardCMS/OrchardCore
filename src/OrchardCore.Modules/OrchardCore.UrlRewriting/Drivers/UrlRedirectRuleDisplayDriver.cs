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
            model.SubstitutionUrl = metadata.SubstitutionUrl;
            model.AppendQueryString = context.IsNew || metadata.AppendQueryString;
            model.Pattern = metadata.Pattern;
            model.IgnoreCase = metadata.IgnoreCase;
            model.RedirectType = metadata.RedirectType;
            model.RedirectTypes =
            [
                new(S["Moved Permanently (301)"], nameof(RedirectType.MovedPermanently301)),
                new(S["Temporary Redirect (307)"], nameof(RedirectType.TemporaryRedirect307)),
                new(S["Found (302)"], nameof(RedirectType.Found302)),
                new(S["Permanent Redirect (308)"], nameof(RedirectType.PermanentRedirect308))
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
            m => m.IgnoreCase,
            m => m.SubstitutionUrl,
            m => m.AppendQueryString,
            m => m.RedirectType);

        if (string.IsNullOrWhiteSpace(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), "The url match pattern is required.");
        }

        if (string.IsNullOrWhiteSpace(model.SubstitutionUrl))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SubstitutionUrl), S["The redirect URL is required"]);
        }

        rule.Put(new UrlRedirectSourceMetadata()
        {
            Pattern = model.Pattern,
            IgnoreCase = model.IgnoreCase,
            SubstitutionUrl = model.SubstitutionUrl,
            AppendQueryString = model.AppendQueryString,
            RedirectType = model.RedirectType,
        });

        return Edit(rule, context);
    }
}
