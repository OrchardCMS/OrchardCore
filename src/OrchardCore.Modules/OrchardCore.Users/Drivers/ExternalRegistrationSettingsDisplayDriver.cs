using Microsoft.AspNetCore.Authorization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalRegistrationSettingsDisplayDriver : SiteDisplayDriver<ExternalRegistrationSettings>
{
    private readonly IAuthorizationService _authorizationService;

    public ExternalRegistrationSettingsDisplayDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    protected override string SettingsGroupId
        => RegistrationSettingsDisplayDriver.GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ExternalRegistrationSettings settings, BuildEditorContext context)
    {
        var user = context.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers))
        {
            return null;
        }

        return Initialize<ExternalRegistrationSettings>("ExternalRegistrationSettings_Edit", model =>
        {
            model.DisableNewRegistrations = settings.DisableNewRegistrations;
            model.NoPassword = settings.NoPassword;
            model.NoUsername = settings.NoUsername;
            model.NoEmail = settings.NoEmail;
            model.UseScriptToGenerateUsername = settings.UseScriptToGenerateUsername;
            model.GenerateUsernameScript = settings.GenerateUsernameScript;
        }).Location("Content:5#External Authentication;5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalRegistrationSettings settings, UpdateEditorContext context)
    {
        var user = context.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }
}
