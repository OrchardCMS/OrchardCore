using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Drivers;

public sealed class UrlRewriteRuleDisplayDriver : DisplayDriver<RewriteRule>
{
    internal readonly IStringLocalizer S;

    public UrlRewriteRuleDisplayDriver(IStringLocalizer<UrlRewriteRuleDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(RewriteRule rule, BuildEditorContext context)
    {
        if (rule.Source != UrlRewriteRuleSource.SourceName)
        {
            return null;
        }

        return Initialize<UrlRewriteRuleViewModel>("UrlRewriteRule_Edit", model =>
        {
            var metadata = rule.As<UrlRewriteSourceMetadata>();
            model.Pattern = metadata.Pattern;
            model.SubstitutionUrl = metadata.SubstitutionUrl;
            model.IgnoreCase = metadata.IgnoreCase;
            model.AppendQueryString = context.IsNew || metadata.AppendQueryString;
            model.SkipFurtherRules = metadata.SkipFurtherRules;
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(RewriteRule rule, UpdateEditorContext context)
    {
        if (rule.Source != UrlRewriteRuleSource.SourceName)
        {
            return null;
        }

        var model = new UrlRewriteRuleViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Pattern,
            m => m.SubstitutionUrl,
            m => m.IgnoreCase,
            m => m.AppendQueryString,
            m => m.SkipFurtherRules);

        if (string.IsNullOrWhiteSpace(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), "The url match pattern is required.");
        }

        if (string.IsNullOrWhiteSpace(model.SubstitutionUrl))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SubstitutionUrl), S["The Rewrite URL is required"]);
        }
        else if (!Uri.TryCreate(model.SubstitutionUrl, UriKind.RelativeOrAbsolute, out var _))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.SubstitutionUrl), S["The Rewrite URL is invalid."]);
        }

        rule.Put(new UrlRewriteSourceMetadata()
        {
            Pattern = model.Pattern,
            SubstitutionUrl = model.SubstitutionUrl,
            IgnoreCase = model.IgnoreCase,
            AppendQueryString = model.AppendQueryString,
            SkipFurtherRules = model.SkipFurtherRules
        });

        return Edit(rule, context);
    }
}
