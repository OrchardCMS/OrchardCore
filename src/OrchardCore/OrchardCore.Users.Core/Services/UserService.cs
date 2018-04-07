using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IStringLocalizer<UserService> T;

        public UserService(UserManager<IUser> userManager, IOptions<IdentityOptions> identityOptions, IStringLocalizer<UserService> stringLocalizer)
        {
            _userManager = userManager;
            _identityOptions = identityOptions;
            T = stringLocalizer;
        }

        public async Task<IUser> CreateUserAsync(string userName, string email, string[] roleNames, string password, Action<string, string> reportError)
        {
            var result = true;

            if (string.IsNullOrWhiteSpace(userName))
            {
                reportError("UserName", T["A user name is required."]);
                result = false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                reportError("Password", T["A password is required."]);
                result = false;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                reportError("Email", T["An email is required."]);
                result = false;
            }

            if (!result)
            {
                return null;
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                reportError(string.Empty, T["The email is already used."]);
                return null;
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                RoleNames = new List<string>(roleNames)
            };

            var identityResult = await _userManager.CreateAsync(user, password);

            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, user, reportError);
                return null;
            }

            return user;
        }

        public async Task<bool> ChangePasswordAsync(IUser user, string currentPassword, string newPassword, Action<string, string> reportError)
        {
            var identityResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, (User)user, reportError);
            }

            return identityResult.Succeeded;
        }

        public Task<IUser> GetAuthenticatedUserAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return Task.FromResult<IUser>(null);
            }

            return _userManager.GetUserAsync(principal);
        }

        public async Task<IUser> GetForgotPasswordUserAsync(string userIdentifier)
        {
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                return await Task.FromResult<IUser>(null);
            }

            var iUser = await FindByUsernameOrEmailAsync(userIdentifier);
            if (iUser == null)
            {
                return await Task.FromResult<IUser>(null);
            }

            var user = (User)iUser;
            user.ResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return user;
        }

        public async Task<bool> ResetPasswordAsync(string userIdentifier, string resetToken, string newPassword, Action<string, string> reportError)
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(userIdentifier))
            {
                reportError("UserName", T["A user name or email is required."]);
                result = false;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                reportError("Password", T["A password is required."]);
                result = false;
            }

            if (string.IsNullOrWhiteSpace(resetToken))
            {
                reportError("Token", T["A token is required."]);
                result = false;
            }

            if (!result)
            {
                return result;
            }

            var iUser = await FindByUsernameOrEmailAsync(userIdentifier);
            if (iUser == null)
            {
                return false;
            }

            var identityResult = await _userManager.ResetPasswordAsync(iUser, resetToken, newPassword);

            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, (User)iUser, reportError);
            }

            return identityResult.Succeeded;
        }

        /// <summary>
        /// Gets the user, if any, associated with the normalized value of the specified identifier, which can refer both to username or email
        /// </summary>
        /// <param name="userIdentification">The username or email address to refer to</param>
        private async Task<IUser> FindByUsernameOrEmailAsync(string userIdentifier)
        {
            userIdentifier = userIdentifier.Normalize();

            var user = await _userManager.FindByNameAsync(userIdentifier);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(userIdentifier);
            }

            return await Task.FromResult(user);
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
                    case "PasswordRequiresUniqueChars":
                        reportError("Password", T["Passwords must contain at least {0} unique characters.", _identityOptions.Value.Password.RequiredUniqueChars]);
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
