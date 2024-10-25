using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminMenu.Services;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.AdminMenu.Controllers;

[Admin("AdminMenu/{action}/{id?}", "AdminMenu{action}")]
public sealed class MenuController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly IAdminMenuService _adminMenuService;
    private readonly PagerOptions _pagerOptions;
    private readonly IShapeFactory _shapeFactory;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public MenuController(
        IAuthorizationService authorizationService,
        IAdminMenuService adminMenuService,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IStringLocalizer<MenuController> stringLocalizer,
        IHtmlLocalizer<MenuController> htmlLocalizer,
        ILogger<MenuController> logger)
    {
        _authorizationService = authorizationService;
        _adminMenuService = adminMenuService;
        _pagerOptions = pagerOptions.Value;
        _shapeFactory = shapeFactory;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
        _logger = logger;
    }

    public async Task<IActionResult> List(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var adminMenuList = (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu;

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            adminMenuList = adminMenuList.Where(x => x.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var startIndex = pager.GetStartIndex();
        var pageSize = pager.PageSize;
        IEnumerable<Models.AdminMenu> results = [];

        // todo: handle the case where there is a deserialization exception on some of the presets.
        // load at least the ones without error. Provide a way to delete the ones on error.
        try
        {
            results = adminMenuList
            .Skip(startIndex)
            .Take(pageSize)
            .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when retrieving the list of admin menus.");
            await _notifier.ErrorAsync(H["Error when retrieving the list of admin menus."]);
        }

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, adminMenuList.Count, routeData);

        var model = new AdminMenuListViewModel
        {
            AdminMenu = results.Select(x => new AdminMenuEntry { AdminMenu = x }).ToList(),
            Options = options,
            Pager = pagerShape,
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(List))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(AdminMenuListViewModel model)
        => RedirectToAction(nameof(List), new RouteValueDictionary
        {
            {_optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        var model = new AdminMenuCreateViewModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminMenuCreateViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var tree = new Models.AdminMenu { Name = model.Name };

            await _adminMenuService.SaveAsync(tree);

            return RedirectToAction(nameof(List));
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        var adminMenuList = await _adminMenuService.GetAdminMenuListAsync();
        var adminMenu = _adminMenuService.GetAdminMenuById(adminMenuList, id);

        if (adminMenu == null)
        {
            return NotFound();
        }

        var model = new AdminMenuEditViewModel
        {
            Id = adminMenu.Id,
            Name = adminMenu.Name
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AdminMenuEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        var adminMenuList = await _adminMenuService.LoadAdminMenuListAsync();
        var adminMenu = _adminMenuService.GetAdminMenuById(adminMenuList, model.Id);

        if (adminMenu == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            adminMenu.Name = model.Name;

            await _adminMenuService.SaveAsync(adminMenu);

            await _notifier.SuccessAsync(H["Admin menu updated successfully."]);

            return RedirectToAction(nameof(List));
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        var adminMenuList = await _adminMenuService.LoadAdminMenuListAsync();
        var adminMenu = _adminMenuService.GetAdminMenuById(adminMenuList, id);

        if (adminMenu == null)
        {
            await _notifier.ErrorAsync(H["Can't find the admin menu."]);
            return RedirectToAction(nameof(List));
        }

        var removed = await _adminMenuService.DeleteAsync(adminMenu);

        if (removed == 1)
        {
            await _notifier.SuccessAsync(H["Admin menu deleted successfully."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["Can't delete the admin menu."]);
        }

        return RedirectToAction(nameof(List));
    }

    [HttpPost, ActionName(nameof(List))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var adminMenuList = (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu;
            var checkedContentItems = adminMenuList.Where(x => itemIds.Contains(x.Id));
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        var adminMenu = adminMenuList.FirstOrDefault(x => string.Equals(x.Id, item.Id, StringComparison.OrdinalIgnoreCase));
                        await _adminMenuService.DeleteAsync(adminMenu);
                    }
                    await _notifier.SuccessAsync(H["Admin menus successfully removed."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.BulkAction.ToString(), "Invalid bulk action.");
            }
        }

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
        {
            return Forbid();
        }

        var adminMenuList = await _adminMenuService.LoadAdminMenuListAsync();
        var adminMenu = _adminMenuService.GetAdminMenuById(adminMenuList, id);

        if (adminMenu == null)
        {
            return NotFound();
        }

        adminMenu.Enabled = !adminMenu.Enabled;

        await _adminMenuService.SaveAsync(adminMenu);

        await _notifier.SuccessAsync(H["Admin menu toggled successfully."]);

        return RedirectToAction(nameof(List));
    }
}
