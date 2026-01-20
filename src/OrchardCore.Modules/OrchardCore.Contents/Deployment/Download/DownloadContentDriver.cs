using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.Download
{
    public class DownloadContentDriver : ContentDisplayDriver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public DownloadContentDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override IDisplayResult Display(ContentItem contentItem)
        {
            var context = _httpContextAccessor.HttpContext;

            return Shape("Download_SummaryAdmin__Button__Actions", new ContentItemViewModel(contentItem)).Location("SummaryAdmin", "ActionsMenu:20")
                    .RenderWhen(async () =>
                    {
                        var hasEditPermission = await _authorizationService.AuthorizeAsync(context.User, OrchardCore.Deployment.CommonPermissions.Export, contentItem);

                        if (hasEditPermission)
                        {
                            return true;
                        }

                        return false;
                    });
        }
    }
}
