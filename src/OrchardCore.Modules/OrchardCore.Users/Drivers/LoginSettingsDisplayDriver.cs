using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class LoginSettingsDisplayDriver : SiteDisplayDriver<LoginSettings>
{
    public const string GroupId = "userLogin";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public LoginSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<LoginSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => GroupId;

    public override IDisplayResult Edit(ISite site, LoginSettings settings, BuildEditorContext context)
    {
        return Combine(
            Initialize<LoginSettings>("LoginSettings_Edit", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "5").Tab("General").Card(S["Login"], "10"))
                .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext.User, UsersPermissions.ManageUsers), this)
                .OnGroup(SettingsGroupId),
            Initialize<LoginSettings>("LoginSettings_Edit__Account", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "5.1").Tab("General").Card(S["Account changes"], "20"))
                .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext.User, UsersPermissions.ManageUsers), this)
                .OnGroup(SettingsGroupId)
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, LoginSettings section, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, UsersPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(section, Prefix);

        return await EditAsync(site, section, context);
    }

    private static void PopulateViewModel(LoginSettings settings, LoginSettings model)
    {
        model.UseSiteTheme = settings.UseSiteTheme;
        model.DisableLocalLogin = settings.DisableLocalLogin;
        model.AllowChangingEmail = settings.AllowChangingEmail;
        model.AllowChangingUsername = settings.AllowChangingUsername;
        model.AllowChangingPhoneNumber = settings.AllowChangingPhoneNumber;
        model.AllowRememberMe = settings.AllowRememberMe;
        model.UsePersistentAuthenticationCookie = settings.UsePersistentAuthenticationCookie;
    }
}
