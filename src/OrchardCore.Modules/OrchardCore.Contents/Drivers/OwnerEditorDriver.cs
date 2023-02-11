using System;
using System.Linq;
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

        public override IDisplayResult Edit(CommonPart part, BuildPartEditorContext context)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner)
                && !await _authorizationService.AuthorizeAsync(currentUser, CommonPermissions.EditContentOwner, part.ContentItem))
            {
                return null;
            }

            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

            return Initialize<OwnerEditorViewModel>("CommonPart_Edit__Owner", async model =>
            {
                if (!String.IsNullOrEmpty(part.ContentItem.Owner))
                {
                    // TODO Move this editor to a user picker.
                    var user = await _userManager.FindByIdAsync(part.ContentItem.Owner);

                    model.OwnerName = user?.UserName;
                }
            }).RenderWhen(() => HasAccessToEditorAsync(part, settings));
        }

        public override async Task<IDisplayResult> UpdateAsync(CommonPart part, UpdatePartEditorContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

            if (!await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner)
                && !await _authorizationService.AuthorizeAsync(currentUser, CommonPermissions.EditContentOwner, part.ContentItem))
            {
                return null;
            }

            var model = new OwnerEditorViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (String.IsNullOrWhiteSpace(model.OwnerName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.OwnerName), S["A value is required for Owner."]);
            }
            else
            {
                var newOwner = await _userManager.FindByNameAsync(model.OwnerName);

                if (newOwner == null)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.OwnerName), S["Invalid username provided for Owner."]);
                }
                else
                {
                    part.ContentItem.Owner = await _userManager.GetUserIdAsync(newOwner);
                }
            }

            return Edit(part, context);
        }

        private async Task<bool> HasAccessToEditorAsync(CommonPart part, CommonPartSettings settings)
        {
            if (!settings.DisplayOwnerEditor)
            {
                return false;
            }

            var currentUser = _httpContextAccessor.HttpContext?.User;

            if (await _authorizationService.AuthorizeAsync(currentUser, StandardPermissions.SiteOwner, part))
            {
                return true;
            }

            var user = await _userManager.FindByNameAsync(currentUser.Identity.Name);

            var userRoles = await _userManager.GetRolesAsync(user);

            return userRoles.Any(userRole => settings.Roles.Contains(userRole));
        }
    }
}
