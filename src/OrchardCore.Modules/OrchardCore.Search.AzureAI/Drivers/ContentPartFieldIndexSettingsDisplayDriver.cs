using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Drivers;

public class ContentPartFieldIndexSettingsDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
    : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        return Initialize<AzureAISearchContentIndexSettings>("AzureAISearchContentIndexSettings_Edit", model => contentPartFieldDefinition.GetSettings<AzureAISearchContentIndexSettings>())
            .Location("Content:10");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition contentPartFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return null;
        }

        var model = new AzureAISearchContentIndexSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return await EditAsync(contentPartFieldDefinition, context.Updater);
    }
}
