using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class ContentsDriver : ContentDisplayDriver
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

        public override IDisplayResult Display(ContentItem contentItem, IUpdateModel updater)
        {
            // We add custom alternates. This could be done generically to all shapes coming from ContentDisplayDriver but right now it's
            // only necessary on this shape. Otherwise c.f. ContentPartDisplayDriver

            var context = _httpContextAccessor.HttpContext;
            var results = new List<IDisplayResult>();
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            var contentsMetadataShape = Shape("ContentsMetadata",
                new ContentItemViewModel(contentItem))
                .Location("Detail", "Content:before");

            if (contentTypeDefinition != null)
            {
                contentsMetadataShape.Displaying(ctx =>
                {
                    var hasStereotype = contentTypeDefinition.TryGetStereotype(out var stereotype);

                    if (hasStereotype && !String.Equals("Content", stereotype, StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Shape.Metadata.Alternates.Add($"{stereotype}__ContentsMetadata");
                    }

                    var displayType = ctx.Shape.Metadata.DisplayType;

                    if (!String.IsNullOrEmpty(displayType) && displayType != "Detail")
                    {
                        ctx.Shape.Metadata.Alternates.Add($"ContentsMetadata_{ctx.Shape.Metadata.DisplayType}");

                        if (hasStereotype && !String.Equals("Content", stereotype, StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Shape.Metadata.Alternates.Add($"{stereotype}_{displayType}__ContentsMetadata");
                        }
                    }
                });

                results.Add(contentsMetadataShape);
                results.Add(Shape("ContentsButtonEdit_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "Actions:10"));
                results.Add(Shape("ContentsButtonActions_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "ActionsMenu:10")
                    .RenderWhen(async () =>
                    {
                        var hasPublishPermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.PublishContent, contentItem);
                        var hasDeletePermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.DeleteContent, contentItem);
                        var hasPreviewPermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.PreviewContent, contentItem);
                        var hasClonePermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.CloneContent, contentItem);

                        if (hasPublishPermission || hasDeletePermission || hasPreviewPermission || hasClonePermission)
                        {
                            return true;
                        }

                        return false;
                    })
                );
            }

            results.Add(Shape("ContentsTags_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "Tags:10"));
            results.Add(Shape("ContentsMeta_SummaryAdmin", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "Meta:20"));

            return Combine(results.ToArray());
        }

        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            var context = _httpContextAccessor.HttpContext;
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            var results = new List<IDisplayResult>();

            if (contentTypeDefinition == null)
            {
                return null;
            }

            results.Add(Dynamic("Content_PublishButton").Location("Actions:10")
                .RenderWhen(() => _authorizationService.AuthorizeAsync(context.User, CommonPermissions.PublishContent, contentItem)));

            results.Add(Dynamic("Content_SaveDraftButton").Location("Actions:20")
                .RenderWhen(async () =>
                {
                    if (contentTypeDefinition.IsDraftable() &&
                        await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.EditContent, contentItem))
                    {
                        return true;
                    }

                    return false;
                })
            );

            return Combine(results.ToArray());
        }
    }
}
