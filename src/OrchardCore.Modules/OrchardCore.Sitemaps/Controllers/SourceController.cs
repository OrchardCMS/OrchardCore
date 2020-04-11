using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class SourceController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<SitemapSource> _displayManager;
        private readonly IEnumerable<ISitemapSourceFactory> _factories;
        private readonly ISitemapManager _sitemapManager;
        private readonly ISiteService _siteService;
        private readonly ISitemapCacheProvider _sitemapCacheProvider;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        private readonly dynamic New;
        private readonly IStringLocalizer S;
        private readonly IHtmlLocalizer H;

        public SourceController(
            IAuthorizationService authorizationService,
            IDisplayManager<SitemapSource> displayManager,
            IEnumerable<ISitemapSourceFactory> factories,
            ISitemapManager sitemapManager,
            ISiteService siteService,
            ISitemapCacheProvider sitemapCacheProvider,
            IUpdateModelAccessor updateModelAccessor,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IStringLocalizer<SourceController> stringLocalizer,
            IHtmlLocalizer<SourceController> htmlLocalizer)
        {
            _displayManager = displayManager;
            _factories = factories;
            _authorizationService = authorizationService;
            _sitemapManager = sitemapManager;
            _siteService = siteService;
            _sitemapCacheProvider = sitemapCacheProvider;
            _updateModelAccessor = updateModelAccessor;
            _notifier = notifier;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Create(string sitemapId, string sourceType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var source = _factories.FirstOrDefault(x => x.Name == sourceType)?.Create();

            if (source == null)
            {
                return NotFound();
            }

            var model = new CreateSourceViewModel
            {
                SitemapId = sitemapId,
                SitemapSource = source,
                SitemapSourceId = source.Id,
                SitemapSourceType = sourceType,
                Editor = await _displayManager.BuildEditorAsync(source, updater: _updateModelAccessor.ModelUpdater, isNew: true)
            };

            model.Editor.SitemapSource = source;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSourceViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.LoadSitemapAsync(model.SitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var source = _factories.FirstOrDefault(x => x.Name == model.SitemapSourceType)?.Create();

            if (source == null)
            {
                return NotFound();
            }

            dynamic editor = await _displayManager.UpdateEditorAsync(source, updater: _updateModelAccessor.ModelUpdater, isNew: true);
            editor.SitemapStep = source;

            if (ModelState.IsValid)
            {
                source.Id = model.SitemapSourceId;
                sitemap.SitemapSources.Add(source);

                // Clear sitemap cache when new source added.
                await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);
                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                _notifier.Success(H["Sitemap source added successfully"]);
                return RedirectToAction("Display", "Admin", new { sitemapId = model.SitemapId });
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string sitemapId, string sourceId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var source = sitemap.SitemapSources.FirstOrDefault(x => String.Equals(x.Id, sourceId, StringComparison.OrdinalIgnoreCase));

            if (source == null)
            {
                return NotFound();
            }

            var model = new EditSourceViewModel
            {
                SitemapId = sitemapId,
                SitemapSource = source,
                SitemapSourceId = source.Id,
                Editor = await _displayManager.BuildEditorAsync(source, updater: _updateModelAccessor.ModelUpdater, isNew: false)
            };

            model.Editor.SitemapSource = source;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSourceViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.LoadSitemapAsync(model.SitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var source = sitemap.SitemapSources.FirstOrDefault(x => String.Equals(x.Id, model.SitemapSourceId, StringComparison.OrdinalIgnoreCase));

            if (source == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(source, updater: _updateModelAccessor.ModelUpdater, isNew: false);

            if (ModelState.IsValid)
            {
                // Clear sitemap cache when source edited.
                await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);

                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                _notifier.Success(H["Sitemap source updated successfully"]);
                return RedirectToAction("Display", "Admin", new { sitemapId = model.SitemapId });
            }

            _notifier.Error(H["The sitemap source has validation errors"]);
            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string sitemapId, string sourceId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.LoadSitemapAsync(sitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var source = sitemap.SitemapSources.FirstOrDefault(x => String.Equals(x.Id, sourceId, StringComparison.OrdinalIgnoreCase));

            if (source == null)
            {
                return NotFound();
            }

            sitemap.SitemapSources.Remove(source);

            // Clear sitemap cache when new source deleted.
            await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);

            await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

            _notifier.Success(H["Sitemap source deleted successfully"]);

            return RedirectToAction("Display", "Admin", new { sitemapId });
        }
    }
}
