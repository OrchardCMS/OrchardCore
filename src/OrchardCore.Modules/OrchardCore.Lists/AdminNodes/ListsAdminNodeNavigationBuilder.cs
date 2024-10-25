using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Navigation;
using YesSql;

namespace OrchardCore.Lists.AdminNodes;

public class ListsAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly ILogger _logger;
    private ListsAdminNode _node;
    private ContentTypeDefinition _contentType;

    // Security check.
    private const int MaxItemsInNode = 100;

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

    public string Name => nameof(ListsAdminNode);

    public async Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
    {
        _node = menuItem as ListsAdminNode;

        if (_node == null || !_node.Enabled || string.IsNullOrEmpty(_node.ContentType))
        {
            return;
        }

        _contentType = await _contentDefinitionManager.GetTypeDefinitionAsync(_node.ContentType);

        if (_node.AddContentTypeAsParent)
        {
            if (_contentType == null)
            {
                _logger.LogError("Can't find The content type '{ContentType}' for list admin node.", _node.ContentType);
            }

            await builder.AddAsync(new LocalizedString(_contentType.DisplayName, _contentType.DisplayName), async listTypeMenu =>
            {
                AddPrefixToClasses(_node.IconForParentLink).ForEach(c => listTypeMenu.AddClass(c));
                listTypeMenu.Permission(ContentTypePermissionsHelper.CreateDynamicPermission(
                    ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name], _contentType));
                await AddContentItemsAsync(listTypeMenu);
            });
        }
        else
        {
            await AddContentItemsAsync(builder);
        }

        // Add external children.
        foreach (var childNode in _node.Items)
        {
            try
            {
                var treeBuilder = treeNodeBuilders.FirstOrDefault(x => x.Name == childNode.GetType().Name);
                await treeBuilder.BuildNavigationAsync(childNode, builder, treeNodeBuilders);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childNode.GetType().Name);
            }
        }
    }

    private async Task AddContentItemsAsync(NavigationBuilder listTypeMenu)
    {
        foreach (var ci in await GetContentItemsAsync())
        {
            var cim = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(ci);

            if (cim.AdminRouteValues.Count > 0 && ci.DisplayText != null)
            {
                listTypeMenu.Add(new LocalizedString(ci.DisplayText, ci.DisplayText), m =>
                {
                    m.Action(cim.AdminRouteValues["Action"] as string, cim.AdminRouteValues["Controller"] as string, cim.AdminRouteValues);
                    m.Resource(ci);
                    m.Priority(_node.Priority);
                    m.Position(_node.Position);
                    m.LocalNav();
                    AddPrefixToClasses(_node.IconForContentItems).ToList().ForEach(c => m.AddClass(c));

                    m.Permission(ContentTypePermissionsHelper.CreateDynamicPermission(
                    ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name], _contentType));
                });
            }
        }
    }

    private async Task<List<ContentItem>> GetContentItemsAsync()
    {
        return (await _session.Query<ContentItem, ContentItemIndex>()
            .With<ContentItemIndex>(x => x.Latest && x.ContentType == _node.ContentType)
            .Take(MaxItemsInNode)
            .ListAsync())
            .OrderBy(x => x.DisplayText)
            .ToList();
    }

    private static List<string> AddPrefixToClasses(string unprefixed)
    {
        return unprefixed?.Split(' ')
            .ToList()
            .Select(c => "icon-class-" + c)
            .ToList()
            ?? [];
    }
}
