using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.AzureAI.Models;

namespace OrchardCore.AzureAI.Drivers;

public sealed class ContentTypePartIndexSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public ContentTypePartIndexSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<ContentTypePartIndexSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchSettings))
        {
            return null;
        }

        return Initialize<AzureAISearchContentIndexSettings>("AzureAISearchContentIndexSettings_Edit", model =>
        {
            model.Included = contentTypePartDefinition.GetSettings<AzureAISearchContentIndexSettings>().Included;
        }).Location(l => l.Zone("Content", "10").Card(S["Indexing"], "40"));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchSettings))
        {
            return null;
        }

        var settings = new AzureAISearchContentIndexSettings();

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        context.Builder.WithSettings(settings);

        return await EditAsync(contentTypePartDefinition, context);
    }
}
