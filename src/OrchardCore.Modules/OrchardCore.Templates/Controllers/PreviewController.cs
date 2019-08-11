using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Controllers
{
    public class PreviewController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentAliasManager _contentAliasManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly string _homeUrl;

        public PreviewController(
            IContentManager contentManager,
            IContentAliasManager contentAliasManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            ShellSettings shellSettings)
        {
            _contentManager = contentManager;
            _contentAliasManager = contentAliasManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _authorizationService = authorizationService;
            _siteService = siteService;
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
                return Unauthorized();
            }

            var name = Request.Form["Name"];
            var content = Request.Form["Content"];

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
            {
                HttpContext.Items["OrchardCore.PreviewTemplate"] = new TemplateViewModel { Name = name, Content = content };
            }

            var alias = Request.Form["Alias"].ToString();

            string contentItemId = string.Empty;

            if (string.IsNullOrEmpty(alias) || alias == _homeUrl)
            {
                var homeRoute = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
                contentItemId = homeRoute["contentItemId"]?.ToString();
            }
            else
            {
                var index = alias.IndexOf(_homeUrl);
                alias = (index < 0) ? alias : alias.Substring(_homeUrl.Length);
                contentItemId = await _contentAliasManager.GetContentItemIdAsync("slug:" + alias);
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

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "Detail");

            return View(model);
        }
    }
}
