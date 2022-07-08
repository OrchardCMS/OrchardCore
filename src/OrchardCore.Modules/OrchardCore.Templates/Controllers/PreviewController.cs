using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentPreview;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Controllers
{
    public class PreviewController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentHandleManager _contentHandleManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly string _homeUrl;

        public PreviewController(
            IContentManager contentManager,
            IContentHandleManager contentHandleManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            ShellSettings shellSettings,
            IUpdateModelAccessor updateModelAccessor)
        {
            _contentManager = contentManager;
            _contentHandleManager = contentHandleManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _authorizationService = authorizationService;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;
            _homeUrl = ('/' + (shellSettings.RequestUrlPrefix ?? string.Empty)).TrimEnd('/') + '/';
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Render()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageTemplates))
            {
                return this.ChallengeOrForbid();
            }

            // Mark request as a `Preview` request so that drivers / handlers or underlying services can be aware of an active preview mode.
            HttpContext.Features.Set(new ContentPreviewFeature());

            var name = Request.Form["Name"];
            var content = Request.Form["Content"];

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
            {
                HttpContext.Items["OrchardCore.PreviewTemplate"] = new TemplateViewModel { Name = name, Content = content };
            }

            var handle = Request.Form["Handle"].ToString();

            string contentItemId;

            if (string.IsNullOrEmpty(handle) || handle == _homeUrl)
            {
                var homeRoute = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
                contentItemId = homeRoute["contentItemId"]?.ToString();
            }
            else
            {
                var index = handle.IndexOf(_homeUrl, StringComparison.Ordinal);
                handle = (index < 0) ? handle : handle.Substring(_homeUrl.Length);
                contentItemId = await _contentHandleManager.GetContentItemIdAsync("slug:" + handle);
            }

            if (string.IsNullOrEmpty(contentItemId))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);

            if (contentItem == null)
            {
                return NotFound();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "Detail");

            return View(model);
        }
    }
}
