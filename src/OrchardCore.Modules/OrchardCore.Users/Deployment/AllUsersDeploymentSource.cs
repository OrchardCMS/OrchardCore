using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Deployment;
using OrchardCore.Users.Models;
using OrchardCore.Users.Recipes;
using YesSql;

namespace OrchardCore.Users.Deployment
{
    public class AllUsersDeploymentSource : IDeploymentSource
    {
        private readonly ISession _session;

        public AllUsersDeploymentSource(ISession session)
        {
            _session = session;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var allRolesStep = step as AllUsersDeploymentStep;

            if (allRolesStep == null)
            {
                return;
            }

            // Get all users
            var allUsers = await _session.Query<User>().ListAsync(); ;
            var users = new JArray();
            var tasks = new List<Task>();

            foreach (var user in allUsers)
            {
                // TODO: check if user has permission to export users
                users.Add(JObject.FromObject(
                    new UsersStepUserModel
                    {
                        UserName = user.UserName,
                        UserId = user.UserId,
                        Email = user.Email,
                        Id = user.Id,
                        EmailConfirmed = user.EmailConfirmed,
                        PasswordHash = user.PasswordHash,
                        IsEnabled = user.IsEnabled,
                        NormalizedEmail = user.NormalizedEmail,
                        NormalizedUserName = user.NormalizedUserName,
                        SecurityStamp = user.SecurityStamp,
                        ResetToken = user.ResetToken
                        //Permissions = currentRole.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue).ToArray()
                    }));
            }

            result.Steps.Add(new JObject(
                new JProperty("name", "Users"),
                new JProperty("Users", users)
            ));
        }
    }
}
