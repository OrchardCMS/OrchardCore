using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Settings.Services;

/// <summary>
/// Grants access to a site settings group when the user has a permission registered for that group.
/// </summary>
public sealed class SiteSettingsAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SiteSettingsPermissionOptions _options;

    public SiteSettingsAuthorizationHandler(
        IServiceProvider serviceProvider,
        IOptions<SiteSettingsPermissionOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.HasSucceeded ||
            requirement.Permission.Name != SettingsPermissions.ManageGroupSettings.Name ||
            context.Resource is not string groupId ||
            string.IsNullOrEmpty(groupId) ||
            !_options.GroupPermissions.TryGetValue(groupId, out var permissions))
        {
            return;
        }

        var authorizationService = _serviceProvider.GetRequiredService<IAuthorizationService>();

        foreach (var permission in permissions)
        {
            if (await authorizationService.AuthorizeAsync(context.User, permission))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
