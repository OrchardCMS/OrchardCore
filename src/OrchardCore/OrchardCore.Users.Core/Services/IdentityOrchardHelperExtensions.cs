using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;
using OrchardCore.Users;

namespace OrchardCore;

public static class IdentityOrchardHelperExtensions
{
    public static async Task<IUser> UserAsync(this IOrchardHelper helper, UserManager<IUser> userManager = null)
    {
        userManager ??= helper.HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();

        return await userManager.FindByIdAsync(await helper.UserIdAsync());
    }

    public static async Task<bool> AuthorizeAsync(this IOrchardHelper helper, Permission permission, IAuthorizationService authorizationService = null)
    {
        if (!await helper.IsAuthenticatedAsync())
        {
            return false;
        }

        authorizationService ??= helper.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        return await authorizationService.AuthorizeAsync(helper.HttpContext.User, permission);
    }

    public static async Task<bool> AuthorizeAsync(this IOrchardHelper helper, Permission permission, object resource, IAuthorizationService authorizationService = null)
    {
        if (!await helper.IsAuthenticatedAsync())
        {
            return false;
        }

        authorizationService ??= helper.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        return await authorizationService.AuthorizeAsync(helper.HttpContext.User, permission, resource);
    }

    public static async Task<Claim> ClaimAsync(this IOrchardHelper helper, string type)
    {
        if (!await helper.IsAuthenticatedAsync())
        {
            throw new UnauthorizedAccessException();
        }

        return helper.HttpContext.User.FindFirst(type);
    }

    public static async Task<IEnumerable<Claim>> ClaimsAsync(this IOrchardHelper helper)
    {
        if (!await helper.IsAuthenticatedAsync())
        {
            throw new UnauthorizedAccessException();
        }

        return helper.HttpContext.User.Claims;
    }

    public static Task<bool> IsAuthenticatedAsync(this IOrchardHelper helper)
    {
        return Task.FromResult((helper.HttpContext.User?.Identity?.IsAuthenticated) ?? false);
    }

    public static async Task<T> UserAsync<T>(this IOrchardHelper helper)
        where T : class, IUser
    {
        return (await helper.UserAsync()) as T;
    }

    public static async Task<string> UserIdAsync(this IOrchardHelper helper)
    {
        var claim = await helper.ClaimAsync(ClaimTypes.NameIdentifier);

        return (claim?.Value) ?? helper.HttpContext.User?.Identity?.Name;
    }
}
