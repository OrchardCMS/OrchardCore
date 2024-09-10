using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers;

public sealed class ContentsDriver : ContentDisplayDriver
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ContentsDriver(
        IContentDefinitionManager contentDefinitionManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
    {
        // We add custom alternates. This could be done generically to all shapes coming from ContentDisplayDriver but right now it's
        // only necessary on this shape. Otherwise c.f. ContentPartDisplayDriver.

        var results = new List<IDisplayResult>()
        {
            Shape("ContentsTags_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "Tags:10"),
            Shape("ContentsMeta_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "Meta:20"),
        };

        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

        if (contentTypeDefinition != null)
        {
            var contentsMetadataShape = Shape("ContentsMetadata", new ContentItemViewModel(contentItem))
                .Location("Detail", "Content:before");

            contentsMetadataShape.Displaying(ctx =>
            {
                var hasStereotype = contentTypeDefinition.TryGetStereotype(out var stereotype);

                if (hasStereotype && !string.Equals("Content", stereotype, StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Shape.Metadata.Alternates.Add($"{stereotype}__ContentsMetadata");
                }

                var displayType = ctx.Shape.Metadata.DisplayType;

                if (!string.IsNullOrEmpty(displayType) && displayType != "Detail")
                {
                    ctx.Shape.Metadata.Alternates.Add($"ContentsMetadata_{ctx.Shape.Metadata.DisplayType}");

                    if (hasStereotype && !string.Equals("Content", stereotype, StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Shape.Metadata.Alternates.Add($"{stereotype}_{displayType}__ContentsMetadata");
                    }
                }
            });

            var user = _httpContextAccessor.HttpContext.User;

            results.Add(contentsMetadataShape);
            results.Add(Shape("ContentsButtonEdit_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "Actions:10"));
            results.Add(Shape("ContentsButtonActions_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "ActionsMenu:10")
                .RenderWhen(async () =>
                {
                    var hasPublishPermission = await _authorizationService.AuthorizeAsync(user, CommonPermissions.PublishContent, contentItem);
                    var hasDeletePermission = await _authorizationService.AuthorizeAsync(user, CommonPermissions.DeleteContent, contentItem);
                    var hasPreviewPermission = await _authorizationService.AuthorizeAsync(user, CommonPermissions.PreviewContent, contentItem);
                    var hasClonePermission = await _authorizationService.AuthorizeAsync(user, CommonPermissions.CloneContent, contentItem);

                    return hasPublishPermission || hasDeletePermission || hasPreviewPermission || hasClonePermission;
                })
            );
        }

        return Combine(results);
    }

    public override async Task<IDisplayResult> EditAsync(ContentItem contentItem, BuildEditorContext context)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

        if (contentTypeDefinition == null)
        {
            return null;
        }

        var user = _httpContextAccessor.HttpContext.User;

        return Combine(
            Dynamic("Content_PublishButton").Location("Actions:10")
                .RenderWhen(() => _authorizationService.AuthorizeAsync(user, CommonPermissions.PublishContent, contentItem)),
            Dynamic("Content_SaveDraftButton").Location("Actions:20")
                .RenderWhen(async () =>
                {
                    return contentTypeDefinition.IsDraftable()
                    && await _authorizationService.AuthorizeAsync(user, CommonPermissions.EditContent, contentItem);
                })
            );
    }
}
