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

    public override IDisplayResult Display(RewriteRule rule, BuildDisplayContext context)
    {
        if (rule.Source != UrlRewriteRuleSource.SourceName)
        {
            return null;
        }

        return Combine(
            Dynamic("UrlRewriteRule_SummaryAdmin", model =>
            {
                model.Query = rule;
            }).Location("Content:5")
        );
    }

    public override IDisplayResult Edit(RewriteRule rule, BuildEditorContext context)
    {
        if (rule.Source != UrlRewriteRuleSource.SourceName)
        {
            return null;
        }

        return Initialize<UrlRewriteRuleViewModel>("UrlRewriteRule_Edit", model =>
        {
            var metadata = rule.As<UrlRedirectSourceMetadata>();
            model.Url = metadata.Url;
            model.Pattern = metadata.Pattern;
            model.IgnoreCase = metadata.IgnoreCase;
            model.AppendQueryString = context.IsNew || metadata.AppendQueryString;
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
            m => m.Url,
            m => m.AppendQueryString,
            m => m.Pattern,
            m => m.IgnoreCase);

        if (string.IsNullOrWhiteSpace(model.Url))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Url), S["The rewrite URL is required"]);
        }

        rule.Put(new UrlRewriteSourceMetadata()
        {
            Pattern = model.Pattern,
            IgnoreCase = model.IgnoreCase,
            Url = model.Url,
            AppendQueryString = model.AppendQueryString,
        });

        return Edit(rule, context);
    }
}
