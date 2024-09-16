using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Models;

namespace OrchardCore.Demo.Drivers;

public sealed class UserProfileDisplayDriver : SectionDisplayDriver<User, UserProfile>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    public UserProfileDisplayDriver(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    public override IDisplayResult Edit(User user, UserProfile profile, BuildEditorContext context)
    {
        return Initialize<EditUserProfileViewModel>("UserProfile_Edit", model =>
        {
            model.Age = profile.Age;
            model.FirstName = profile.FirstName;
            model.LastName = profile.LastName;
        })
        .Location("Content:2")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageOwnUserProfile));
    }

    public override async Task<IDisplayResult> UpdateAsync(User user, UserProfile profile, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, Permissions.ManageOwnUserProfile))
        {
            return Edit(user, profile, context);
        }

        var model = new EditUserProfileViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        profile.Age = model.Age;
        profile.FirstName = model.FirstName;
        profile.LastName = model.LastName;
        profile.UpdatedAt = DateTime.UtcNow;

        return Edit(user, profile, context);
    }
}
