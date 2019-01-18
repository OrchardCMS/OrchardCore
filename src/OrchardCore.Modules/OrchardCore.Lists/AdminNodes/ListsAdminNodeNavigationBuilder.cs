using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.AdminMenu.Services;
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
        private ListsAdminNode _node;
        private const int MaxItemsInNode = 100; // security check

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
            _node = menuItem as ListsAdminNode;

            if ((_node == null) || (!_node.Enabled))
            {
                return;
            }

            if (_node.AddContentTypeAsParent)
            {
                var contentType = _contentDefinitionManager.GetTypeDefinition(_node.ContentType);
                if (contentType == null)
                {
                    _logger.LogError("Can't find The content type {0} for list admin node.", _node.ContentType);
                }

                builder.Add(new LocalizedString(contentType.DisplayName, contentType.DisplayName), listTypeMenu =>
                {
                    AddPrefixToClasses(_node.IconForParentLink).ForEach(c => listTypeMenu.AddClass(c));

                    AddContentItems(listTypeMenu);
                });
            }
            else
            {
                AddContentItems(builder);
            }


            // Add external children
            foreach (var childNode in _node.Items)
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

        private async void AddContentItems(NavigationBuilder listTypeMenu)
        {
            foreach (var ci in await getContentItems())
            {
                var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);

                if (cim.AdminRouteValues.Any() && ci.DisplayText != null)
                {
                    listTypeMenu.Add(new LocalizedString(ci.DisplayText, ci.DisplayText), m =>
                    {
                        m.Action(cim.AdminRouteValues["Action"] as string, cim.AdminRouteValues["Controller"] as string, cim.AdminRouteValues);
                        m.Permission(Contents.Permissions.EditOwnContent);
                        m.Resource(ci);
                        m.Priority(_node.Priority);
                        m.Position(_node.Position);
                        m.LocalNav();
                        AddPrefixToClasses(_node.IconForContentItems).ToList().ForEach(c => m.AddClass(c));                     
                    });
                }
            }
        }


        private async Task<List<ContentItem>> getContentItems()
        {
            return (await _session.Query<ContentItem, ContentItemIndex>()
                .With<ContentItemIndex>(x => x.Latest)
                .With<ContentItemIndex>(x => x.ContentType == _node.ContentType)
                .Take(MaxItemsInNode)
                .ListAsync())
                .OrderBy(x => x.DisplayText)
                .ToList();
        }


        private List<string> AddPrefixToClasses(string unprefixed)
        {
            return unprefixed?.Split(' ')
                .ToList()
                .Select(c => "icon-class-" + c)
                .ToList<string>()
                ?? new List<string>();
        }
    }
}