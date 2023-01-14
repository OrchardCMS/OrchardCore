using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Security;
using OrchardCore.Users;

namespace OrchardCore.Contents.Drivers
{
    public class OwnerEditorDriver : ContentPartDisplayDriver<CommonPart>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IUser> _userManager;
        private readonly IStringLocalizer S;

        public OwnerEditorDriver(IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IUser> userManager,
            IStringLocalizer<OwnerEditorDriver> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            S = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(CommonPart part, BuildPartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part))
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

                        var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

                        var user = await _userManager.FindByIdAsync(part.ContentItem.Owner ?? currentUserId);

                        model.OwnerName = user?.UserName ?? currentUser.Identity.Name;
                    }
                });
            }

            return null;
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, UpdatePartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part))
            {
                return null;
            }

            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

            if (settings.DisplayOwnerEditor)
            {
                var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = await _userManager.FindByIdAsync(part.ContentItem.Owner ?? currentUserId);

                if (user == null)
                {
                    // At this point, we know that the owner is invalid or no longer exists.
                    user = await _userManager.FindByIdAsync(currentUserId);

                    part.ContentItem.Owner = currentUserId;
                }

                var model = new OwnerEditorViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix) && user.UserName != model.OwnerName)
                {
                    var newOwner = await _userManager.FindByNameAsync(model.OwnerName);

                    if (newOwner == null)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.OwnerName), S["Invalid username"]);
                    }
                    else
                    {
                        part.ContentItem.Owner = await _userManager.GetUserIdAsync(newOwner);
                    }
                }
            }

            return await EditAsync(part, context);
        }
    }
}
