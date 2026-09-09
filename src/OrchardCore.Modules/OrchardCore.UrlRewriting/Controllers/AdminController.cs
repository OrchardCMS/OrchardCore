using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell;
using OrchardCore.Routing;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.ViewModels;

namespace OrchardCore.UrlRewriting.Controllers;

[Admin("UrlRewriting/{action}/{id?}", "UrlRewriting{action}")]
public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly IDisplayManager<RewriteRule> _rewriteRuleDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IServiceProvider _serviceProvider;
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
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(
        RewriteRuleOptions options,
        [FromServices] IShapeFactory shapeFactory,
        [FromServices] IAdminListService adminListService)
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
            var shape = await _rewriteRuleDisplayManager.BuildDisplayAsync(rule, _updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.SummaryAdmin);

            if (shape is Shape rowShape)
            {
                // The rules are dragged to be reordered, and the sortable script only moves the rows of that class.
                rowShape.Classes.Add("item");

                // The row carries the value used by the client-side search of the list-management script.
                rowShape.Attributes["data-filter-value"] = rule.Name?.ToLowerInvariant();
            }

            model.Rules.Add(new RewriteRuleEntry
            {
                Rule = rule,
                Shape = shape,
            });
        }

        model.Options.BulkActions =
        [
            new SelectListItem(S["Delete"], nameof(RewriteRuleAction.Remove)),
        ];

        var toolbar = await shapeFactory.CreateAsync("AdminListToolbar", Arguments.From(new
        {
            ItemsCount = model.Rules.Count,
            TotalItemCount = model.Rules.Count,
            StartIndex = model.Rules.Count > 0 ? 1 : 0,
            EndIndex = model.Rules.Count,
            BulkActions = model.Options.BulkActions,
        }));

        var layout = await adminListService.GetLayoutAsync(UrlRewritingAdminList.Name, cancellationToken: HttpContext.RequestAborted);

        // The rules are reordered by dragging them, which the Grid layout cannot express: its rows are laid out by
        // the grid itself and have no box to drag, so the Table layout, which has the same columns, is used instead.
        if (string.Equals(layout, AdminListConstants.Grid, StringComparison.OrdinalIgnoreCase))
        {
            layout = AdminListConstants.Table;
        }

        // The AdminList shape renders the rules with the configured layout (List, Table, ...).
        model.List = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
        {
            Name = UrlRewritingAdminList.Name,
            Layout = layout,
            Columns = await adminListService.GetColumnsAsync(UrlRewritingAdminList.Name, UrlRewritingAdminList.GetDefaultColumns(S), HttpContext.RequestAborted),
            Rows = model.Rules.Select(entry => entry.Shape).ToList(),
            // The rules are evaluated in order, so the element holding the rows is the one the sortable script reorders.
            RowsAttributes = new Dictionary<string, string> { ["id"] = UrlRewritingAdminList.SortableContainerId },
            Toolbar = toolbar,
            ItemCssClass = "list-group-item",
            EmptyMessage = H["<strong>Nothing here!</strong> There are no rewrite rules at the moment."],
        }));

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

        if (ruleIds?.Any() == true)
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
