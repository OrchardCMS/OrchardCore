using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers;

[Feature("OrchardCore.Tenants.FeatureProfiles")]
[Admin("TenantFeatureProfiles/{action}/{id?}", "TenantFeatureProfiles{action}")]
public sealed class FeatureProfilesController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly FeatureProfilesManager _featureProfilesManager;
    private readonly INotifier _notifier;
    private readonly PagerOptions _pagerOptions;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public FeatureProfilesController(
        IAuthorizationService authorizationService,
        FeatureProfilesManager featureProfilesManager,
        INotifier notifier,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        IStringLocalizer<FeatureProfilesController> stringLocalizer,
        IHtmlLocalizer<FeatureProfilesController> htmlLocalizer
        )
    {
        _authorizationService = authorizationService;
        _featureProfilesManager = featureProfilesManager;
        _notifier = notifier;
        _pagerOptions = pagerOptions.Value;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("TenantFeatureProfiles", "TenantFeatureProfilesIndex")]
    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

        var featureProfiles = featureProfilesDocument.FeatureProfiles.ToList();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            featureProfiles = featureProfiles.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var count = featureProfiles.Count;

        featureProfiles = featureProfiles.OrderBy(x => x.Key)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);

        var model = new FeatureProfilesIndexViewModel
        {
            FeatureProfiles = featureProfiles.Select(x => new FeatureProfileEntry
            {
                Name = x.Value.Name ?? x.Key,
                FeatureProfile = x.Value,
                Id = x.Key
            }).ToList(),
            Options = options,
            Pager = pagerShape
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(FeatureProfilesIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
        {
            return Forbid();
        }

        var viewModel = new FeatureProfileViewModel()
        {
            Id = IdGenerator.GenerateId(),
        };

        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(FeatureProfileViewModel model, string submit)
    {
        return await ProcessSaveAsync(model, submit, true, async (profile) =>
        {
            await _featureProfilesManager.UpdateFeatureProfileAsync(profile.Id, profile);
        });
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
        {
            return Forbid();
        }

        var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

        if (!featureProfilesDocument.FeatureProfiles.TryGetValue(id, out var featureProfile))
        {
            return NotFound();
        }

        var model = new FeatureProfileViewModel
        {
            // For backward compatibility, we use the name as id where id does not exists
            // the id is immutable whereas the name is mutable
            Id = featureProfile.Id ?? id,
            Name = featureProfile.Name ?? id,
            FeatureRules = JConvert.SerializeObject(featureProfile.FeatureRules, JOptions.Indented),
        };

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(FeatureProfileViewModel model, string submit)
    {
        return await ProcessSaveAsync(model, submit, false, async (profile) =>
        {
            await _featureProfilesManager.RemoveFeatureProfileAsync(profile.Id);

            await _featureProfilesManager.UpdateFeatureProfileAsync(profile.Id, profile);
        });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
        {
            return Forbid();
        }

        var featureProfilesDocument = await _featureProfilesManager.LoadFeatureProfilesDocumentAsync();

        if (!featureProfilesDocument.FeatureProfiles.ContainsKey(id))
        {
            return NotFound();
        }

        await _featureProfilesManager.RemoveFeatureProfileAsync(id);

        await _notifier.SuccessAsync(H["Feature profile deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var featureProfilesDocument = await _featureProfilesManager.LoadFeatureProfilesDocumentAsync();
            var checkItems = featureProfilesDocument.FeatureProfiles.Where(x => itemIds.Contains(x.Key));
            switch (options.BulkAction)
            {
                case ContentsBulkAction.Remove:
                    foreach (var item in checkItems)
                    {
                        await _featureProfilesManager.RemoveFeatureProfileAsync(item.Key);
                    }
                    await _notifier.SuccessAsync(H["Feature profiles successfully removed."]);
                    break;
                default:
                    break;
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ProcessSaveAsync(FeatureProfileViewModel model, string submit, bool isNew, Func<FeatureProfile, Task> onSuccessAsync)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
        {
            return Forbid();
        }

        var profile = new FeatureProfile();

        if (ModelState.IsValid)
        {
            try
            {
                profile.Id = model.Id;
                profile.Name = model.Name;
                profile.FeatureRules = JConvert.DeserializeObject<List<FeatureRule>>(model.FeatureRules);

                var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

                if (FeatureExists(profile, featureProfilesDocument, isNew))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["A feature profile with the same name already exists."]);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["Invalid json supplied."]);
            }
        }

        if (ModelState.IsValid)
        {
            await onSuccessAsync(profile);

            if (submit == "SaveAndContinue")
            {
                return RedirectToAction(nameof(Edit), new { id = profile.Id });
            }

            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    private static bool FeatureExists(FeatureProfile model, FeatureProfilesDocument featureProfilesDocument, bool isNew)
    {
        // For backward compatibility, we use the key value as the name when the new name property is not set.
        var profiles = featureProfilesDocument.FeatureProfiles.Where(x => (x.Value.Name ?? x.Key) == model.Name);

        return profiles.Any(x => isNew || x.Key != model.Id);
    }
}
