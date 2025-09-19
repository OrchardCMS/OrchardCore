using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class ContentPartFieldIndexSettingsDisplayDriver(IAuthorizationService authorizationService)
    : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition contentPartFieldDefinition, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchISettings))
        {
            return null;
        }

        return Initialize<AzureAISearchContentIndexSettings>("AzureAISearchContentIndexSettings_Edit", model =>
        {
            model.Included = contentPartFieldDefinition.GetSettings<AzureAISearchContentIndexSettings>().Included;
        }).Location("Content:10");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition contentPartFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, AzureAISearchPermissions.ManageAzureAISearchISettings))
        {
            return null;
        }

        var settings = new AzureAISearchContentIndexSettings();

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        context.Builder.WithSettings(settings);

        return await EditAsync(contentPartFieldDefinition, context);
    }
}
