using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Media.Models;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Controllers
{
    [Admin]
    public class MediaProfilesController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly MediaProfilesManager _mediaProfilesManager;
        private readonly MediaOptions _mediaOptions;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public MediaProfilesController(
            IAuthorizationService authorizationService,
            MediaProfilesManager mediaProfilesManager,
            IOptions<MediaOptions> mediaOptions,
            ISiteService siteService,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _mediaProfilesManager = mediaProfilesManager;
            _mediaOptions = mediaOptions.Value;
            _siteService = siteService;
            _notifier = notifier;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
            {
                return Forbid();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

            var count = mediaProfilesDocument.MediaProfiles.Count;

            var mediaProfiles = mediaProfilesDocument.MediaProfiles.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var model = new MediaProfileIndexViewModel
            {
                MediaProfiles = mediaProfiles.Select(x => new MediaProfileEntry { Name = x.Key, MediaProfile = x.Value }).ToList(),
                Pager = pagerShape
            };

            return View("Index", model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
            {
                return Forbid();
            }

            var model = new MediaProfileViewModel();

            model.AvailableWidths.Add(new SelectListItem() { Text = S["Undefined"], Value = "" });
            model.AvailableHeights.Add(new SelectListItem() { Text = S["Undefined"], Value = "" });

            model.AvailableWidths.AddRange(_mediaOptions.SupportedSizes.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }));
            model.AvailableHeights.AddRange(_mediaOptions.SupportedSizes.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }));

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(MediaProfileViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(MediaProfileViewModel.Name), S["The name is mandatory."]);
                }
                else
                {
                    var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

                    if (mediaProfilesDocument.MediaProfiles.ContainsKey(model.Name))
                    {
                        ModelState.AddModelError(nameof(MediaProfileViewModel.Name), S["A profile with the same name already exists."]);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var mediaProfile = new MediaProfile
                {
                    Hint = model.Hint,
                    Width = model.SelectedWidth,
                    Height = model.SelectedHeight,
                    Mode = model.SelectedMode,
                    Format = model.SelectedFormat,
                    Quality = model.Quality

                };

                await _mediaProfilesManager.UpdateMediaProfileAsync(model.Name, mediaProfile);

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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
            {
                return Forbid();
            }

            var mediaProfilesDocument = await _mediaProfilesManager.GetMediaProfilesDocumentAsync();

            if (!mediaProfilesDocument.MediaProfiles.ContainsKey(name))
            {
                return RedirectToAction("Create", new { name });
            }

            var mediaProfile = mediaProfilesDocument.MediaProfiles[name];

            var model = new MediaProfileViewModel
            {
                Name = name,
                Hint = mediaProfile.Hint,
                SelectedWidth = mediaProfile.Width,
                SelectedHeight = mediaProfile.Height,
                SelectedMode = mediaProfile.Mode,
                SelectedFormat = mediaProfile.Format,
                Quality = mediaProfile.Quality
            };

            model.AvailableWidths.Add(new SelectListItem() { Text = S["Unspecified"], Value = "" });
            model.AvailableHeights.Add(new SelectListItem() { Text = S["Unspecified"], Value = "" });
            model.AvailableWidths.AddRange(_mediaOptions.SupportedSizes.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }));
            model.AvailableHeights.AddRange(_mediaOptions.SupportedSizes.Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() }));

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string sourceName, MediaProfileViewModel model, string submit)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
            {
                return Forbid();
            }

            var mediaProfilesDocument = await _mediaProfilesManager.LoadMediaProfilesDocumentAsync();

            if (!mediaProfilesDocument.MediaProfiles.ContainsKey(sourceName))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(MediaProfileViewModel.Name), S["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var mediaProfile = new MediaProfile
                {
                    Hint = model.Hint,
                    Width = model.SelectedWidth,
                    Height = model.SelectedHeight,
                    Mode = model.SelectedMode,
                    Format = model.SelectedFormat,
                    Quality = model.Quality

                };

                await _mediaProfilesManager.RemoveMediaProfileAsync(sourceName);

                await  _mediaProfilesManager.UpdateMediaProfileAsync(model.Name, mediaProfile);

                if (submit != "SaveAndContinue")
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
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaProfiles))
            {
                return Forbid();
            }

            var mediaProfilesDocument = await _mediaProfilesManager.LoadMediaProfilesDocumentAsync();

            if (!mediaProfilesDocument.MediaProfiles.ContainsKey(name))
            {
                return NotFound();
            }

            await _mediaProfilesManager.RemoveMediaProfileAsync(name);

            _notifier.Success(H["Media profile deleted successfully"]);

            return RedirectToAction(nameof(Index));
        }
    }
}
