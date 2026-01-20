using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Drivers;

public sealed class RewriteRulesDisplayDriver : DisplayDriver<RewriteRule>
{
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    public RewriteRulesDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<RewriteRulesDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(RewriteRule rule, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RewriteRule_Fields_SummaryAdmin", rule).Location("Content:1"),
            View("RewriteRule_Buttons_SummaryAdmin", rule).Location("Actions:5"),
            View("RewriteRule_DefaultTags_SummaryAdmin", rule).Location("Tags:5"),
            View("RewriteRule_DefaultMeta_SummaryAdmin", rule).Location("Meta:5")
        );
    }

    public override IDisplayResult Edit(RewriteRule rule, BuildEditorContext context)
    {
        context.AddTenantReloadWarningWrapper();

        return Initialize<EditRewriteRuleViewModel>("RewriteRule_Fields_Edit", model =>
        {
            model.Name = rule.Name;
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(RewriteRule rule, UpdateEditorContext context)
    {
        var model = new EditRewriteRuleViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name);

        if (string.IsNullOrEmpty(model.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["Name is required"]);
        }

        rule.Name = model.Name;

        _shellReleaseManager.RequestRelease();

        return Edit(rule, context);
    }
}
