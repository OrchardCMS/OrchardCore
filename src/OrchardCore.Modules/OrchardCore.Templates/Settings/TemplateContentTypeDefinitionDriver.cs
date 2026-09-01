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

public sealed class TemplateContentTypeDefinitionDriver : ContentTypeDefinitionDisplayDriver
{
    internal readonly IStringLocalizer S;

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplateContentTypeDefinitionDriver(
        IStringLocalizer<TemplateContentTypeDefinitionDriver> localizer,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        S = localizer;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        if (!await IsAuthorizedAsync())
        {
            return null;
        }

        return Initialize<ContentSettingsViewModel>("TemplateSettings", model =>
        {
            if (!contentTypeDefinition.TryGetStereotype(out var stereotype))
            {
                stereotype = "Content";
            }

            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = $"{stereotype}__{contentTypeDefinition.Name}",
                    Description = S["Template for a {0} content item in detail views", contentTypeDefinition.DisplayName],
                });

            model.ContentSettingsEntries.Add(
                new ContentSettingsEntry
                {
                    Key = $"{stereotype}_Summary__{contentTypeDefinition.Name}",
                    Description = S["Template for a {0} content item in summary views", contentTypeDefinition.DisplayName],
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
