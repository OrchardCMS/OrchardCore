using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Templates.Settings;

public sealed class TemplateContentPartDefinitionDriver : ContentPartDefinitionDisplayDriver
{
    internal readonly IStringLocalizer S;

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplateContentPartDefinitionDriver(
        IStringLocalizer<TemplateContentPartDefinitionDriver> localizer,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        S = localizer;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> EditAsync(ContentPartDefinition contentPartDefinition, BuildEditorContext context)
    {
        if (!await IsAuthorizedAsync())
        {
            return null;
        }

        return Initialize<ContentSettingsViewModel>("TemplateSettings", model =>
        {
            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = contentPartDefinition.Name,
                    Description = S["Template for a {0} part in detail views", contentPartDefinition.DisplayName()],
                });

            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = $"{contentPartDefinition.Name}_Summary",
                    Description = S["Template for a {0} part in summary views", contentPartDefinition.DisplayName()],
                });
        }).Location("Shortcuts");
    }

    private async Task<bool> IsAuthorizedAsync() =>
        await _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageTemplates) &&
        await _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            LiquidPermissions.ManageLiquidTemplates);
}
