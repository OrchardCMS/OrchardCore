using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Deployment;

public class AllUsersDeploymentSource : IDeploymentSource
{
    private readonly ISession _session;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AllUsersDeploymentSource(
        ISession session,
        IOptions<JsonSerializerOptions> jsonSerializerOptions)
    {
        _session = session;
        _jsonSerializerOptions = jsonSerializerOptions.Value;
    }

    public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
    {
        if (step is not AllUsersDeploymentStep)
        {
            return;
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
                }, _jsonSerializerOptions));
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Users",
            ["Users"] = users,
        });
    }
}
