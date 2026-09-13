using System.Text.Json.Nodes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Users.Deployment;

public sealed class CustomUserSettingsDeploymentSource
    : DeploymentSourceBase<CustomUserSettingsDeploymentStep>
{
    private readonly CustomUserSettingsService _customUserSettingsService;
    private readonly ISession _session;

    private readonly IAuthorizationService _authorization;
    private readonly IHttpContextAccessor _httpContext;

    /// <summary>Creates a source enforcing settings-type and user-resource permissions.</summary>
    public CustomUserSettingsDeploymentSource(
        CustomUserSettingsService customUserSettingsService,
        ISession session,
        IAuthorizationService authorization = null,
        IHttpContextAccessor httpContext = null)
    {
        _customUserSettingsService = customUserSettingsService;
        _session = session;
        _authorization = authorization;
        _httpContext = httpContext;
    }

    protected override async Task ProcessAsync(CustomUserSettingsDeploymentStep step, DeploymentPlanResult result)
    {
        var settingsTypes = step.IncludeAll
            ? (await _customUserSettingsService.GetAllSettingsTypesAsync()).ToArray()
            : (await _customUserSettingsService.GetSettingsTypesAsync(step.SettingsTypeNames)).ToArray();

        var principal = result.User ?? _httpContext?.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        foreach (var type in settingsTypes)
        {
            if (_authorization is null || !await _authorization.AuthorizeAsync(principal, CustomUserSettingsPermissions.CreatePermissionForType(type)))
            {
                throw new UnauthorizedAccessException("The initiating user cannot export the selected custom user settings.");
            }
        }
        var userData = new JsonArray();
        var allUsers = await _session.Query<User>().ListAsync();

        foreach (var user in allUsers)
        {
            if (_authorization is null || !await _authorization.AuthorizeAsync(principal, UsersPermissions.ViewUsers, user))
            {
                throw new UnauthorizedAccessException("The initiating user cannot export settings for the selected users.");
            }
            var userSettingsData = new JsonArray();
            foreach (var settingsType in settingsTypes)
            {
                var userSetting = await _customUserSettingsService.GetSettingsAsync(user, settingsType);
                userSettingsData.Add(JObject.FromObject(userSetting));
            }

            userData.Add(new JsonObject
            {
                ["userId"] = user.UserId,
                ["user-custom-user-settings"] = userSettingsData,
            });
        }

        // Adding custom user settings
        result.Steps.Add(new JsonObject
        {
            ["name"] = "custom-user-settings",
            ["custom-user-settings"] = userData,
        });
    }
}
