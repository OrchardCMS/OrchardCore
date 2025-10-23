using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Catalogs;
using OrchardCore.Catalogs.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Security.Core;

namespace OrchardCore.Security.Controllers;

[Feature(SecurityConstants.Features.Credentials)]
public class CredentialsController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly INamedSourceCatalogManager<Credential> _manager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IDisplayManager<Credential> _displayDriver;
    private readonly SecurityOptions _securityOptions;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public CredentialsController(
        INamedSourceCatalogManager<Credential> manager,
        IAuthorizationService authorizationService,
        IUpdateModelAccessor updateModelAccessor,
        IShellReleaseManager shellReleaseManager,
        IDisplayManager<Credential> displayManager,
        IOptions<SecurityOptions> securityOptions,
        INotifier notifier,
        IHtmlLocalizer<CredentialsController> htmlLocalizer,
        IStringLocalizer<CredentialsController> stringLocalizer)
    {
        _manager = manager;
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        _shellReleaseManager = shellReleaseManager;
        _displayDriver = displayManager;
        _securityOptions = securityOptions.Value;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("security/credentials", "SecurityCredentials")]
    public async Task<IActionResult> Index(
        CatalogEntryOptions options,
        PagerParameters pagerParameters,
        [FromServices] IOptions<PagerOptions> pagerOptions,
        [FromServices] IShapeFactory shapeFactory)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());

        var result = await _manager.PageAsync(pager.Page, pager.PageSize, new QueryContext
        {
            Sorted = true,
            Name = options.Search,
        });

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var viewModel = new ListSourceCatalogEntryViewModel<Credential>
        {
            Models = [],
            Options = options,
            Pager = await shapeFactory.PagerAsync(pager, result.Count, routeData),
            Sources = _securityOptions.CredentialProviders.Keys.Order(),
        };

        foreach (var model in result.Entries)
        {
            viewModel.Models.Add(new CatalogEntryViewModel<Credential>
            {
                Model = model,
                Shape = await _displayDriver.BuildDisplayAsync(model, _updateModelAccessor.ModelUpdater, "SummaryAdmin"),
            });
        }

        viewModel.Options.BulkActions =
        [
            new SelectListItem(S["Delete"], nameof(CatalogEntryAction.Remove)),
        ];

        return View(viewModel);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    [Admin("security/credentials", "SecurityCredentials")]
    public ActionResult IndexFilterPost(ListCatalogEntryViewModel model)
    {
        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options?.Search },
        });
    }

    [Admin("security/credentials/create/{providerName}", "SecurityCredentialsCreate")]
    public async Task<ActionResult> Create(string providerName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        if (!_securityOptions.CredentialProviders.TryGetValue(providerName, out var connectionSource))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider with the name '{0}'.", providerName]);

            return RedirectToAction(nameof(Index));
        }

        var model = await _manager.NewAsync(providerName);

        var viewModel = new EditCatalogEntryViewModel
        {
            DisplayName = connectionSource.DisplayName,
            Editor = await _displayDriver.BuildEditorAsync(model, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        return View(viewModel);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [Admin("security/credentials/create/{providerName}", "SecurityCredentialsCreate")]
    public async Task<ActionResult> CreatePost(string providerName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        if (!_securityOptions.CredentialProviders.TryGetValue(providerName, out var connectionSource))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider with the name '{0}'.", providerName]);

            return RedirectToAction(nameof(Index));
        }

        var model = await _manager.NewAsync(providerName);

        var viewModel = new EditCatalogEntryViewModel
        {
            DisplayName = model.DisplayText,
            Editor = await _displayDriver.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        if (ModelState.IsValid)
        {
            _shellReleaseManager.RequestRelease();

            await _manager.CreateAsync(model);
            await _notifier.SuccessAsync(H["A new credential has been created successfully."]);

            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    [Admin("security/credentials/edit/{id}", "SecurityCredentialsEdit")]
    public async Task<ActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        var model = await _manager.FindByIdAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        var viewModel = new EditCatalogEntryViewModel
        {
            DisplayName = model.DisplayText,
            Editor = await _displayDriver.BuildEditorAsync(model, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        return View(viewModel);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [Admin("security/credentials/edit/{id}", "SecurityCredentialsEdit")]
    public async Task<ActionResult> EditPost(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        var model = await _manager.FindByIdAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        var viewModel = new EditCatalogEntryViewModel
        {
            DisplayName = model.DisplayText,
            Editor = await _displayDriver.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        if (ModelState.IsValid)
        {
            _shellReleaseManager.RequestRelease();

            await _manager.UpdateAsync(model);

            await _notifier.SuccessAsync(H["The credential has been updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    [HttpPost]
    [Admin("security/credentials/delete/{id}", "SecurityCredentialsDelete")]

    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        var model = await _manager.FindByIdAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        if (await _manager.DeleteAsync(model))
        {
            _shellReleaseManager.RequestRelease();

            await _notifier.SuccessAsync(H["The credential has been deleted successfully."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["Unable to remove the credential."]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    [Admin("security/credentials", "SecurityCredentials")]
    public async Task<ActionResult> IndexPost(CatalogEntryOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecurityConstants.Permissions.ManageCredentials))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            switch (options.BulkAction)
            {
                case CatalogEntryAction.None:
                    break;
                case CatalogEntryAction.Remove:
                    var counter = 0;
                    foreach (var id in itemIds)
                    {
                        var instance = await _manager.FindByIdAsync(id);

                        if (instance == null)
                        {
                            continue;
                        }

                        if (await _manager.DeleteAsync(instance))
                        {
                            counter++;
                        }
                    }
                    if (counter == 0)
                    {
                        await _notifier.WarningAsync(H["No credentials were removed."]);
                    }
                    else
                    {
                        _shellReleaseManager.RequestRelease();

                        await _notifier.SuccessAsync(H.Plural(counter, "1 credential has been removed successfully.", "{0} credentials have been removed successfully."));
                    }
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
