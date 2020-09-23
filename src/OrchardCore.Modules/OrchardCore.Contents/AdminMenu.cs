using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Controllers;
using OrchardCore.Contents.Security;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Contents
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly IStringLocalizer S;

        public AdminMenu(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            IStringLocalizer<AdminMenu> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            S = localizer;
        }

        public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);

            builder.Add(S["Content"], NavigationConstants.AdminMenuContentPosition, content => content
                .AddClass("content").Id("content")
                .Add(S["Content Items"], S["Content Items"].PrefixPosition(), contentItems => contentItems
                    .Permission(Permissions.EditOwnContent)
                    .Action(nameof(AdminController.List), typeof(AdminController).ControllerName(), new { area = "OrchardCore.Contents", contentTypeId = "" })
                    .LocalNav())
                );
            var contentTypes = contentTypeDefinitions.Where(ctd => ctd.GetSettings<ContentTypeSettings>().Creatable).OrderBy(ctd => ctd.DisplayName);
            if (contentTypes.Any())
            {
                await builder.AddAsync(S["New"], "-1", async newMenu =>
                {
                    newMenu.LinkToFirstChild(false).AddClass("new").Id("new");
                    foreach (var contentTypeDefinition in contentTypes)
                    {
                        var ci = await _contentManager.NewAsync(contentTypeDefinition.Name);
                        var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);
                        var createRouteValues = cim.CreateRouteValues;
                        createRouteValues.Add("returnUrl", _linkGenerator.GetPathByRouteValues(_httpContextAccessor.HttpContext, "", new
                        {
                            area = "OrchardCore.Contents",
                            controller = "Admin",
                            action = "List"
                        }));
                        if (createRouteValues.Any())
                            newMenu.Add(new LocalizedString(contentTypeDefinition.DisplayName, contentTypeDefinition.DisplayName), "5", item => item
                                .Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues)
                                .Permission(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[Permissions.PublishOwnContent.Name], contentTypeDefinition))
                                );
                    }
                });
            }
        }
    }
}
