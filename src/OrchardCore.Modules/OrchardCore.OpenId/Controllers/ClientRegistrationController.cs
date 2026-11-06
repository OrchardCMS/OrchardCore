using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Controllers;

[Feature(OpenIdConstants.Features.Client)]
[Admin("OpenId/ClientRegistrations/{action}/{name?}", "OpenIdClientRegistrations{action}")]
public sealed class ClientRegistrationController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IOpenIdClientRegistrationService _registrationService;
    private readonly IDisplayManager<OpenIdClientRegistration> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly INotifier _notifier;
    private readonly PagerOptions _pagerOptions;

    internal readonly IHtmlLocalizer H;

    public ClientRegistrationController(
        IAuthorizationService authorizationService,
        IOpenIdClientRegistrationService registrationService,
        IDisplayManager<OpenIdClientRegistration> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IShellReleaseManager shellReleaseManager,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IOptions<PagerOptions> pagerOptions,
        IHtmlLocalizer<ClientRegistrationController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _registrationService = registrationService;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _shellReleaseManager = shellReleaseManager;
        _shapeFactory = shapeFactory;
        _notifier = notifier;
        _pagerOptions = pagerOptions.Value;
        H = htmlLocalizer;
    }

    [Admin("OpenId/ClientRegistrations", "OpenIdClientRegistrations")]
    public async Task<IActionResult> Index(PagerParameters pagerParameters, string searchText)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageClientSettings))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions);

        IEnumerable<OpenIdClientRegistration> registrations = await _registrationService.GetRegistrationsAsync();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            registrations = registrations.Where(registration =>
                registration.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (registration.DisplayName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (registration.Authority?.AbsoluteUri.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var items = registrations
            .OrderBy(registration => registration.DisplayName ?? registration.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Maintain the search when generating page links.
        RouteData routeData = null;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            routeData = new RouteData();
            routeData.Values[nameof(searchText)] = searchText;
        }

        var model = new OpenIdClientRegistrationIndexViewModel
        {
            SearchText = searchText,

            // The AdminList shape renders the registrations with the configured layout (List, Grid, ...).
            List = new AdminListContext<OpenIdClientRegistration>
            {
                Source = items.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList(),
                Pager = await _shapeFactory.PagerAsync(pager, items.Count, routeData),
            },
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageClientSettings))
        {
            return Forbid();
        }

        var registration = CreateRegistration();

        return View(new OpenIdClientRegistrationEditorViewModel
        {
            Editor = await _displayManager.BuildEditorAsync(registration, _updateModelAccessor.ModelUpdater, isNew: true),
        });
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost()
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageClientSettings))
        {
            return Forbid();
        }

        var registration = CreateRegistration();

        var editor = await _displayManager.UpdateEditorAsync(registration, _updateModelAccessor.ModelUpdater, isNew: true);

        if (!ModelState.IsValid)
        {
            return View(new OpenIdClientRegistrationEditorViewModel
            {
                Editor = editor,
            });
        }

        await _registrationService.UpdateRegistrationAsync(registration);

        // The client registrations are read when the tenant builds its authentication options.
        _shellReleaseManager.RequestRelease();

        await _notifier.SuccessAsync(H["The client registration has been created successfully."]);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageClientSettings))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name) || await _registrationService.GetRegistrationAsync(name) is not { } registration)
        {
            return NotFound();
        }

        return View(new OpenIdClientRegistrationEditorViewModel
        {
            Name = registration.Name,
            DisplayName = registration.DisplayName ?? registration.Name,
            Editor = await _displayManager.BuildEditorAsync(registration, _updateModelAccessor.ModelUpdater, isNew: false),
        });
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageClientSettings))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name) || await _registrationService.LoadRegistrationAsync(name) is not { } registration)
        {
            return NotFound();
        }

        var editor = await _displayManager.UpdateEditorAsync(registration, _updateModelAccessor.ModelUpdater, isNew: false);

        if (!ModelState.IsValid)
        {
            return View(new OpenIdClientRegistrationEditorViewModel
            {
                Name = registration.Name,
                DisplayName = registration.DisplayName ?? registration.Name,
                Editor = editor,
            });
        }

        await _registrationService.UpdateRegistrationAsync(registration);

        _shellReleaseManager.RequestRelease();

        await _notifier.SuccessAsync(H["The client registration has been updated successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenIdPermissions.ManageClientSettings))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name) || !await _registrationService.DeleteRegistrationAsync(name))
        {
            return NotFound();
        }

        _shellReleaseManager.RequestRelease();

        await _notifier.SuccessAsync(H["The client registration has been deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    // The defaults match the ones of the editor of the client settings of the site.
    private static OpenIdClientRegistration CreateRegistration()
        => new()
        {
            ResponseType = OpenIdConstants.Client.ResponseTypes.Code,
            ResponseMode = OpenIddict.Abstractions.OpenIddictConstants.ResponseModes.FormPost,
        };
}
