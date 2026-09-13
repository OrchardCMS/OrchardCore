using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.Deployment.Controllers;

[Admin("DeploymentPlan/{action}/{id?}", "DeploymentPlan{action}")]
public sealed class DeploymentPlanController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<DeploymentStep> _displayManager;
    private readonly IEnumerable<IDeploymentStepFactory> _factories;
    private readonly IDeploymentPlanService _plans;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    /// <summary>Creates the admin editor using the shared tenant plan service.</summary>
    public DeploymentPlanController(
        IAuthorizationService authorizationService,
        IDisplayManager<DeploymentStep> displayManager,
        IEnumerable<IDeploymentStepFactory> factories,
        IDeploymentPlanService plans,
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
        _plans = plans;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        _updateModelAccessor = updateModelAccessor;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Export))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions);

        var page = await _plans.ListAsync(options.Search, pager.GetStartIndex(), pager.PageSize);
        var count = page.TotalCount;
        var results = page.Items;

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
            Pager = pagerShape,
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
            { _optionsSearch, model.Options.Search },
        });

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexBulkActionPOST(ContentOptions options, IEnumerable<long> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        if (itemIds?.Any() == true)
        {
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Delete:
                    foreach (var id in itemIds.Distinct())
                    {
                        await _plans.DeleteAsync(id);
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
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _plans.GetAsync(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        var items = new List<dynamic>();
        foreach (var step in deploymentPlan.DeploymentSteps)
        {
            var item = await _displayManager.BuildDisplayAsync(step, _updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.Summary);
            item.Properties["DeploymentStep"] = step;
            items.Add(item);
        }

        var thumbnails = new List<DisplayDeploymentPlanThumbnailViewModel>();
        foreach (var factory in _factories.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            var step = factory.Create();
            var thumbnail = await _displayManager.BuildDisplayAsync(step, _updateModelAccessor.ModelUpdater, "Thumbnail");
            thumbnail.Properties["DeploymentStep"] = step;
            var category = step.Category?.Value ?? string.Empty;

            thumbnails.Add(new DisplayDeploymentPlanThumbnailViewModel
            {
                Category = category,
                CategoryId = category.HtmlClassify(),
                Thumbnail = thumbnail,
                Type = factory.Name,
            });
        }

        var model = new DisplayDeploymentPlanViewModel
        {
            DeploymentPlan = deploymentPlan,
            Categories = thumbnails
                .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                .Select(x => new DisplayDeploymentPlanCategoryViewModel
                {
                    Name = x.Category,
                    Id = x.CategoryId,
                })
                .DistinctBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            Items = items,
            Thumbnails = thumbnails,
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var model = new CreateDeploymentPlanViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeploymentPlanViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var result = await _plans.CreateAsync(model.Name);
            if (result.Error == DeploymentPlanManagementError.None)
            {
                return RedirectToAction(nameof(Display), new { id = result.Plan.Id });
            }
            AddNameError(result.Error);
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _plans.GetAsync(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        var model = new EditDeploymentPlanViewModel
        {
            Id = deploymentPlan.Id,
            Name = deploymentPlan.Name,
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditDeploymentPlanViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _plans.GetAsync(model.Id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var result = await _plans.RenameAsync(model.Id, model.Name);
            if (result.Error == DeploymentPlanManagementError.NotFound)
            {
                return NotFound();
            }
            if (result.Error == DeploymentPlanManagementError.None)
            {
                await _notifier.SuccessAsync(H["Deployment plan updated successfully."]);
                return RedirectToAction(nameof(Index));
            }
            AddNameError(result.Error);
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ManageDeploymentPlan))
        {
            return Forbid();
        }

        var deploymentPlan = await _plans.GetAsync(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        await _plans.DeleteAsync(id);

        await _notifier.SuccessAsync(H["Deployment plan deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    private void AddNameError(DeploymentPlanManagementError error)
    {
        ModelState.AddModelError(nameof(CreateDeploymentPlanViewModel.Name), error == DeploymentPlanManagementError.MissingName
            ? S["The name is mandatory."]
            : S["A deployment plan with the same name already exists."]);
    }
}
