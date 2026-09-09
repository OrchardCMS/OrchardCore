using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Controllers;

[Feature(OpenIdConstants.Features.Management)]
[Admin("OpenId/Scope/{action}/{id?}", "OpenIdScope{action}")]
public sealed class ScopeController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ShellSettings _shellSettings;
    private readonly IOpenIdScopeManager _scopeManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public ScopeController(
        IOpenIdScopeManager scopeManager,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IStringLocalizer<ScopeController> stringLocalizer,
        IHtmlLocalizer<ScopeController> htmlLocalizer,
        IAuthorizationService authorizationService,
        ShellSettings shellSettings)
    {
        _scopeManager = scopeManager;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        S = stringLocalizer;
        H = htmlLocalizer;
        _authorizationService = authorizationService;
        _shellSettings = shellSettings;
    }

    [Admin("OpenId/Scope", "OpenIdScope")]
    public async Task<ActionResult> Index(
        PagerParameters pagerParameters,
        string searchText,
        [FromServices] IDisplayManager<OpenIdScopeEntry> displayManager,
        [FromServices] IUpdateModelAccessor updateModelAccessor,
        [FromServices] IAdminListService adminListService)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageScopes))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions);

        // The manager can only page, not filter, so the search runs over the whole set before paging, the
        // same way the placements list does. The number of scopes a tenant defines is small.
        var scopes = new List<OpenIdScopeEntry>();

        await foreach (var scope in _scopeManager.ListAsync(null, null))
        {
            scopes.Add(new OpenIdScopeEntry
            {
                Description = await _scopeManager.GetDescriptionAsync(scope),
                DisplayName = await _scopeManager.GetDisplayNameAsync(scope),
                Id = await _scopeManager.GetPhysicalIdAsync(scope),
                Name = await _scopeManager.GetNameAsync(scope),
            });
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            scopes = scopes
                .Where(x => (x.DisplayName != null && x.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    || (x.Name != null && x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var count = scopes.Count;

        // Maintain the search when generating page links.
        RouteData routeData = null;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            routeData = new RouteData();
            routeData.Values[nameof(searchText)] = searchText;
        }

        var model = new OpenIdScopeIndexViewModel
        {
            SearchText = searchText,
            Pager = await _shapeFactory.PagerAsync(pager, count, routeData),
        };

        foreach (var entry in scopes.OrderBy(x => x.DisplayName).ThenBy(x => x.Name).Skip(pager.GetStartIndex()).Take(pager.PageSize))
        {
            model.Scopes.Add(entry);
        }

        var rows = new List<object>(model.Scopes.Count);

        foreach (var entry in model.Scopes)
        {
            rows.Add(await displayManager.BuildDisplayAsync(entry, updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.SummaryAdmin));
        }

        // The AdminList shape renders the scopes with the configured layout (List, Table, ...).
        model.List = await _shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
        {
            Name = OpenIdScopesAdminList.Name,
            Layout = await adminListService.GetLayoutAsync(OpenIdScopesAdminList.Name, cancellationToken: HttpContext.RequestAborted),
            Columns = await adminListService.GetColumnsAsync(OpenIdScopesAdminList.Name, OpenIdScopesAdminList.GetDefaultColumns(S), HttpContext.RequestAborted),
            Rows = rows,
            Pager = model.Pager,
            ItemCssClass = "list-group-item",
            EmptyMessage = H["<strong>Nothing here!</strong> There are no scopes at the moment."],
        }));

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageScopes))
        {
            return Forbid();
        }

        var model = new CreateOpenIdScopeViewModel();

        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOpenIdScopeViewModel model, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageScopes))
        {
            return Forbid();
        }

        if (ModelState.IsValid && await _scopeManager.FindByNameAsync(model.Name) != null)
        {
            ModelState.AddModelError(nameof(model.Name), S["The name is already taken by another scope."]);
        }

        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        var descriptor = new OpenIdScopeDescriptor
        {
            Description = model.Description,
            DisplayName = model.DisplayName,
            Name = model.Name,
        };

        if (!string.IsNullOrEmpty(model.Resources))
        {
            if (model.Resources.Contains(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Resources), S["The resources field cannot contain the value: {0}.", OpenIdConstants.Prefixes.Tenant + _shellSettings.Name]);

                ViewData["ReturnUrl"] = returnUrl;

                return View(model);
            }

            descriptor.Resources.UnionWith(model.Resources.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        await _scopeManager.CreateAsync(descriptor);

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction("Index");
        }

        return this.LocalRedirect(returnUrl, true);
    }

    public async Task<IActionResult> Edit(string id, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageScopes))
        {
            return Forbid();
        }

        var scope = await _scopeManager.FindByPhysicalIdAsync(id);

        if (scope == null)
        {
            return NotFound();
        }

        var model = new EditOpenIdScopeViewModel
        {
            Description = await _scopeManager.GetDescriptionAsync(scope),
            DisplayName = await _scopeManager.GetDisplayNameAsync(scope),
            Id = await _scopeManager.GetPhysicalIdAsync(scope),
            Name = await _scopeManager.GetNameAsync(scope),
        };

        var resources = (await _scopeManager.GetResourcesAsync(scope))
            .Where(resource => !string.IsNullOrEmpty(resource));

        model.Resources = string.Join(' ', resources);

        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditOpenIdScopeViewModel model, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageScopes))
        {
            return Forbid();
        }

        var scope = await _scopeManager.FindByPhysicalIdAsync(model.Id);

        if (scope == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var other = await _scopeManager.FindByNameAsync(model.Name);

            if (other != null && !string.Equals(await _scopeManager.GetIdAsync(other), await _scopeManager.GetIdAsync(scope), StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.Name), S["The name is already taken by another scope."]);
            }
        }

        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        var descriptor = new OpenIdScopeDescriptor();
        await _scopeManager.PopulateAsync(descriptor, scope);

        descriptor.Description = model.Description;
        descriptor.DisplayName = model.DisplayName;
        descriptor.Name = model.Name;

        descriptor.Resources.Clear();

        if (!string.IsNullOrEmpty(model.Resources))
        {
            if (model.Resources.Contains(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Resources), S["The resources field cannot contain the value: {0}.", OpenIdConstants.Prefixes.Tenant + _shellSettings.Name]);

                ViewData["ReturnUrl"] = returnUrl;

                return View(model);
            }

            descriptor.Resources.UnionWith(model.Resources.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        await _scopeManager.UpdateAsync(scope, descriptor);

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction(nameof(Index));
        }

        return this.LocalRedirect(returnUrl, true);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageScopes))
        {
            return Forbid();
        }

        var scope = await _scopeManager.FindByPhysicalIdAsync(id);

        if (scope == null)
        {
            return NotFound();
        }

        await _scopeManager.DeleteAsync(scope);

        return RedirectToAction(nameof(Index));
    }
}
