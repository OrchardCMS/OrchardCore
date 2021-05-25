using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.AuditTrail.Drivers
{
    public class AuditTrailContentsDriver : ContentDisplayDriver
    {
        // private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AuditTrailContentsDriver(
            // IContentDefinitionManager contentDefinitionManager,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            // _contentDefinitionManager = contentDefinitionManager;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override IDisplayResult Display(ContentItem contentItem, IUpdateModel updater)
        {
            // We add custom alternates. This could be done generically to all shapes coming from ContentDisplayDriver but right now it's
            // only necessary on this shape. Otherwise c.f. ContentPartDisplayDriver

            // var context = _httpContextAccessor.HttpContext;
           return Initialize<ContentItemViewModel>("AuditTrailContentsAction_SummaryAdmin", m => m.ContentItem = contentItem).Location("SummaryAdmin", "ActionsMenu:10");
                    // .RenderWhen(async () =>
                    // {
                    //     // var hasPublishPermission = await _authorizationService.AuthorizeAsync(context.User, OrchardCore.Contents.CommonPermissions.PublishContent, contentItem);
                    //     // var hasDeletePermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.DeleteContent, contentItem);
                    //     // var hasPreviewPermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.PreviewContent, contentItem);
                    //     // var hasClonePermission = await _authorizationService.AuthorizeAsync(context.User, CommonPermissions.CloneContent, contentItem);

                    //     if (hasPublishPermission || hasDeletePermission || hasPreviewPermission || hasClonePermission)
                    //     {
                    //         return true;
                    //     }

                    //     return false;
                    // })
                // );
            // )

        }
    }
}
