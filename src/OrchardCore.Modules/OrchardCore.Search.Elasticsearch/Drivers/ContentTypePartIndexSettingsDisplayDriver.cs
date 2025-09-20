using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers;

public sealed class ContentTypePartIndexSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IAuthorizationService _authorizationService;

    public ContentTypePartIndexSettingsDisplayDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return null;
        }

        return Initialize<ElasticContentIndexSettingsViewModel>("ElasticContentIndexSettings_Edit", model =>
        {
            model.ElasticContentIndexSettings = contentTypePartDefinition.GetSettings<ElasticContentIndexSettings>();
        }).Location("Content:10");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return null;
        }

        var model = new ElasticContentIndexSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model.ElasticContentIndexSettings);

        return await EditAsync(contentTypePartDefinition, context);
    }
}
