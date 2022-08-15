using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers
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
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageElasticIndexes))
            {
                return null;
            }

            return Initialize<ElasticContentIndexSettingsViewModel>("ElasticContentIndexSettings_Edit", model =>
            {
                model.ElasticContentIndexSettings = contentPartFieldDefinition.GetSettings<ElasticContentIndexSettings>();
            }).Location("Content:10");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition contentPartFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageElasticIndexes))
            {
                return null;
            }

            var model = new ElasticContentIndexSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model.ElasticContentIndexSettings);

            return await EditAsync(contentPartFieldDefinition, context.Updater);
        }
    }
}
