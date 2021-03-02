using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;


namespace OrchardCore.AdminDashboard.Drivers
{
    public class DashboardContentDisplayDriver : ContentDisplayDriver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;


        public DashboardContentDisplayDriver(IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IContentManager contentManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _contentManager = contentManager;

        }

        public override async Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var dashboardFeature = httpContext.Features.Get<DashboardFeature>();

            // Return if it's not Manage dashboard request 
            if (dashboardFeature == null || !dashboardFeature.IsManageRequest)
            {
                return null;
            }

            var results = new List<IDisplayResult>();
            var hasPublished = await _contentManager.HasPublishedVersionAsync(model);
            var hasDraft = model.HasDraft();
            var hasEditPermission = await _authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.EditContent, model);
            var hasDeletePermission = await _authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.DeleteContent, model);
            var hasPublishPermission = await _authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.PublishContent, model);

            var dragHandle = Initialize<ContentItemViewModel>("Dashboard_DragHandle", m =>
            {
                m.ContentItem = model;
            }).Location("Leading:before");
            results.Add(dragHandle);

            if (hasEditPermission)
            {
                var editButton = Initialize<ContentItemViewModel>("Dashboard_EditButton", m =>
                {
                    m.ContentItem = model;
                }).Location("ActionsMenu:after");
                results.Add(editButton);
            }

            if (hasDeletePermission)
            {
                var deleteButton = Initialize<ContentItemViewModel>("Dashboard_DeleteButton", m =>
                {
                    m.ContentItem = model;
                }).Location("ActionsMenu:after");
                results.Add(deleteButton);
            }

            if (hasPublished && hasPublishPermission)
            {
                var unpublishButton = Initialize<ContentItemViewModel>("Dashboard_UnpublishButton", m =>
                {
                    m.ContentItem = model;
                }).Location("ActionsMenu:after");
                results.Add(unpublishButton);
            }

            if (hasDraft && hasPublishPermission)
            {
                var publishButton = Initialize<ContentItemViewModel>("Dashboard_PublishButton", m =>
                {
                    m.ContentItem = model;
                }).Location("ActionsMenu:after");
                results.Add(publishButton);
            }

            if (hasDraft && hasEditPermission)
            {
                var discardDraftButton = Initialize<ContentItemViewModel>("Dashboard_DiscardDraftButton", m =>
                {
                    m.ContentItem = model;
                }).Location("ActionsMenu:after");
                results.Add(discardDraftButton);
            }

            var shapeTag = Initialize<ContentItemViewModel>("DashboardWidget_DetailAdmin__ContentsTags", m =>
            {
                m.ContentItem = model;
            }).Location("DetailAdmin", "Tags:10");
            results.Add(shapeTag);

            return Combine(results.ToArray());
        }
    }
}
