using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Settings
{
    public class ContentPartFieldIndexSettingsDisplayDriver : ContentPartFieldDefinitionDisplayDriver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ContentPartFieldIndexSettingsDisplayDriver(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition contentPartFieldDefinition, IUpdateModel updater)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageLuceneIndexes))
            {
                return null;
            }

            return Initialize<LuceneContentIndexSettingsViewModel>("LuceneContentIndexSettings_Edit", model =>
            {
                model.LuceneContentIndexSettings = contentPartFieldDefinition.GetSettings<LuceneContentIndexSettings>();
            }).Location("Content:10");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition contentPartFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageLuceneIndexes))
            {
                return null;
            }

            var model = new LuceneContentIndexSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model.LuceneContentIndexSettings);

            return await EditAsync(contentPartFieldDefinition, context.Updater);
        }
    }
}
