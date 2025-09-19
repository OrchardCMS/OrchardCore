using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Settings;

public sealed class ContentTypePartIndexSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IAuthorizationService _authorizationService;

    public ContentTypePartIndexSettingsDisplayDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return null;
        }

        return Initialize<LuceneContentIndexSettingsViewModel>("LuceneContentIndexSettings_Edit", model =>
        {
            model.LuceneContentIndexSettings = contentTypePartDefinition.GetSettings<LuceneContentIndexSettings>();
        }).Location("Content:10");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return null;
        }

        var model = new LuceneContentIndexSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model.LuceneContentIndexSettings);

        return await EditAsync(contentTypePartDefinition, context);
    }
}
