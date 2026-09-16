using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Rules;
using OrchardCore.Settings;

namespace OrchardCore.Layers.Controllers;

[Admin("Layers/{action}/{id?}", "Layers.{action}")]
public sealed class AdminController : Controller
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly ISiteService _siteService;
    private readonly ILayerService _layerService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ILayerWidgetService _widgets;
    private readonly IDisplayManager<Condition> _conditionDisplayManager;
    private readonly IDisplayManager<Rule> _ruleDisplayManager;
    private readonly IEnumerable<IConditionFactory> _conditionFactories;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IContentDefinitionManager contentDefinitionManager,
        IContentItemDisplayManager contentItemDisplayManager,
        ISiteService siteService,
        ILayerService layerService,
        IAuthorizationService authorizationService,
        IUpdateModelAccessor updateModelAccessor,
        ILayerWidgetService widgets,
        IDisplayManager<Condition> conditionDisplayManager,
        IDisplayManager<Rule> ruleDisplayManager,
        IEnumerable<IConditionFactory> conditionFactories,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        INotifier notifier,
        ILogger<AdminController> logger)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _siteService = siteService;
        _layerService = layerService;
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        _widgets = widgets;
        _conditionDisplayManager = conditionDisplayManager;
        _ruleDisplayManager = ruleDisplayManager;
        _conditionFactories = conditionFactories;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
        _logger = logger;
    }

    [Admin("Layers", "Layers.Index")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        var layers = await _layerService.GetLayersAsync();
        var widgets = await _layerService.GetLayerWidgetsMetadataAsync(c => c.Latest == true);

        var model = new LayersIndexViewModel { Layers = layers.Layers.ToList() };

        var contentDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        model.Zones = (await _siteService.GetSettingsAsync<LayerSettings>()).Zones ?? [];
        model.Widgets = [];

        foreach (var widget in widgets.OrderBy(x => x.Position))
        {
            var zone = widget.Zone;
            List<dynamic> list;
            if (!model.Widgets.TryGetValue(zone, out list))
            {
                model.Widgets.Add(zone, list = []);
            }

            if (contentDefinitions.Any(c => c.Name == widget.ContentItem.ContentType))
            {
                list.Add(await _contentItemDisplayManager.BuildDisplayAsync(widget.ContentItem, _updateModelAccessor.ModelUpdater, "SummaryAdmin"));
            }
            else
            {
                _logger.LogWarning("The Widget content item with id {ContentItemId} has no matching {ContentType} content type definition.", widget.ContentItem.ContentItemId, widget.ContentItem.ContentType);
                await _notifier.WarningAsync(H["The Widget content item with id {0} has no matching {1} content type definition.", widget.ContentItem.ContentItemId, widget.ContentItem.ContentType]);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        return View();
    }

    [HttpPost, ActionName("Create")]
    public async Task<IActionResult> CreatePost(LayerEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var result = await _layerService.CreateAsync(model.Name, model.Description);
            if (result.Status == LayerMutationStatus.Success)
            {
                return RedirectToAction(nameof(Edit), new { name = result.Layer.Name });
            }
            ModelState.AddModelError(nameof(LayerEditViewModel.Name), result.Error);
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        var layer = await _layerService.GetLayerAsync(name);

        if (layer == null)
        {
            return NotFound();
        }

        var rule = await _ruleDisplayManager.BuildDisplayAsync(layer.LayerRule, _updateModelAccessor.ModelUpdater, "Summary");
        rule.Properties["ConditionId"] = layer.LayerRule.ConditionId;

        var thumbnails = new Dictionary<string, dynamic>();
        foreach (var factory in _conditionFactories)
        {
            var condition = factory.Create();
            var thumbnail = await _conditionDisplayManager.BuildDisplayAsync(condition, _updateModelAccessor.ModelUpdater, "Thumbnail");
            thumbnail.Properties["Condition"] = condition;
            thumbnail.Properties["TargetUrl"] = Url.ActionLink("Create", "LayerRule", new { name, type = factory.Name });
            thumbnails.Add(factory.Name, thumbnail);
        }

        var model = new LayerEditViewModel
        {
            Name = layer.Name,
            Description = layer.Description,
            LayerRule = rule,
            Thumbnails = thumbnails,
        };

        return View(model);
    }

    [HttpPost, ActionName("Edit")]
    public async Task<IActionResult> EditPost(LayerEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            // Editing the layer's metadata leaves its rule and condition identities intact.
            var result = await _layerService.UpdateAsync(model.Name, model.Description);
            if (result.Status == LayerMutationStatus.NotFound)
            {
                return NotFound();
            }
            if (result.Status == LayerMutationStatus.Success)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(nameof(LayerEditViewModel.Name), result.Error);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Forbid();
        }

        var result = await _layerService.DeleteAsync(name);
        if (result.Status == LayerMutationStatus.NotFound)
        {
            return NotFound();
        }
        if (result.Status == LayerMutationStatus.Success)
        {
            await _notifier.SuccessAsync(H["Layer deleted successfully."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["The layer couldn't be deleted: you must remove any associated widgets first."]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePosition(string contentItemId, double position, string zone)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLayers))
        {
            return Unauthorized();
        }

        var result = await _widgets.UpdateAsync(User, contentItemId,
            new LayerMetadata { Position = position, Zone = zone }, positionOnly: true);
        if (result.Status == LayerWidgetMutationStatus.NotFound)
        {
            return NotFound();
        }
        if (result.Status == LayerWidgetMutationStatus.Forbidden)
        {
            return Forbid();
        }
        if (result.Status == LayerWidgetMutationStatus.Invalid)
        {
            return BadRequest(result.Errors);
        }

        if (Request.Headers != null && Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            return Ok();
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
