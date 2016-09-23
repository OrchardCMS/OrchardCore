using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Orchard.Events;
using Orchard.Users.Models;

namespace Orchard.Users.Services
{
    public interface ISetupEventHandler : IEventHandler
    {
        Task Setup(string userName, string email, string password, Action<string, string> reportError);
    }

    /// <summary>
    /// During setup, creates the admin user account.
    /// </summary>
    public class SetupEventHandler : ISetupEventHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public SetupEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Setup(string userName, string email, string password, Action<string, string> reportError)
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();
            var options = _serviceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;
            var T = _serviceProvider.GetRequiredService<IStringLocalizer<SetupEventHandler>>();

            var superUser = new User
            {
                UserName = userName,
                Email = email,
                RoleNames = { "Administrator" }
            };

            var result = await userManager.CreateAsync(superUser, password);

            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    switch (error.Code)
                    {
                        // Password
                        case "PasswordRequiresDigit":
                            reportError("Password", T["Passwords must have at least one digit ('0'-'9')."]);
                            break;
                        case "PasswordRequiresLower":
                            reportError("Password", T["Passwords must have at least one lowercase ('a'-'z')."]);
                            break;
                        case "PasswordRequiresUpper":
                            reportError("Password", T["Passwords must have at least one uppercase('A'-'Z')."]);
                            break;
                        case "PasswordRequiresNonAlphanumeric":
                            reportError("Password", T["Passwords must have at least one non letter or digit character."]);
                            break;
                        case "PasswordTooShort":
                            reportError("Password", T["Passwords must be at least {0} characters.", options.Password.RequiredLength]);
                            break;

                        // Password confirmation
                        case "PasswordMismatch":
                            reportError("PasswordConfirmation", T["Incorrect password."]);
                            break;

                        // User name
                        case "InvalidUserName":
                            reportError("AdminUserName", T["User name '{0}' is invalid, can only contain letters or digits.", superUser]);
                            break;

                        // Email
                        case "InvalidEmail":
                            reportError("AdminEmail", T["Email '{0}' is invalid.", email]);
                            break;
                    }

                }
            }

            return;
        }
    }
}