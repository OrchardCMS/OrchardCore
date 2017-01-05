using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Contents.Models;
using Orchard.Contents.ViewModels;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Security;

namespace Orchard.Contents.Drivers
{
    public class OwnerEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OwnerEditorDriver(
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> EditAsync(CommonPart part, BuildPartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser == null || !(await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part)))
            {
                return null;
            }

            var settings = GetSettings(part);

            if (settings.DisplayOwnerEditor)
            {
                return Shape<OwnerEditorViewModel>("CommonPart_Edit__Owner", model =>
                {
                    model.Owner = part.ContentItem.Owner;
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, BuildPartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser == null || !(await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part)))
            {
                return null;
            }

            var settings = GetSettings(part);

            if (settings.DisplayOwnerEditor)
            {
                var model = new OwnerEditorViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (!string.IsNullOrEmpty(part.ContentItem.Owner) || part.ContentItem.Number > 0)
                {
                    part.ContentItem.Owner = model.Owner;
                }                
            }

            return await EditAsync(part, context);
        }

        public CommonPartSettings GetSettings(CommonPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "CommonPart", StringComparison.Ordinal));
            return contentTypePartDefinition.GetSettings<CommonPartSettings>();
        }
    }
}