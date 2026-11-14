using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Drivers;

public sealed class ExternalRegistrationSettingsDisplayDriver : SiteDisplayDriver<ExternalRegistrationSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    public ExternalRegistrationSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IStringLocalizer<ExternalRegistrationSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => RegistrationSettingsDisplayDriver.GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, ExternalRegistrationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers))
        {
            return null;
        }

        return Combine(
            Initialize<ExternalRegistrationSettings>("ExternalRegistrationSettings_Edit", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "5").Tab("External Authentication", "5").Card(S["Registration"], "10"))
                .OnGroup(SettingsGroupId),
            Initialize<ExternalRegistrationSettings>("ExternalRegistrationSettings_Edit__Script", model => PopulateViewModel(settings, model))
                .Location(l => l.Zone("Content", "5.1").Tab("External Authentication", "5").Card(S["User name generation"], "20"))
                .OnGroup(SettingsGroupId)
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, ExternalRegistrationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, UsersPermissions.ManageUsers))
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(settings, Prefix);

        return await EditAsync(site, settings, context);
    }

    private static void PopulateViewModel(ExternalRegistrationSettings settings, ExternalRegistrationSettings model)
    {
        model.DisableNewRegistrations = settings.DisableNewRegistrations;
        model.NoPassword = settings.NoPassword;
        model.NoUsername = settings.NoUsername;
        model.NoEmail = settings.NoEmail;
        model.UseScriptToGenerateUsername = settings.UseScriptToGenerateUsername;
        model.GenerateUsernameScript = settings.GenerateUsernameScript;
    }
}
