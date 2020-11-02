using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Security;
using OrchardCore.Users.Services;

namespace OrchardCore.Contents.Drivers
{
    public class OwnerEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IStringLocalizer S;

        public OwnerEditorDriver(IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IStringLocalizer<OwnerEditorDriver> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            S = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(CommonPart part, BuildPartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser == null || !(await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part)))
            {
                return null;
            }

            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

            if (settings.DisplayOwnerEditor)
            {
                return Initialize<OwnerEditorViewModel>("CommonPart_Edit__Owner", async model =>
                {
                    if (part.ContentItem.Owner != null)
                    {
                        // TODO Move this editor to a user picker.
                        var user = await _userService.GetUserByUniqueIdAsync(part.ContentItem.Owner);
                        model.OwnerName = user.UserName;
                    }
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, UpdatePartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (currentUser == null || !(await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part)))
            {
                return null;
            }

            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

            if (!settings.DisplayOwnerEditor)
            {
                if (part.ContentItem.Owner == null)
                {
                    part.ContentItem.Owner = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                }
            }
            else
            {
                var model = new OwnerEditorViewModel();

                if (part.ContentItem.Owner != null)
                {
                    var user = await _userService.GetUserByUniqueIdAsync(part.ContentItem.Owner);
                    model.OwnerName = user.UserName;
                }

                var priorOwnerName = model.OwnerName;
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (model.OwnerName != priorOwnerName)
                {
                    var newOwner = await _userService.GetUserAsync(model.OwnerName);

                    if (newOwner == null)
                    {
                        context.Updater.ModelState.AddModelError("CommonPart.OwnerName", S["Invalid user name"]);
                    }
                    else
                    {
                        part.ContentItem.Owner = newOwner.UserId;
                    }
                }
            }

            return await EditAsync(part, context);
        }
    }
}
