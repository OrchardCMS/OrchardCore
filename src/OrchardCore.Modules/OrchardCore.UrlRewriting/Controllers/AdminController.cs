using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Routing;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Controllers;

[Admin("UrlRewriting/{action}/{id?}", "UrlRewriting{action}")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly IDisplayManager<RewriteRule> _rewriteRuleDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IEnumerable<IUrlRewriteRuleSource> _urlRewritingRuleSources;
    private readonly IRewriteRulesManager _rewriteRulesManager;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IDisplayManager<RewriteRule> rewriteRuleDisplayManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IShellReleaseManager shellReleaseManager,
        IEnumerable<IUrlRewriteRuleSource> urlRewritingRuleSources,
        IRewriteRulesManager rewriteRulesManager,
        IUpdateModelAccessor updateModelAccessor,
        IServiceProvider serviceProvider,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer
        )
    {
        _rewriteRuleDisplayManager = rewriteRuleDisplayManager;
        _authorizationService = authorizationService;
        _notifier = notifier;
        _shellReleaseManager = shellReleaseManager;
        _urlRewritingRuleSources = urlRewritingRuleSources;
        _rewriteRulesManager = rewriteRulesManager;
        _updateModelAccessor = updateModelAccessor;
        _serviceProvider = serviceProvider;
        _logger = logger;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(RewriteRuleOptions options)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        var rules = await _rewriteRulesManager.GetAllAsync();

        var model = new ListRewriteRuleViewModel
        {
            Rules = [],
            Options = options,
            SourceNames = _urlRewritingRuleSources.Select(x => x.TechnicalName),
        };

        foreach (var rule in rules)
        {
            model.Rules.Add(new RewriteRuleEntry
            {
                Rule = rule,
                Shape = await _rewriteRuleDisplayManager.BuildDisplayAsync(rule, _updateModelAccessor.ModelUpdater, "SummaryAdmin")
            });
        }

        model.Options.BulkActions =
        [
            new SelectListItem(S["Delete"], nameof(RewriteRuleAction.Remove)),
        ];

        return View(model);
    }

    public async Task<ActionResult> Create(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        var ruleSource = _serviceProvider.GetKeyedService<IUrlRewriteRuleSource>(id);

        if (ruleSource == null)
        {
            await _notifier.ErrorAsync(H["Unable to find a rule-source that can handle the source '{Source}'.", id]);

            return RedirectToAction(nameof(Index));
        }

        var rule = await _rewriteRulesManager.NewAsync(id);

        if (rule == null)
        {
            await _notifier.ErrorAsync(H["Invalid rule source."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new RewriteRuleViewModel
        {
            DisplayName = ruleSource.DisplayName,
            Editor = await _rewriteRuleDisplayManager.BuildEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<ActionResult> CreatePOST(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        var ruleSource = _serviceProvider.GetKeyedService<IUrlRewriteRuleSource>(id);

        if (ruleSource == null)
        {
            await _notifier.ErrorAsync(H["Unable to find a rule-source that can handle the source '{Source}'.", id]);

            return RedirectToAction(nameof(Index));
        }

        var rule = await _rewriteRulesManager.NewAsync(id);

        if (rule == null)
        {
            await _notifier.ErrorAsync(H["Invalid rule source."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new RewriteRuleViewModel
        {
            DisplayName = ruleSource.DisplayName,
            Editor = await _rewriteRuleDisplayManager.UpdateEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        if (ModelState.IsValid)
        {
            await _rewriteRulesManager.SaveAsync(rule);

            await _notifier.SuccessAsync(H["Rule created successfully."]);

            return RedirectToAction(nameof(Index));
        }

        _shellReleaseManager.SuspendReleaseRequest();

        return View(model);
    }

    public async Task<ActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        var rule = await _rewriteRulesManager.FindByIdAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        var model = new RewriteRuleViewModel
        {
            DisplayName = rule.Name,
            Editor = await _rewriteRuleDisplayManager.BuildEditorAsync(rule, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<ActionResult> EditPOST(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        var rule = await _rewriteRulesManager.FindByIdAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        // Clone the rule to prevent modifying the original instance in the store.
        var ruleToUpdate = rule.Clone();

        var model = new RewriteRuleViewModel
        {
            DisplayName = ruleToUpdate.Name,
            Editor = await _rewriteRuleDisplayManager.UpdateEditorAsync(ruleToUpdate, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        if (ModelState.IsValid)
        {
            await _rewriteRulesManager.SaveAsync(ruleToUpdate);

            await _notifier.SuccessAsync(H["Rule updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        _shellReleaseManager.SuspendReleaseRequest();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        var rule = await _rewriteRulesManager.FindByIdAsync(id);

        if (rule == null)
        {
            return NotFound();
        }

        await _rewriteRulesManager.DeleteAsync(rule);

        _shellReleaseManager.RequestRelease();

        await _notifier.SuccessAsync(H["Rule deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(RewriteRuleOptions options, IEnumerable<string> ruleIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, UrlRewritingPermissions.ManageUrlRewritingRules))
        {
            return Forbid();
        }

        if (ruleIds?.Count() > 0)
        {
            switch (options.BulkAction)
            {
                case RewriteRuleAction.None:
                    break;
                case RewriteRuleAction.Remove:
                    foreach (var id in ruleIds)
                    {
                        var rule = await _rewriteRulesManager.FindByIdAsync(id);

                        if (rule == null)
                        {
                            continue;
                        }

                        await _rewriteRulesManager.DeleteAsync(rule);
                    }
                    await _notifier.SuccessAsync(H["Rules removed successfully."]);
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
