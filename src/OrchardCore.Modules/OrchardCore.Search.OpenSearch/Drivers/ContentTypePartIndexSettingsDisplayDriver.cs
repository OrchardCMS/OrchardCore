using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.OpenSearch.Core.Models;
using OrchardCore.Search.OpenSearch.ViewModels;

namespace OrchardCore.Search.OpenSearch.Drivers;

public sealed class ContentTypePartIndexSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public ContentTypePartIndexSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, OpenSearchPermissions.ManageOpenSearchIndexes))
        {
            return null;
        }

        return Initialize<OpenSearchContentIndexSettingsViewModel>("OpenSearchContentIndexSettings_Edit", model =>
        {
            model.OpenSearchContentIndexSettings = contentTypePartDefinition.GetSettings<OpenSearchContentIndexSettings>();
        }).Location("Content:10");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, OpenSearchPermissions.ManageOpenSearchIndexes))
        {
            return null;
        }

        var model = new OpenSearchContentIndexSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model.OpenSearchContentIndexSettings);

        return await EditAsync(contentTypePartDefinition, context);
    }
}
