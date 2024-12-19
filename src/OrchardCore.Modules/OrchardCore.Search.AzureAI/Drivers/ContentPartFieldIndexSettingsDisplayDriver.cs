using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Drivers;

public sealed class ContentPartFieldIndexSettingsDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition contentPartFieldDefinition, BuildEditorContext updater)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
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
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        var settings = new AzureAISearchContentIndexSettings();

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        context.Builder.WithSettings(settings);

        return await EditAsync(contentPartFieldDefinition, context);
    }
}
