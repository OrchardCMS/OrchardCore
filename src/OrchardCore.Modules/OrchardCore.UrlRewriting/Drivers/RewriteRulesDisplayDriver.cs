using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.UrlRewriting;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.Queries.Drivers;

public sealed class RewriteRulesDisplayDriver : DisplayDriver<RewriteRule>
{
    private readonly IRewriteRulesManager _rulesManager;

    internal readonly IStringLocalizer S;

    public RewriteRulesDisplayDriver(
        IRewriteRulesManager rulesManager,
        IStringLocalizer<RewriteRulesDisplayDriver> stringLocalizer)
    {
        _rulesManager = rulesManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(RewriteRule rule, BuildDisplayContext context)
    {
        return CombineAsync(
            Dynamic("RewriteRule_Fields_SummaryAdmin", model =>
            {
                model.Name = rule.Name;
                model.Source = rule.Source;
                model.Rule = rule;
            }).Location("Content:1"),
            Dynamic("RewriteRule_Buttons_SummaryAdmin", model =>
            {
                model.Name = rule.Name;
                model.Source = rule.Source;
                model.Rule = rule;
            }).Location("Actions:5")
        );
    }

    public override Task<IDisplayResult> EditAsync(RewriteRule rule, BuildEditorContext context)
    {
        return CombineAsync(
            Initialize<EditRewriteRuleViewModel>("RewriteRule_Fields_Edit", model =>
            {
                model.Name = rule.Name;
                model.SkipFurtherRules = rule.SkipFurtherRules;
                model.Source = rule.Source;
                model.Rule = rule;
            }).Location("Content:1"),
            Initialize<EditRewriteRuleViewModel>("RewriteRule_Fields_Buttons", model =>
            {
                model.Name = rule.Name;
                model.Source = rule.Source;
                model.SkipFurtherRules = rule.SkipFurtherRules;
                model.Rule = rule;
            }).Location("Actions:5")
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(RewriteRule rule, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(rule, Prefix,
            m => m.Name,
            m => m.SkipFurtherRules,
            m => m.Source);

        if (string.IsNullOrEmpty(rule.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(rule.Name), S["Name is required"]);
        }

        return await EditAsync(rule, context);
    }
}
