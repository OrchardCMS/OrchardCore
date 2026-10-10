using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Builds the principal of the user a run reads and writes data for.
/// </summary>
public sealed class DataPipelineUserResolver
{
    private readonly UserManager<IUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<IUser> _claimsPrincipalFactory;

    public DataPipelineUserResolver(UserManager<IUser> userManager, IUserClaimsPrincipalFactory<IUser> claimsPrincipalFactory)
    {
        _userManager = userManager;
        _claimsPrincipalFactory = claimsPrincipalFactory;
    }

    /// <summary>
    /// Builds the principal of a user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <returns>The principal, or <see langword="null"/> when the user doesn't exist or is disabled.</returns>
    public async Task<ClaimsPrincipal> GetPrincipalAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null || (user is User { IsEnabled: false }))
        {
            return null;
        }

        return await _claimsPrincipalFactory.CreateAsync(user);
    }
}
