using Microsoft.Extensions.Localization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.Environment.Navigation;
using System;
using System.Linq;

namespace Orchard.Contents
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);

            builder.Add(T["Content"], "1.4", content => content
                .Permission(Permissions.EditOwnContent)
                .AddClass("content").Id("content")
                .Add(T["Content Items"], "1", contentItems => contentItems
                    .Action("List", "Admin", new { area = "Orchard.Contents" })
                    .LocalNav())
                );

            var contentTypes = contentTypeDefinitions.Where(ctd => ctd.Settings.ToObject<ContentTypeSettings>().Creatable).OrderBy(ctd => ctd.DisplayName);
            if (contentTypes.Any()) {
                builder.Add(T["New"], "-1", newMenu =>
                {
                    newMenu.LinkToFirstChild(false).AddClass("new").Id("new");
                    foreach (var contentTypeDefinition in contentTypes)
                    {
                        var ci = _contentManager.New(contentTypeDefinition.Name);
                        var cim = _contentManager.PopulateAspect<ContentItemMetadata>(ci);
                        var createRouteValues = cim.CreateRouteValues;
                        if (createRouteValues.Any())
                            newMenu.Add(new LocalizedString(contentTypeDefinition.DisplayName, contentTypeDefinition.DisplayName), "5", item => item
                                .Action(cim.CreateRouteValues["Action"] as string, cim.CreateRouteValues["Controller"] as string, cim.CreateRouteValues)
                                // Apply "PublishOwn" permission for the content type
                                //.Permission(DynamicPermissions.CreateDynamicPermission(DynamicPermissions.PermissionTemplates[Permissions.PublishOwnContent.Name], contentTypeDefinition)
                                );
                    }
                });
            }
        }
    }
}