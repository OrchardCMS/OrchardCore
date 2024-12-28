using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
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
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;

    public ScopeController(
        IOpenIdScopeManager scopeManager,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IStringLocalizer<ScopeController> stringLocalizer,
        IAuthorizationService authorizationService,
        ShellSettings shellSettings,
        INotifier notifier)
    {
        _scopeManager = scopeManager;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        S = stringLocalizer;
        _authorizationService = authorizationService;
        _shellSettings = shellSettings;
        _notifier = notifier;
    }

    [Admin("OpenId/Scope", "OpenIdScope")]
    public async Task<ActionResult> Index(PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var count = await _scopeManager.CountAsync();

        var model = new OpenIdScopeIndexViewModel
        {
            Pager = await _shapeFactory.PagerAsync(pager, (int)count),
        };

        await foreach (var scope in _scopeManager.ListAsync(pager.PageSize, pager.GetStartIndex()))
        {
            model.Scopes.Add(new OpenIdScopeEntry
            {
                Description = await _scopeManager.GetDescriptionAsync(scope),
                DisplayName = await _scopeManager.GetDisplayNameAsync(scope),
                Id = await _scopeManager.GetPhysicalIdAsync(scope),
                Name = await _scopeManager.GetNameAsync(scope)
            });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
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
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
        {
            return Forbid();
        }

        if (await _scopeManager.FindByNameAsync(model.Name) != null)
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
            Name = model.Name
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
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
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
            Name = await _scopeManager.GetNameAsync(scope)
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
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
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
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageScopes))
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
