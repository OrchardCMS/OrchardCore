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
        // TODO what permission are we looking for here?
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public AuditTrailContentsDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override IDisplayResult Display(ContentItem contentItem, IUpdateModel updater)
        {
           return Initialize<ContentItemViewModel>("AuditTrailContentsAction_SummaryAdmin", m => m.ContentItem = contentItem).Location("SummaryAdmin", "ActionsMenu:10");
        }
    }
}
