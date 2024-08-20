using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Placements.Services;
using OrchardCore.Placements.ViewModels;
using OrchardCore.Routing;

namespace OrchardCore.Placements.Controllers;

[Admin("Placements/{action}/{shapeType?}", "Placements.{action}")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly ILogger _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly PlacementsManager _placementsManager;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        ILogger<AdminController> logger,
        IAuthorizationService authorizationService,
        PlacementsManager placementsManager,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer,
        INotifier notifier,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory)
    {
        _logger = logger;
        _authorizationService = authorizationService;
        _placementsManager = placementsManager;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;

        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("Placements", "Placements.Index")]
    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var shapeTypes = await _placementsManager.ListShapePlacementsAsync();

        var shapeList = shapeTypes.Select(entry => new ShapePlacementViewModel
        {
            ShapeType = entry.Key
        }).ToList();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            shapeList = shapeList.Where(x => x.ShapeType.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var count = shapeList.Count;

        shapeList = shapeList.OrderBy(x => x.ShapeType)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);

        var model = new ListShapePlacementsViewModel
        {
            ShapePlacements = shapeList,
            Pager = pagerShape,
            Options = options,
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(ListShapePlacementsViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Create(string suggestion, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
        {
            return Forbid();
        }

        var template = new PlacementNode[] { new() };

        var viewModel = new EditShapePlacementViewModel
        {
            Creating = true,
            ShapeType = suggestion,
            Nodes = JConvert.SerializeObject(template, JOptions.Indented)
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(nameof(Edit), viewModel);
    }

    public async Task<IActionResult> Edit(string shapeType, string displayType = null, string contentType = null, string contentPart = null, string differentiator = null, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
        {
            return Forbid();
        }

        var placementNodes = (await _placementsManager.GetShapePlacementsAsync(shapeType))?.ToList() ?? [];

        if (placementNodes.Count == 0 || ShouldCreateNode(placementNodes, displayType, contentType, contentPart, differentiator))
        {
            var generatedNode = new PlacementNode
            {
                DisplayType = displayType,
                Differentiator = differentiator
            };

            if (!string.IsNullOrEmpty(contentType))
            {
                generatedNode.Filters.Add("contentType", new JsonArray(contentType));
            }

            if (!string.IsNullOrEmpty(contentPart))
            {
                generatedNode.Filters.Add("contentPart", new JsonArray(contentPart));
            }

            placementNodes.Add(generatedNode);
        }

        var viewModel = new EditShapePlacementViewModel
        {
            ShapeType = shapeType,
            Nodes = JConvert.SerializeObject(placementNodes, JOptions.Indented)
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<IActionResult> Edit(EditShapePlacementViewModel viewModel, string submit, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
        {
            return Forbid();
        }

        ViewData["ReturnUrl"] = returnUrl;

        if (viewModel.Creating && await _placementsManager.GetShapePlacementsAsync(viewModel.ShapeType) != null)
        {
            // Prevent overriding existing rules on creation.
            await _notifier.WarningAsync(H["Placement rules for \"{0}\" already exists. Please edit existing rule.", viewModel.ShapeType]);
            return View(viewModel);
        }

        try
        {
            var placementNodes = JConvert.DeserializeObject<PlacementNode[]>(viewModel.Nodes)
                ?? Enumerable.Empty<PlacementNode>();

            // Remove empty nodes.
            placementNodes = placementNodes.Where(node => !IsEmpty(node));

            if (placementNodes.Any())
            {
                // Save.
                await _placementsManager.UpdateShapePlacementsAsync(viewModel.ShapeType, placementNodes);
                viewModel.Creating = false;

                await _notifier.SuccessAsync(H["The \"{0}\" placement have been saved.", viewModel.ShapeType]);
            }
            else if (viewModel.Creating)
            {
                await _notifier.WarningAsync(H["The \"{0}\" placement is empty.", viewModel.ShapeType]);
                return View(viewModel);
            }
            else
            {
                // Remove if empty.
                await _placementsManager.RemoveShapePlacementsAsync(viewModel.ShapeType);
                await _notifier.SuccessAsync(H["The \"{0}\" placement has been deleted.", viewModel.ShapeType]);
            }
        }
        catch (JsonException jsonException)
        {
            await _notifier.ErrorAsync(H["An error occurred while parsing the placement<br/>{0}", jsonException.Message]);
            return View(viewModel);
        }
        catch (Exception e)
        {
            await _notifier.ErrorAsync(H["An error occurred while saving the placement."]);
            _logger.LogError(e, "An error occurred while saving the placement.");
            return View(viewModel);
        }

        if (submit != "SaveAndContinue")
        {
            return RedirectToReturnUrlOrIndex(returnUrl);
        }

        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Delete))]
    public async Task<IActionResult> Delete(string shapeType, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
        {
            return Forbid();
        }

        await _placementsManager.RemoveShapePlacementsAsync(shapeType);
        await _notifier.SuccessAsync(H["The \"{0}\" placement has been deleted.", shapeType]);

        return RedirectToReturnUrlOrIndex(returnUrl);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManagePlacements))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in itemIds)
                    {
                        await _placementsManager.RemoveShapePlacementsAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Placements successfully removed."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.BulkAction.ToString(), "Invalid bulk action.");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToReturnUrlOrIndex(string returnUrl)
    {
        if ((string.IsNullOrEmpty(returnUrl) == false) && (Url.IsLocalUrl(returnUrl)))
        {
            return this.Redirect(returnUrl, true);
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }
    }

    private static bool ShouldCreateNode(IEnumerable<PlacementNode> nodes, string displayType, string contentType, string contentPart, string differentiator)
    {
        if (string.IsNullOrEmpty(displayType) && string.IsNullOrEmpty(differentiator))
        {
            return false;
        }
        else
        {
            return !nodes.Any(node =>
                (string.IsNullOrEmpty(displayType) || node.DisplayType == displayType) &&
                (string.IsNullOrEmpty(contentType) || (node.Filters.ContainsKey("contentType") && FilterEquals(node.Filters["contentType"], contentType))) &&
                (string.IsNullOrEmpty(contentPart) || (node.Filters.ContainsKey("contentPart") && FilterEquals(node.Filters["contentPart"], contentPart))) &&
                (string.IsNullOrEmpty(differentiator) || node.Differentiator == differentiator));
        }
    }

    private static bool IsEmpty(PlacementNode node)
    {
        return string.IsNullOrEmpty(node.Location)
            && string.IsNullOrEmpty(node.ShapeType)
            && (node.Alternates == null || node.Alternates.Length == 0)
            && (node.Wrappers == null || node.Wrappers.Length == 0);
    }


    private static bool FilterEquals(object node, string value)
    {
        var jsonNode = JNode.FromObject(node);
        if (jsonNode is JsonArray jsonArray)
        {
            var tokenValues = jsonArray.Values<string>();
            return tokenValues.Count() == 1 && tokenValues.First() == value;
        }
        else
        {
            return jsonNode.Value<string>() == value;
        }
    }
}
