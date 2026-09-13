using System.Text.Json.Nodes;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Users.Deployment;

public sealed class AllUsersDeploymentSource
    : DeploymentSourceBase<AllUsersDeploymentStep>
{
    private readonly ISession _session;

    private readonly IAuthorizationService _authorization;
    private readonly IHttpContextAccessor _httpContext;

    /// <summary>Creates a user export source requiring permission to manage credential-bearing user records.</summary>
    public AllUsersDeploymentSource(ISession session, IAuthorizationService authorization = null, IHttpContextAccessor httpContext = null)
    {
        _session = session;
        _authorization = authorization;
        _httpContext = httpContext;
    }

    protected override async Task ProcessAsync(AllUsersDeploymentStep step, DeploymentPlanResult result)
    {
        var principal = result.User ?? _httpContext?.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        if (_authorization is null || !await _authorization.AuthorizeAsync(principal, Permissions.ManageUsers))
        {
            throw new UnauthorizedAccessException("The initiating user cannot export credential-bearing user records.");
        }
        var allUsers = await _session.Query<User>().ListAsync();
        var users = new JsonArray();

        foreach (var user in allUsers)
        {
            users.Add(JObject.FromObject(
                new UsersStepUserModel
                {
                    UserName = user.UserName,
                    UserId = user.UserId,
                    Id = user.Id,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PasswordHash = user.PasswordHash,
                    IsEnabled = user.IsEnabled,
                    NormalizedEmail = user.NormalizedEmail,
                    NormalizedUserName = user.NormalizedUserName,
                    SecurityStamp = user.SecurityStamp,
                    ResetToken = user.ResetToken,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    IsLockoutEnabled = user.IsLockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    RoleNames = user.RoleNames,
                }));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Users",
            ["Users"] = users,
        });
    }
}
