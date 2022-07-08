using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Setup.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// During setup, creates the admin user account.
    /// </summary>
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IUserService _userService;

        public SetupEventHandler(IUserService userService)
        {
            _userService = userService;
        }

        public Task Setup(
            IDictionary<string, object> properties,
            Action<string, string> reportError
            )
        {
            var user = new User
            {
                UserName = properties.TryGetValue(SetupConstants.AdminUsername, out var adminUserName) ? adminUserName?.ToString() : String.Empty,
                UserId = properties.TryGetValue(SetupConstants.AdminUserId, out var adminUserId) ? adminUserId?.ToString() : String.Empty,
                Email = properties.TryGetValue(SetupConstants.AdminEmail, out var adminEmail) ? adminEmail?.ToString() : String.Empty,
                RoleNames = new string[] { "Administrator" },
                EmailConfirmed = true
            };

            return _userService.CreateUserAsync(user, properties[SetupConstants.AdminPassword]?.ToString(), reportError);
        }
    }
}
