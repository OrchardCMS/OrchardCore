using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.ViewModels;

namespace OrchardCore.Lucene.Settings;

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
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, LuceneSearchPermissions.ManageLuceneIndexes))
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
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return null;
        }

        var model = new LuceneContentIndexSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model.LuceneContentIndexSettings);

        return await EditAsync(contentTypePartDefinition, context);
    }
}
