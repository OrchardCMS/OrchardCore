using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;
using OrchardCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.UrlRewriting.Services;
using OrchardCore.UrlRewriting.Rules;
using OrchardCore.UrlRewriting.Helpers;

namespace OrchardCore.UrlRewriting.Drivers;

internal sealed class RewriteRuleDisplayDriver : DisplayDriver<RewriteRule>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly RewriteRulesStore _rewriteRulesStore;

    internal readonly IStringLocalizer S;

    public RewriteRuleDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        RewriteRulesStore rewriteRulesStore,
        IStringLocalizer<RewriteRuleDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _rewriteRulesStore = rewriteRulesStore;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(RewriteRule rule, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return null;
        }

        return Initialize<RewriteRuleViewModel>("RewriteRuleFields_Edit", viewModel => BuildViewModel(rule, viewModel, context.IsNew))
       .Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(RewriteRule rule, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return null;
        }

        var model = new RewriteRuleViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var rules = await _rewriteRulesStore.LoadRewriteRulesAsync();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RewriteRuleViewModel.Name), "The rule name is required.");
        }
        else if (context.IsNew && rules.Rules.Any(x => string.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RewriteRuleViewModel.Name), "The rule name already exists.");
        }
        if (string.IsNullOrWhiteSpace(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RewriteRuleViewModel.Pattern), "The url match pattern is required.");
        }
        if (model.RuleAction == RuleAction.Rewrite && string.IsNullOrWhiteSpace(model.RewriteAction.RewriteUrl))
        {
            context.Updater.ModelState.AddModelError(Prefix, "RewriteAction.RewriteUrl", "The rewrite url substitition is required.");
        }
        if (model.RuleAction == RuleAction.Redirect && string.IsNullOrWhiteSpace(model.RedirectAction.RedirectUrl))
        {
            context.Updater.ModelState.AddModelError(Prefix, "RedirectAction.RedirectUrl", "The redirect url substitition is required.");
        }

        if (context.Updater.ModelState.IsValid)
        {
            if (ApacheRuleValidator.ValidateRule(model, out var message) == false)
            {
                context.Updater.ModelState.AddModelError(Prefix, "RuleError", S["Rule error: {0}", message]);
            }
        }

        rule.Name = model.Name;
        rule.Pattern = model.Pattern;
        rule.Substitution = model.RuleAction == RuleAction.Rewrite ? model.RewriteAction.RewriteUrl : model.RedirectAction.RedirectUrl;
        rule.Flags = ApacheRules.FlagsFromViewModel(model);

        return await EditAsync(rule, context);
    }

    private void BuildViewModel(RewriteRule model, RewriteRuleViewModel viewModel, bool isNew)
    {
        viewModel.FromModel(model);

        viewModel.IsNew = isNew;

        viewModel.AvailableActions.Add(new SelectListItem() { Text = S["Rewrite"], Value = ((int)RuleAction.Rewrite).ToString() });
        viewModel.AvailableActions.Add(new SelectListItem() { Text = S["Redirect"], Value = ((int)RuleAction.Redirect).ToString() });

        viewModel.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Moved Permanently (301)"], Value = ((int)RedirectType.MovedPermanently301).ToString() });
        viewModel.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Found (302)"], Value = ((int)RedirectType.Found302).ToString() });
        viewModel.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Temporary Redirect (307)"], Value = ((int)RedirectType.TemporaryRedirect307).ToString() });
        viewModel.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Pernament Redirect (308)"], Value = ((int)RedirectType.PernamentRedirect308).ToString() });
    }
}
