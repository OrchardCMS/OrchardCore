using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.UrlRewriting.Rules;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Controllers;

[Admin("UrlRewriting/{action}/{id?}", "UrlRewriting{action}")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        S = stringLocalizer;
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
        var model = new CreateUrlRewriteRuleViewModel();

        BuildViewModel(model);

        return View(model);
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
