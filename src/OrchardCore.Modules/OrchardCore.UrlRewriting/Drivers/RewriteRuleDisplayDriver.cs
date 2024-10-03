using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.UrlRewriting.Helpers;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Rules;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Drivers;

internal sealed class RewriteRuleDisplayDriver : DisplayDriver<RewriteRule>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;

    public RewriteRuleDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<RewriteRuleDisplayDriver> stringLocalizer,
        IShellReleaseManager shellReleaseManager)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
        _shellReleaseManager = shellReleaseManager;
    }

    public override async Task<IDisplayResult> EditAsync(RewriteRule rule, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return null;
        }

        return Initialize<RewriteRuleViewModel>("RewriteRuleFields_Edit", model =>
        {
            model.FromModel(rule);

            model.AvailableActions.Add(new SelectListItem(S["Rewrite"], nameof(RuleAction.Rewrite)));
            model.AvailableActions.Add(new SelectListItem(S["Redirect"], nameof(RuleAction.Redirect)));

            model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem(S["Moved Permanently (301)"], nameof(RedirectType.MovedPermanently301)));
            model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem(S["Found (302)"], nameof(RedirectType.Found302)));
            model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem(S["Temporary Redirect (307)"], nameof(RedirectType.TemporaryRedirect307)));
            model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem(S["Permanent Redirect (308)"], nameof(RedirectType.PermanentRedirect308)));
        }).Location("Content:1");
    }

    public override async Task<IDisplayResult> UpdateAsync(RewriteRule rule, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return null;
        }

        var model = new RewriteRuleViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RewriteRuleViewModel.Name), "The rule name is required.");
        }

        if (string.IsNullOrWhiteSpace(model.Pattern))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(RewriteRuleViewModel.Pattern), "The url match pattern is required.");
        }

        if (model.RuleAction == RuleAction.Rewrite && string.IsNullOrWhiteSpace(model.RewriteAction.RewriteUrl))
        {
            context.Updater.ModelState.AddModelError(Prefix, "RewriteAction.RewriteUrl", "The rewrite url substitution is required.");
        }

        if (model.RuleAction == RuleAction.Redirect && string.IsNullOrWhiteSpace(model.RedirectAction.RedirectUrl))
        {
            context.Updater.ModelState.AddModelError(Prefix, "RedirectAction.RedirectUrl", "The redirect url substitution is required.");
        }
        if (ApacheRuleValidator.ValidateRule(model, out var message) == false)
        {
            context.Updater.ModelState.AddModelError(Prefix, "RuleError", S["Rule error: {0}", message]);
        }

        rule.Name = model.Name;
        rule.Pattern = model.Pattern;
        rule.Substitution = model.RuleAction == RuleAction.Rewrite
            ? model.RewriteAction.RewriteUrl
            : model.RedirectAction.RedirectUrl;
        rule.Flags = ApacheRules.FlagsFromViewModel(model);

        _shellReleaseManager.RequestRelease();

        return await EditAsync(rule, context);
    }
}
