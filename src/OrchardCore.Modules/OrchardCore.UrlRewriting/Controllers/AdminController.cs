using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Controllers;

[Admin("UrlRewriting/{action}/{id?}", "UrlRewriting{action}")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly RewriteRulesStore _rewriteRulesStore;
    private readonly IDisplayManager<RewriteRule> _rewriteRuleDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IDisplayManager<RewriteRule> rewriteRuleDisplayManager,
        IAuthorizationService authorizationService,
        RewriteRulesStore rewriteRulesStore,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _rewriteRuleDisplayManager = rewriteRuleDisplayManager;
        _authorizationService = authorizationService;
        _rewriteRulesStore = rewriteRulesStore;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rules = await _rewriteRulesStore.GetRewriteRulesAsync();

        var model = new RewriteRulesViewModel
        {
            Rules = rules.Rules.ToList()
        };

        model.Rules.Add(new RewriteRule() { Name = "Rule One" });
        model.Rules.Add(new RewriteRule() { Name = "Rule Two" });
        model.Rules.Add(new RewriteRule() { Name = "Rule Three" });
        model.Rules.Add(new RewriteRule() { Name = "Rule Four" });

        return View(model);
    }

    [Admin("UrlRewriting/CreateRule", nameof(CreateRule))]
    public async Task<ActionResult> CreateRule()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = new RewriteRule();

        var shape = await _rewriteRuleDisplayManager.BuildEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: true, string.Empty, string.Empty);

        return View(shape);
    }

    [HttpPost, ActionName(nameof(CreateRule))]
    public async Task<ActionResult> CreateRulePOST()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = new RewriteRule();

        var shape = await _rewriteRuleDisplayManager.UpdateEditorAsync(rule, updater: _updateModelAccessor.ModelUpdater, isNew: true, string.Empty, string.Empty);

        if (!ModelState.IsValid)
        {
            return View(shape);
        }

        await _rewriteRulesStore.CreateAsync(rule);

        await _notifier.SuccessAsync(H["Rule created successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRule(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var currentRule = await _rewriteRulesStore.FindByIdAsync(id);

        if (currentRule == null)
        {
            return NotFound();
        }

        await _rewriteRulesStore.DeleteAsync(currentRule);

        await _notifier.SuccessAsync(H["Rule deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    /*
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
    */
}
