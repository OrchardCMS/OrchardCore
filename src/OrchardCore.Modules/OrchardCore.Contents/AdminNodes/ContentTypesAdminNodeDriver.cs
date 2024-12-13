using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.AdminNodes;

public sealed class ContentTypesAdminNodeDriver : DisplayDriver<MenuItem, ContentTypesAdminNode>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ContentTypesAdminNodeDriver(
        IContentDefinitionManager contentDefinitionManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentTypesAdminNode treeNode, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ContentTypesAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
            View("ContentTypesAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(ContentTypesAdminNode treeNode, BuildEditorContext context)
    {
        return Initialize<ContentTypesAdminNodeViewModel>("ContentTypesAdminNode_Fields_TreeEdit", async model =>
            {
                var listable = await GetListableContentTypeDefinitionsAsync();

                model.ShowAll = treeNode.ShowAll;
                model.IconClass = treeNode.IconClass;
                model.ContentTypes = listable.Select(x => new ContentTypeEntryViewModel
                {
                    ContentTypeId = x.Name,
                    IsChecked = treeNode.ContentTypes.Any(selected => string.Equals(selected.ContentTypeId, x.Name, StringComparison.OrdinalIgnoreCase)),
                    IconClass = treeNode.ContentTypes.FirstOrDefault(selected => selected.ContentTypeId == x.Name)?.IconClass ?? string.Empty
                }).ToArray();
            }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypesAdminNode treeNode, UpdateEditorContext context)
    {
        // Initializes the value to empty otherwise the model is not updated if no type is selected.
        treeNode.ContentTypes = [];

        var model = new ContentTypesAdminNodeViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, x => x.ShowAll, x => x.IconClass, x => x.ContentTypes);

        treeNode.ShowAll = model.ShowAll;
        treeNode.IconClass = model.IconClass;
        treeNode.ContentTypes = model.ContentTypes
            .Where(x => x.IsChecked == true)
            .Select(x =>
            new ContentTypeEntry
            {
                ContentTypeId = x.ContentTypeId,
                IconClass = x.IconClass
            })
            .ToArray();

        return Edit(treeNode, context);
    }

    private async Task<IEnumerable<ContentTypeDefinition>> GetListableContentTypeDefinitionsAsync()
    {
        var contentTypeDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        var listableContentTypeDefinitions = new List<ContentTypeDefinition>();

        foreach (var contentTypeDefinition in contentTypeDefinitions)
        {
            if (!await _authorizationService.AuthorizeContentTypeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ListContent, contentTypeDefinition))
            {
                continue;
            }

            listableContentTypeDefinitions.Add(contentTypeDefinition);
        }

        return listableContentTypeDefinitions.OrderBy(t => t.DisplayName);
    }
}
