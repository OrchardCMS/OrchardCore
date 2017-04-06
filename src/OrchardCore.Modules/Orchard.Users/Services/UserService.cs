using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Orchard.Users.Services
{
    public class UserService: IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IStringLocalizer<UserService> T;
        public UserService(UserManager<User> userManager, IOptions<IdentityOptions> identityOptions, IStringLocalizer<UserService> stringLocalizer)
        {
            _userManager = userManager;
            _identityOptions = identityOptions;
            T = stringLocalizer;
        }

        public async Task<bool> CreateUserAsync(User user, string password, Action<string, string> reportError)
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(user.UserName))
            {
                reportError("UserName", T["A user name is required."]);
                result = false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                reportError("Password", T["A password is required."]);
                result = false;
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                reportError("Email", T["An email is required."]);
                result = false;
            }

            if (!result)
            {
                return result;
            }

            if (await _userManager.FindByEmailAsync(user.Email) != null)
            {
                reportError(string.Empty, T["The email is already used."]);
                return false;
            }
            
            var identityResult = await _userManager.CreateAsync(user, password);
                        
            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, user, reportError);
            }

            return identityResult.Succeeded;
        }


        public async Task<bool> ChangePasswordAsync(User user, string currentPassword, string newPassword, Action<string, string> reportError)
        {
            var identityResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if(!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, user, reportError);
            }

            return identityResult.Succeeded;
        }

        public Task<User> GetAuthenticatedUserAsync(ClaimsPrincipal principal)
        {
            if(principal == null)
            {
                return Task.FromResult<User>(null);
            }

            return _userManager.GetUserAsync(principal);
        }

        private void ProcessValidationErrors(IEnumerable<IdentityError> errors, User user, Action<string, string> reportError)
        {
            foreach (var error in errors)
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
                        reportError("Password", T["Passwords must be at least {0} characters.", _identityOptions.Value.Password.RequiredLength]);
                        break;

                    // CurrentPassword
                    case "PasswordMismatch":
                        reportError("CurrentPassword", T["Incorrect password."]);
                        break;

                    // User name
                    case "InvalidUserName":
                        reportError("UserName", T["User name '{0}' is invalid, can only contain letters or digits.", user.UserName]);
                        break;
                    case "DuplicateUserName":
                        reportError("UserName", T["User name '{0}' is already used.", user.UserName]);
                        break;

                    // Email
                    case "InvalidEmail":
                        reportError("Email", T["Email '{0}' is invalid.", user.Email]);
                        break;
                    default:
                        reportError(string.Empty, T["Unexpected error: '{0}'.", error.Code]);
                        break;
                }
            }
        }
    }
}
