using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment.Download;

public sealed class DownloadContentDriver : ContentDisplayDriver
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

    public override IDisplayResult Display(ContentItem contentItem, BuildDisplayContext context)
    {
        var user = _httpContextAccessor.HttpContext.User;

        return Shape("Download_SummaryAdmin__Button__Actions", new ContentItemViewModel(contentItem))
            .Location("SummaryAdmin", "ActionsMenu:20")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(user, OrchardCore.Deployment.CommonPermissions.Export, contentItem));
    }
}
