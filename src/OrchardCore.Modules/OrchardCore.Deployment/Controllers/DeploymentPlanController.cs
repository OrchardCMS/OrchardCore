using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment.Indexes;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Deployment.Controllers;

[Admin("DeploymentPlan/{action}/{id?}", "DeploymentPlan{action}")]
public sealed class DeploymentPlanController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<DeploymentStep> _displayManager;
    private readonly IEnumerable<IDeploymentStepFactory> _factories;
    private readonly ISession _session;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public DeploymentPlanController(
        IAuthorizationService authorizationService,
        IDisplayManager<DeploymentStep> displayManager,
        IEnumerable<IDeploymentStepFactory> factories,
        ISession session,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        IStringLocalizer<DeploymentPlanController> stringLocalizer,
        IHtmlLocalizer<DeploymentPlanController> htmlLocalizer,
        INotifier notifier,
        IUpdateModelAccessor updateModelAccessor)
    {
        _displayManager = displayManager;
        _factories = factories;
        _authorizationService = authorizationService;
        _session = session;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.Export))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var deploymentPlans = _session.Query<DeploymentPlan, DeploymentPlanIndex>();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            deploymentPlans = deploymentPlans.Where(x => x.Name.Contains(options.Search));
        }

        var count = await deploymentPlans.CountAsync();

        var results = await deploymentPlans
            .OrderBy(p => p.Name)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ListAsync();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);

        var model = new DeploymentPlanIndexViewModel
        {
            DeploymentPlans = results.Select(x => new DeploymentPlanEntry { DeploymentPlan = x }).ToList(),
            Options = options,
            Pager = pagerShape
        };

        model.Options.DeploymentPlansBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Delete)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(DeploymentPlanIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexBulkActionPOST(ContentOptions options, IEnumerable<long> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var checkedItems = await _session.Query<DeploymentPlan, DeploymentPlanIndex>().Where(x => x.DocumentId.IsIn(itemIds)).ListAsync();
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Delete:
                    foreach (var item in checkedItems)
                    {
                        _session.Delete(item);
                    }
                    await _notifier.SuccessAsync(H["Deployment plans successfully deleted."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.BulkAction.ToString(), "Invalid bulk action.");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Display(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        var items = new List<dynamic>();
        foreach (var step in deploymentPlan.DeploymentSteps)
        {
            var item = await _displayManager.BuildDisplayAsync(step, _updateModelAccessor.ModelUpdater, "Summary");
            item.Properties["DeploymentStep"] = step;
            items.Add(item);
        }

        var thumbnails = new Dictionary<string, dynamic>();
        foreach (var factory in _factories)
        {
            var step = factory.Create();
            var thumbnail = await _displayManager.BuildDisplayAsync(step, _updateModelAccessor.ModelUpdater, "Thumbnail");
            thumbnail.Properties["DeploymentStep"] = step;
            thumbnails.Add(factory.Name, thumbnail);
        }

        var model = new DisplayDeploymentPlanViewModel
        {
            DeploymentPlan = deploymentPlan,
            Items = items,
            Thumbnails = thumbnails,
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var model = new CreateDeploymentPlanViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeploymentPlanViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(CreateDeploymentPlanViewModel.Name), S["The name is mandatory."]);
            }

            var count = await _session.QueryIndex<DeploymentPlanIndex>(x => x.Name == model.Name).CountAsync();
            if (count > 0)
            {
                ModelState.AddModelError(nameof(CreateDeploymentPlanViewModel.Name), S["A deployment plan with the same name already exists."]);
            }
        }

        if (ModelState.IsValid)
        {
            var deploymentPlan = new DeploymentPlan { Name = model.Name };

            await _session.SaveAsync(deploymentPlan);

            return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        var model = new EditDeploymentPlanViewModel
        {
            Id = deploymentPlan.Id,
            Name = deploymentPlan.Name
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditDeploymentPlanViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(model.Id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(EditDeploymentPlanViewModel.Name), S["The name is mandatory."]);
            }
            if (!string.Equals(model.Name, deploymentPlan.Name, StringComparison.OrdinalIgnoreCase))
            {
                var count = await _session.QueryIndex<DeploymentPlanIndex>(x => x.Name == model.Name && x.DocumentId != model.Id).CountAsync();
                if (count > 0)
                {
                    ModelState.AddModelError(nameof(CreateDeploymentPlanViewModel.Name), S["A deployment plan with the same name already exists."]);
                }
            }
        }

        if (ModelState.IsValid)
        {
            deploymentPlan.Name = model.Name;

            await _session.SaveAsync(deploymentPlan);

            await _notifier.SuccessAsync(H["Deployment plan updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        _session.Delete(deploymentPlan);

        await _notifier.SuccessAsync(H["Deployment plan deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }
}
