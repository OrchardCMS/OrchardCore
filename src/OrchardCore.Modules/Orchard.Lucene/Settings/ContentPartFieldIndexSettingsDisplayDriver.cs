using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentTypes.Editors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Indexing;

namespace Orchard.Lucene.Settings
{
    public class ContentPartFieldIndexSettingsDisplayDriver : ContentPartFieldDisplayDriver
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
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageIndexes))
            {
                return null;
            }

            return Shape<ContentIndexSettingsViewModel>("ContentIndexSettings_Edit", model =>
            {
                model.ContentIndexSettings = contentPartFieldDefinition.GetSettings<ContentIndexSettings>();

                return Task.CompletedTask;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition contentPartFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageIndexes))
            {
                return null;
            }

            var model = new ContentIndexSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model.ContentIndexSettings);

            return Edit(contentPartFieldDefinition, context.Updater);
        }
    }
}