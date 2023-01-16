using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Abstractions.Setup;
using OrchardCore.Security;
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
        private readonly RoleManager<IRole> _roleManager;

        public SetupEventHandler(IUserService userService, RoleManager<IRole> roleManager)
        {
            _userService = userService;
            _roleManager = roleManager;
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
                EmailConfirmed = true
            };

            user.RoleNames.Add(_roleManager.NormalizeKey("Administrator"));

            return _userService.CreateUserAsync(user, properties[SetupConstants.AdminPassword]?.ToString(), reportError);
        }
    }
}
