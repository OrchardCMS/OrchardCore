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
using OrchardCore.Users;

namespace OrchardCore.Contents.Drivers;

public sealed class OwnerEditorDriver : ContentPartDisplayDriver<CommonPart>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IUser> _userManager;

    internal readonly IStringLocalizer S;

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

        if (!await _authorizationService.AuthorizeAsync(currentUser, CommonPermissions.EditContentOwner, part.ContentItem).ConfigureAwait(false))
        {
            return null;
        }

        var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

        if (settings.DisplayOwnerEditor)
        {
            return Initialize<OwnerEditorViewModel>("CommonPart_Edit__Owner", async model =>
            {
                if (!string.IsNullOrEmpty(part.ContentItem.Owner))
                {
                    // TODO Move this editor to a user picker.
                    var user = await _userManager.FindByIdAsync(part.ContentItem.Owner).ConfigureAwait(false);

                    model.OwnerName = user?.UserName;
                }
            });
        }

        return null;
    }

    public override async Task<IDisplayResult> UpdateAsync(CommonPart part, UpdatePartEditorContext context)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(currentUser, CommonPermissions.EditContentOwner, part.ContentItem).ConfigureAwait(false))
        {
            return null;
        }

        var settings = context.TypePartDefinition.GetSettings<CommonPartSettings>();

        if (settings.DisplayOwnerEditor)
        {
            var model = new OwnerEditorViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(model.OwnerName))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.OwnerName), S["A value is required for Owner."]);
            }
            else
            {
                var newOwner = await _userManager.FindByNameAsync(model.OwnerName).ConfigureAwait(false);

                if (newOwner == null)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.OwnerName), S["Invalid username provided for Owner."]);
                }
                else
                {
                    part.ContentItem.Owner = await _userManager.GetUserIdAsync(newOwner).ConfigureAwait(false);
                }
            }
        }

        return await EditAsync(part, context).ConfigureAwait(false);
    }
}
