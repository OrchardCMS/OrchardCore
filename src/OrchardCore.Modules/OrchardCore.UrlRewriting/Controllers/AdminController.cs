using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Rules;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Controllers;

[Admin("UrlRewriting/{action}/{id?}", "UrlRewriting{action}")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    //private readonly IDisplayManager<UrlRewritingSettings> _urlRewritingSettingsDisplayManager;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        //IDisplayManager<UrlRewritingSettings> urlRewritingSettingsDisplayManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        //_urlRewritingSettingsDisplayManager = urlRewritingSettingsDisplayManager;
        _authorizationService = authorizationService;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        return View();
    }

    [Admin("UrlRewriting/Create", "CreateRule")]
    public async Task<ActionResult> CreateRule()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var model = new CreateUrlRewriteRuleViewModel();

        BuildViewModel(model);

        return View(model);
    }

    [HttpPost, ActionName("CreateRule")]
    public async Task<ActionResult> CreateRulePOST(CreateUrlRewriteRuleViewModel viewModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        viewModel.DisplayName = viewModel.DisplayName?.Trim() ?? string.Empty;
        viewModel.Pattern = viewModel.Pattern ?? string.Empty;
        viewModel.RewriteAction.RewriteUrl = viewModel.RewriteAction.RewriteUrl ?? string.Empty;
        viewModel.RedirectAction.RedirectUrl = viewModel.RedirectAction.RedirectUrl ?? string.Empty;

        if (string.IsNullOrWhiteSpace(viewModel.DisplayName))
        {
            ModelState.AddModelError("DisplayName", S["The Display Name can't be empty."]);
        }
        if (string.IsNullOrWhiteSpace(viewModel.Pattern))
        {
            ModelState.AddModelError("Pattern", S["The Pattern can't be empty."]);
        }
        if (viewModel.RuleAction == RuleAction.Rewrite && string.IsNullOrEmpty(viewModel.RewriteAction.RewriteUrl))
        {
            ModelState.AddModelError("RewriteAction.RewriteUrl", S["The Rewrite URL can't be empty."]);
        }
        if (viewModel.RuleAction == RuleAction.Redirect && string.IsNullOrEmpty(viewModel.RedirectAction.RedirectUrl))
        {
            ModelState.AddModelError("RedirectAction.RedirectUrl", S["The Redirect URL can't be empty."]);
        }

        // If basic validation is ok, do final check
        if (ModelState.IsValid)
        {
            var rewriteRule = ApacheRuleBuilder.FromViewModel(viewModel, true);

            try
            {
                using var apacheModRewrite = new StringReader(rewriteRule);
                var rewriteOptions = new RewriteOptions();
                rewriteOptions.AddApacheModRewrite(apacheModRewrite);
            }
            catch (FormatException ex)
            {
                ModelState.AddModelError("Pattern", S["Parsing error: {0}", ex.Message]);
            }
            catch (NotImplementedException ex)
            {
                ModelState.AddModelError("Pattern", S["Parsing error: {0}", ex.Message]);
            }
        }

        if (ModelState.IsValid)
        {
            // TODO - save settings/reload rules
            await _notifier.SuccessAsync(H["URL rewriting rule created successfully."]);

            return RedirectToAction(nameof(Index));
        }

        BuildViewModel(viewModel);

        return View(viewModel);
    }

    private void BuildViewModel(CreateUrlRewriteRuleViewModel model)
    {
        model.AvailableActions.Add(new SelectListItem() { Text = S["Rewrite"], Value = ((int)RuleAction.Rewrite).ToString() });
        model.AvailableActions.Add(new SelectListItem() { Text = S["Redirect"], Value = ((int)RuleAction.Redirect).ToString() });

        model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Moved Permanently (301)"], Value = ((int)RedirectType.MovedPermanently301).ToString() });
        model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Found (302)"], Value = ((int)RedirectType.Found302).ToString() });
        model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Temporary Redirect (307)"], Value = ((int)RedirectType.TemporaryRedirect307).ToString() });
        model.RedirectAction.AvailableRedirectTypes.Add(new SelectListItem() { Text = S["Pernament Redirect (308)"], Value = ((int)RedirectType.PernamentRedirect308).ToString() });
    }
}
