using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;
using LiquidPermissions = OrchardCore.Liquid.Permissions;

namespace OrchardCore.Templates.Settings;

public sealed class TemplateContentTypePartDefinitionDriver : ContentTypePartDefinitionDisplayDriver
{
    internal readonly IStringLocalizer S;

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplateContentTypePartDefinitionDriver(
        IStringLocalizer<TemplateContentTypePartDefinitionDriver> localizer,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        S = localizer;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (!await IsAuthorizedAsync())
        {
            return null;
        }

        return Initialize<ContentSettingsViewModel>("TemplateSettings", model =>
        {
            var contentType = contentTypePartDefinition.ContentTypeDefinition.Name;
            var partName = contentTypePartDefinition.Name;

            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = $"{contentType}__{partName}",
                    Description = S["Template for the {0} part in a {1} type in detail views", partName, contentTypePartDefinition.ContentTypeDefinition.DisplayName],
                });

            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = $"{contentType}_Summary__{partName}",
                    Description = S["Template for the {0} part in a {1} type in summary views", partName, contentTypePartDefinition.ContentTypeDefinition.DisplayName],
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
