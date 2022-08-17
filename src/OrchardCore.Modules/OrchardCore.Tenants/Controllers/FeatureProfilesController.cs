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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
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
        private readonly INotifier _notifier;
        private readonly PagerOptions _pagerOptions;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public FeatureProfilesController(
            IAuthorizationService authorizationService,
            FeatureProfilesManager featueProfilesManager,
            INotifier notifier,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            IStringLocalizer<FeatureProfilesController> stringLocalizer,
            IHtmlLocalizer<FeatureProfilesController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _featureProfilesManager = featueProfilesManager;
            _notifier = notifier;
            _pagerOptions = pagerOptions.Value;
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

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);
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
                FeatureProfiles = featureProfiles.Select(x => new FeatureProfileEntry { Name = x.Key, FeatureProfile = x.Value }).ToList(),
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

            return View(new FeatureProfileViewModel());
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(FeatureProfileViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            List<FeatureRule> featureRules = null;

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["The name is mandatory."]);
                }
                else
                {
                    var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

                    if (featureProfilesDocument.FeatureProfiles.ContainsKey(model.Name))
                    {
                        ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["A profile with the same name already exists."]);
                    }
                }

                if (String.IsNullOrEmpty(model.FeatureRules))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["The feature rules are mandatory."]);
                }
                else
                {
                    try
                    {
                        featureRules = JsonConvert.DeserializeObject<List<FeatureRule>>(model.FeatureRules);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["Invalid json supplied."]);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var template = new FeatureProfile
                {
                    FeatureRules = featureRules
                };

                await _featureProfilesManager.UpdateFeatureProfileAsync(model.Name, template);

                if (submit == "SaveAndContinue")
                {
                    return RedirectToAction(nameof(Edit), new { name = model.Name });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            var featureProfilesDocument = await _featureProfilesManager.GetFeatureProfilesDocumentAsync();

            if (!featureProfilesDocument.FeatureProfiles.ContainsKey(name))
            {
                return RedirectToAction(nameof(Create), new { name });
            }

            var featureProfile = featureProfilesDocument.FeatureProfiles[name];

            var model = new FeatureProfileViewModel
            {
                Name = name,
                FeatureRules = JsonConvert.SerializeObject(featureProfile.FeatureRules, Formatting.Indented)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string sourceName, FeatureProfileViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTenantFeatureProfiles))
            {
                return Forbid();
            }

            var featureProfilesDocument = await _featureProfilesManager.LoadFeatureProfilesDocumentAsync();

            if (!featureProfilesDocument.FeatureProfiles.ContainsKey(sourceName))
            {
                return NotFound();
            }

            List<FeatureRule> featureRules = null;

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["The name is mandatory."]);
                }
                else if (!String.Equals(model.Name, sourceName, StringComparison.OrdinalIgnoreCase)
                    && featureProfilesDocument.FeatureProfiles.ContainsKey(model.Name))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.Name), S["A feature profile with the same name already exists."]);
                }

                if (String.IsNullOrEmpty(model.FeatureRules))
                {
                    ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["The feature rules are mandatory."]);
                }
                else
                {
                    try
                    {
                        featureRules = JsonConvert.DeserializeObject<List<FeatureRule>>(model.FeatureRules);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(nameof(FeatureProfileViewModel.FeatureRules), S["Invalid json supplied."]);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var featureProfile = new FeatureProfile
                {
                    FeatureRules = featureRules
                };

                await _featureProfilesManager.RemoveFeatureProfileAsync(sourceName);

                await _featureProfilesManager.UpdateFeatureProfileAsync(model.Name, featureProfile);

                if (submit == "SaveAndContinue")
                {
                    return RedirectToAction(nameof(Edit), new { name = model.Name });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
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
