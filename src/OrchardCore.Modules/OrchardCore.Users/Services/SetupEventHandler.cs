using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                UserName = properties["AdminUsername"]?.ToString(),
                UserId = properties["AdminUserId"]?.ToString(),
                Email =properties["AdminEmail"]?.ToString(),
                RoleNames = new string[] { "Administrator" },
                EmailConfirmed = true
            };

            return _userService.CreateUserAsync(user, properties["AdminPassword"]?.ToString(), reportError);
        }
    }
}
