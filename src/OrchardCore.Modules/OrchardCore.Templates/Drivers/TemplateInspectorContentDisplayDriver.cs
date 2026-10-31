using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Templates.Drivers;

/// <summary>
/// Adds an "Inspect templates" action to the content items of the admin lists, to see the templates they render with.
/// </summary>
public sealed class TemplateInspectorContentDisplayDriver : ContentDisplayDriver
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplateInspectorContentDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Display(ContentItem contentItem, BuildDisplayContext context)
    {
        // Only the admin lists show actions.
        if (context.DisplayType is not (OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid))
        {
            return null;
        }

        return View("TemplateInspectorAction_SummaryAdmin", contentItem)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:20")
            .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:20")
            .RenderWhen(async () =>
            {
                var user = _httpContextAccessor.HttpContext.User;

                return await _authorizationService.AuthorizeAsync(user, OrchardCore.Liquid.Permissions.ManageLiquidTemplates)
                    && await _authorizationService.AuthorizeAsync(user, Permissions.ManageTemplates);
            });
    }
}
