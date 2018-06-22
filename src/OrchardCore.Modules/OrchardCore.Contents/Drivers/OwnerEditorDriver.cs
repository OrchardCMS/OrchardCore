using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security;
using OrchardCore.Users.Services;

namespace OrchardCore.Contents.Drivers
{
    public class OwnerEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IStringLocalizer T;

        public OwnerEditorDriver(
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IStringLocalizer<OwnerEditorDriver> stringLocalizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            T = stringLocalizer;
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
                return Initialize<OwnerEditorViewModel>("CommonPart_Edit__Owner", model =>
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

            if (!settings.DisplayOwnerEditor)
            {
                if (part.ContentItem.Owner == null)
                {
                    part.ContentItem.Owner = currentUser.Identity.Name;
                }
            }
            else
            {
                var model = new OwnerEditorViewModel();

                if (part.ContentItem.Owner != null)
                {
                    model.Owner = part.ContentItem.Owner;
                }

                var priorOwner = model.Owner;
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (!string.IsNullOrEmpty(part.ContentItem.Owner) && model.Owner != priorOwner)
                {
                    var newOwner = await _userService.GetUserAsync(model.Owner);

                    if (newOwner == null)
                    {
                        context.Updater.ModelState.AddModelError("CommonPart.Owner", T["Invalid user name"]);
                    }
                    else
                    {
                        part.ContentItem.Owner = newOwner.UserName;
                    }
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