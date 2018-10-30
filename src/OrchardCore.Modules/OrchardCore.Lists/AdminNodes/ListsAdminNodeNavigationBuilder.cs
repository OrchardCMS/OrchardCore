using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.AdminTrees.Services;
using OrchardCore.Navigation;
using YesSql;
using System.Threading.Tasks;

namespace OrchardCore.Lists.AdminNodes
{
    public class ListsAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly ILogger<ListsAdminNodeNavigationBuilder> _logger;

        public ListsAdminNodeNavigationBuilder(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session,
            ILogger<ListsAdminNodeNavigationBuilder> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _logger = logger;
        }

        public string Name => typeof(ListsAdminNode).Name;

        public async Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            var node = menuItem as ListsAdminNode;

            if ((node == null) || (!node.Enabled))
            {
                return;
            }

            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions().OrderBy(d => d.Name);
            var selectedNames = node.ContentTypes ?? new string[] {};

            var selected = contentTypeDefinitions
                .Where(ctd => selectedNames.ToList<string>().Contains(ctd.Name))
                .Where(ctd => ctd.DisplayName != null);

            foreach (var ctd in selected)
            {
                if (node.AddContentTypeAsParent)
                {
                    builder.Add(new LocalizedString(ctd.DisplayName, ctd.DisplayName), listTypeMenu => { AddContentItems(listTypeMenu, ctd.Name); });
                }
                else
                {
                    AddContentItems(builder, ctd.Name);
                }
            }

            // Add external children
            foreach (var childNode in node.Items)
            {
                try
                {
                    var treeBuilder = treeNodeBuilders.Where(x => x.Name == childNode.GetType().Name).FirstOrDefault();
                    await treeBuilder.BuildNavigationAsync(childNode, builder, treeNodeBuilders);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childNode.GetType().Name);
                }
            }

        }

        private async void AddContentItems(NavigationBuilder listTypeMenu, string contentTypeName)
        {
            var ListContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.Latest)
                .With<ContentItemIndex>(x => x.ContentType == contentTypeName)
                .ListAsync();

            foreach (var ci in ListContentItems)
            {
                var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);

                if (cim.AdminRouteValues.Any() && ci.DisplayText != null)
                {
                    listTypeMenu.Add(new LocalizedString(ci.DisplayText, ci.DisplayText), m => m
                        .Action(cim.AdminRouteValues["Action"] as string, cim.AdminRouteValues["Controller"] as string, cim.AdminRouteValues)
                        .Permission(Contents.Permissions.EditOwnContent)
                        .Resource(ci)
                        .LocalNav());
                }

            }
        }
    }
}
