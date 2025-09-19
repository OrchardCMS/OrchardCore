using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class ContentTypePartIndexSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IAuthorizationService _authorizationService;

    public ContentTypePartIndexSettingsDisplayDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchISettings))
        {
            return null;
        }

        return Initialize<AzureAISearchContentIndexSettings>("AzureAISearchContentIndexSettings_Edit", model =>
        {
            model.Included = contentTypePartDefinition.GetSettings<AzureAISearchContentIndexSettings>().Included;
        }).Location("Content:10");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchISettings))
        {
            return null;
        }

        var settings = new AzureAISearchContentIndexSettings();

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        context.Builder.WithSettings(settings);

        return await EditAsync(contentTypePartDefinition, context);
    }
}
