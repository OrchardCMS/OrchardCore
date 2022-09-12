using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public class UserProfileSettingsDisplayDriver : SectionDisplayDriver<ISite, UserProfileSettings>
{
    public const string GroupId = "user-profile";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    public UserProfileSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellHost shellHost,
         ShellSettings shellSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
    }

    public override IDisplayResult Edit(UserProfileSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        return Initialize<UserProfileSettingsViewModel>("UserProfileSettings_Edit", model =>
        {
            model.AllowChangingEmail = settings.AllowChangingEmail;
            model.AllowChangingUsername = settings.AllowChangingUsername;

        }).Location("Content:5").OnGroup(GroupId)
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUserProfileSettings));
    }


    public override async Task<IDisplayResult> UpdateAsync(UserProfileSettings settings, IUpdateModel updater, BuildEditorContext context)
    {
        if (!GroupId.Equals(context.GroupId, StringComparison.OrdinalIgnoreCase) || !await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, CommonPermissions.ManageUserProfileSettings))
        {
            return null;
        }

        var model = new UserProfileSettingsViewModel();

        await updater.TryUpdateModelAsync(model, Prefix);

        if (settings.AllowChangingEmail != model.AllowChangingEmail || settings.AllowChangingUsername != model.AllowChangingUsername)
        {
            settings.AllowChangingEmail = model.AllowChangingEmail;
            settings.AllowChangingUsername = model.AllowChangingUsername;

            // Release the tenant to apply the settings
            await _shellHost.ReleaseShellContextAsync(_shellSettings);
        }

        return Edit(settings, context);
    }
}
