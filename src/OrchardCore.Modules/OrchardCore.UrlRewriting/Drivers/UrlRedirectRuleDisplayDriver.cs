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
            model.AppendQueryString = context.IsNew || metadata.AppendQueryString;
            model.RedirectType = Enum.IsDefined(typeof(RedirectType), metadata.RedirectType)
            ? metadata.RedirectType
            : RedirectType.Found;

            model.RedirectTypes =
            [
                new(S["Found (302)"], nameof(RedirectType.Found)),
                new(S["Moved Permanently (301)"], nameof(RedirectType.MovedPermanently)),
                new(S["Temporary Redirect (307)"], nameof(RedirectType.TemporaryRedirect)),
                new(S["Permanent Redirect (308)"], nameof(RedirectType.PermanentRedirect))
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
            m => m.AppendQueryString,
            m => m.RedirectType);

        if (string.IsNullOrWhiteSpace(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), S["The Match URL Pattern is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.SubstitutionPattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SubstitutionPattern), S["The Redirect URL Pattern is required."]);
        }

        rule.Put(new UrlRedirectSourceMetadata()
        {
            Pattern = model.Pattern,
            SubstitutionPattern = model.SubstitutionPattern,
            IsCaseInsensitive = model.IsCaseInsensitive,
            AppendQueryString = model.AppendQueryString,
            RedirectType = model.RedirectType,
        });

        return Edit(rule, context);
    }
}
