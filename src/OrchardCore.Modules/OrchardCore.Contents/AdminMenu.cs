using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Security;
using OrchardCore.Entities;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Contents
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public AdminMenu(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IStringLocalizer<AdminMenu> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _authorizationService = authorizationService;
            _siteService = siteService;
            S = localizer;
        }

        public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            var context = _httpContextAccessor.HttpContext;

            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);
            var contentTypes = contentTypeDefinitions.Where(ctd => ctd.IsCreatable()).OrderBy(ctd => ctd.DisplayName);
            await builder.AddAsync(S["Content"], NavigationConstants.AdminMenuContentPosition, async content =>
            {
                content.AddClass("content").Id("content");
                await content.AddAsync(S["Content Items"], S["Content Items"].PrefixPosition(), async contentItems =>
                {
                    if (!await _authorizationService.AuthorizeContentTypeDefinitionsAsync(context.User, CommonPermissions.ListContent, contentTypes, _contentManager))
                    {
                        contentItems.Permission(Permissions.ListContent);
                    }

                    contentItems.Action(nameof(AdminController.List), typeof(AdminController).ControllerName(), new { area = "OrchardCore.Contents", contentTypeId = "" });
                    contentItems.LocalNav();
                });
            });

            var adminSettings = (await _siteService.GetSiteSettingsAsync()).As<AdminSettings>();

            if (adminSettings.DisplayNewMenu && contentTypes.Any())
            {
                await builder.AddAsync(S["New"], "-1", async newMenu =>
                {
                    newMenu.LinkToFirstChild(false).AddClass("new").Id("new");
                    foreach (var contentTypeDefinition in contentTypes)
                    {
                        var ci = await _contentManager.NewAsync(contentTypeDefinition.Name);
                        var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);
                        var createRouteValues = cim.CreateRouteValues;
                        createRouteValues.Add("returnUrl", _linkGenerator.GetPathByRouteValues(context, "", new
                        {
                            area = "OrchardCore.Contents",
                            controller = "Admin",
                            action = "List"
                        }));

                        if (createRouteValues.Any())
                        {
                            newMenu.Add(new LocalizedString(contentTypeDefinition.DisplayName, contentTypeDefinition.DisplayName), "5", item => item
                                .Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues)
                                .Permission(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditOwnContent.Name], contentTypeDefinition))
                                );
                        }
                    }
                });
            }
        }
    }
}
