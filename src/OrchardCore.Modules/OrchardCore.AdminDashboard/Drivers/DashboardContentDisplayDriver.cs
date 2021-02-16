using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ContentManagement.Display.ViewModels;


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
            var isManageRequest = httpContext.Items.ContainsKey(typeof(DashboardManageAttribute));
            var hasPublished = await _contentManager.HasPublishedVersionAsync(model);
            var hasDraft = model.HasDraft();

            var hasEditPermission = await _authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.EditContent, model);
            var hasDeletePermission = await _authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.DeleteContent, model);
            var hasPublishPermission = await _authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.PublishContent, model);
    
            
            var results = new List<IDisplayResult>();

            var dragHandle = Dynamic("Dashboard_DragHandle", m =>
            {
                m.ContentItem = model;
            }).Location("Leading:before")
            .RenderWhen(() =>
            {
                return Task.FromResult(isManageRequest);
            });
            results.Add(dragHandle);

            var editButton = Dynamic("Dashboard_EditButton", m =>
            {
                m.ContentItem = model;
            }).Location("ActionsMenu:after")
            .RenderWhen(async () =>
            {
                return await Task.FromResult(isManageRequest && hasEditPermission);
            });
            results.Add(editButton);

            var deleteButton = Dynamic("Dashboard_DeleteButton", m =>
            {
                m.ContentItem = model;
            }).Location("ActionsMenu:after")
            .RenderWhen(async () =>
            {
                
                return await Task.FromResult(isManageRequest && hasDeletePermission);
            });
            results.Add(deleteButton);

            var unpublishButton = Dynamic("Dashboard_UnpublishButton", m =>
            {
                m.ContentItem = model;
            }).Location("ActionsMenu:after")
            .RenderWhen(async () =>
            {                
                return await Task.FromResult(isManageRequest && hasPublished && hasPublishPermission);
            });
            results.Add(unpublishButton);

            var publishButton = Dynamic("Dashboard_PublishButton", m =>
            {
                m.ContentItem = model;
            }).Location("ActionsMenu:after")
            .RenderWhen(async () =>
            {                
                return await Task.FromResult(isManageRequest && hasDraft && hasPublishPermission);
            });
            results.Add(publishButton);

            var discardDraftButton = Dynamic("Dashboard_DiscardDraftButton", m =>
            {
                m.ContentItem = model;
            }).Location("ActionsMenu:after")
            .RenderWhen(async () =>
            {                
                return await Task.FromResult(isManageRequest && hasDraft && hasEditPermission);
            });
            results.Add(discardDraftButton);


            var shapeTag = Shape("DashboardWidget__ContentsTags", new ContentItemViewModel(model))
            .Location("Detail", "Tags:10")
            .RenderWhen( () =>
            {
                return Task.FromResult(isManageRequest);
            });
            results.Add(shapeTag);

            return await Task.FromResult<IDisplayResult>(Combine(results.ToArray()));
        }
    }
}
