using System;
using System.Threading.Tasks;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Setup;
using OrchardCore.Setup.Services;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// During setup, creates the admin user account.
    /// </summary>
    public class SetupEventHandler : TenantSetupHandlerBase
    {
        private readonly IUserService _userService;

        public SetupEventHandler(IUserService userService)
        {
            _userService = userService;
        }

        public override Task SettingUpAsync(SetupContext context)
        {
            var user = new User
            {
                UserName = context.Properties.TryGetValue(SetupConstants.AdminUsername, out var adminUserName) ? adminUserName?.ToString() : String.Empty,
                UserId = context.Properties.TryGetValue(SetupConstants.AdminUserId, out var adminUserId) ? adminUserId?.ToString() : String.Empty,
                Email = context.Properties.TryGetValue(SetupConstants.AdminEmail, out var adminEmail) ? adminEmail?.ToString() : String.Empty,
                EmailConfirmed = true
            };

            user.RoleNames.Add("Administrator");

            return _userService.CreateUserAsync(user, context.Properties[SetupConstants.AdminPassword]?.ToString(), (key, message) =>
            {
                context.Errors[key] = message;
            });
        }
    }
}
