using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    [Feature("OrchardCore.Tenants.FeatureProfiles")]
    [Admin]
    public class FeatureProfilesController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly FeatureProfilesManager _featureProfilesManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public FeatureProfilesController(
            IAuthorizationService authorizationService,
            FeatureProfilesManager featueProfilesManager,
            ISiteService siteService,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _featureProfilesManager = featueProfilesManager;
            _siteService = siteService;
            _notifier = notifier;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

            var featureProfiles = featureProfilesDocument.FeatureProfiles.ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                featureProfiles = featureProfiles.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var count = featureProfiles.Count;

            featureProfiles = featureProfiles.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize).ToList();

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var model = new FeatureProfilesIndexViewModel
            {
                FeatureProfiles = featureProfiles.Select(x => new FeatureProfileEntry { Name = x.Value.Name ?? x.Key, FeatureProfile = x.Value, Id = x.Key }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            return View("Index", model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(FeatureProfilesIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            return View(new FeatureProfileViewModel()
            {
                Id = IdGenerator.GenerateId()
            });
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(FeatureProfileViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
               var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

                if (featureProfilesDocument.FeatureProfiles.Any(x => x.Key != model.Id && (x.Value.Name ?? x.Key).Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["A feature profile with the same name already exists."]);
                }

                try
                {
                    var featureRules = JsonConvert.DeserializeObject<List<FeatureRule>>(model.FeatureRules);

                    var template = new FeatureProfile
                    {
                        FeatureRules = featureRules,
                        Name = model.Name,
                    };

                    await _featureProfilesManager.UpdateFeatureProfileAsync(model.Id, template);

                    if (submit == "SaveAndContinue")
                    {
                        return RedirectToAction(nameof(Edit), new { name = model.Id });
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["Invalid json supplied."]);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            if(string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

            if (!featureProfilesDocument.FeatureProfiles.ContainsKey(id))
            {
                return RedirectToAction(nameof(Create), new { id });
            }

            var featureProfile = featureProfilesDocument.FeatureProfiles[id];

            var model = new FeatureProfileViewModel
            {
                Id = id,
                Name = featureProfile.Name ?? id,
                FeatureRules = JsonConvert.SerializeObject(featureProfile.FeatureRules, Formatting.Indented)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(FeatureProfileViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var featureProfilesDocument = await _featureProfilesManager.LoadFeatureProfilesDocumentAsync();

                if (!featureProfilesDocument.FeatureProfiles.TryGetValue(model.Id, out FeatureProfile feature))
                {
                    return NotFound();
                }

                if (featureProfilesDocument.FeatureProfiles.Any(x => x.Key != model.Id && (x.Value.Name ?? x.Key).Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["A feature profile with the same name already exists."]);
                }
               
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var featureRules = JsonConvert.DeserializeObject<List<FeatureRule>>(model.FeatureRules);

                    var featureProfile = new FeatureProfile
                    {
                        Name = model.Name,
                        FeatureRules = featureRules
                    };

                    await _featureProfilesManager.RemoveFeatureProfileAsync(model.Id);

                    await _featureProfilesManager.UpdateFeatureProfileAsync(model.Id, featureProfile);

                    if (submit == "SaveAndContinue")
                    {
                        return RedirectToAction(nameof(Edit), new { name = model.Name, id = model.Id });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["Invalid json supplied."]);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            var featureProfilesDocument = await _featureProfilesManager.LoadFeatureProfilesDocumentAsync();

            if (!featureProfilesDocument.FeatureProfiles.ContainsKey(name))
            {
                return NotFound();
            }

            await _featureProfilesManager.RemoveFeatureProfileAsync(name);

            await _notifier.SuccessAsync(H["Feature profile deleted successfully."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Index")]
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
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkItems)
                        {
                            await _featureProfilesManager.RemoveFeatureProfileAsync(item.Key);
                        }
                        await _notifier.SuccessAsync(H["Feature profiles successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
