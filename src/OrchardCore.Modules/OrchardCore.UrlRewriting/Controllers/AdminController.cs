using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.UrlRewriting.Helpers;
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
    private readonly IShellReleaseManager _shellReleaseManager;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IDisplayManager<RewriteRule> rewriteRuleDisplayManager,
        IAuthorizationService authorizationService,
        RewriteRulesStore rewriteRulesStore,
        INotifier notifier,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IUpdateModelAccessor updateModelAccessor)
    {
        _rewriteRuleDisplayManager = rewriteRuleDisplayManager;
        _authorizationService = authorizationService;
        _rewriteRulesStore = rewriteRulesStore;
        _notifier = notifier;
        _shellReleaseManager = shellReleaseManager;
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
            Rules = rules.Rules.Values
                .Select(BuildViewModel)
                .ToArray()
        };

        return View(model);
    }

    public async Task<ActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = new RewriteRule();

        var shape = await _rewriteRuleDisplayManager.BuildEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: true);

        return View(shape);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<ActionResult> CreatePOST()
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = new RewriteRule();

        var shape = await _rewriteRuleDisplayManager.UpdateEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: true);

        if (ModelState.IsValid)
        {
            rule.Id = IdGenerator.GenerateId();

            await _rewriteRulesStore.SaveAsync(rule);

            await _notifier.SuccessAsync(H["Rule created successfully."]);

            return RedirectToAction(nameof(Index));
        }

        _shellReleaseManager.SuspendReleaseRequest();

        return View(shape);
    }

    public async Task<ActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = await _rewriteRulesStore.FindByIdAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        var shape = await _rewriteRuleDisplayManager.BuildEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: false);

        return View(shape);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<ActionResult> EditPOST(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = await _rewriteRulesStore.FindByIdAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        var shape = await _rewriteRuleDisplayManager.UpdateEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: false);

        if (ModelState.IsValid)
        {
            await _rewriteRulesStore.SaveAsync(rule);

            await _notifier.SuccessAsync(H["Rule updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        _shellReleaseManager.SuspendReleaseRequest();

        return View(shape);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewriting))
        {
            return Forbid();
        }

        var rule = await _rewriteRulesStore.FindByIdAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        await _rewriteRulesStore.DeleteAsync(rule);

        _shellReleaseManager.RequestRelease();

        await _notifier.SuccessAsync(H["Rule deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    private static RewriteRuleViewModel BuildViewModel(RewriteRule rule)
    {
        var viewModel = new RewriteRuleViewModel();

        viewModel.FromModel(rule);

        return viewModel;
    }
}
