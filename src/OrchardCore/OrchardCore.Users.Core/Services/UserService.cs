using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Implements <see cref="IUserService"/> by using the ASP.NET Core Identity packages.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly SignInManager<IUser> _signInManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IEnumerable<IPasswordRecoveryFormEvents> _passwordRecoveryFormEvents;
        private readonly IStringLocalizer S;
        private readonly ILogger _logger;

        public UserService(
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager,
            IOptions<IdentityOptions> identityOptions,
            IEnumerable<IPasswordRecoveryFormEvents> passwordRecoveryFormEvents,
            IStringLocalizer<UserService> stringLocalizer,
            ILogger<UserService> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _identityOptions = identityOptions;
            _passwordRecoveryFormEvents = passwordRecoveryFormEvents;
            S = stringLocalizer;
            _logger = logger;
        }

        public async Task<IUser> AuthenticateAsync(string userName, string password, Action<string, string> reportError)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                reportError("UserName", S["A user name is required."]);
                return null;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                reportError("Password", S["A password is required."]);
                return null;
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                reportError(string.Empty, S["The specified username/password couple is invalid."]);
                return null;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                reportError(string.Empty, S["The user is locked out."]);
                return null;
            }
            else if (result.IsNotAllowed)
            {
                reportError(string.Empty, S["The specified user is not allowed to sign in."]);
                return null;
            }
            else if (result.RequiresTwoFactor)
            {
                reportError(string.Empty, S["The specified user is not allowed to sign in using password authentication."]);
                return null;
            }
            else if (!result.Succeeded)
            {
                reportError(string.Empty, S["The specified username/password couple is invalid."]);
                return null;
            }

            if (!(user as User).IsEnabled)
            {
                reportError(string.Empty, S["The specified user is not allowed to sign in."]);

                return null;
            }

            return user;
        }

        public async Task<IUser> CreateUserAsync(IUser user, string password, Action<string, string> reportError)
        {
            if (!(user is User newUser))
            {
                throw new ArgumentException("Expected a User instance.", nameof(user));
            }

            // Accounts can be created with no password
            var identityResult = String.IsNullOrWhiteSpace(password)
                ? await _userManager.CreateAsync(user)
                : await _userManager.CreateAsync(user, password);
            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, newUser, reportError);
                return null;
            }

            return user;
        }

        public async Task<bool> ChangeEmailAsync(IUser user, string newEmail, Action<string, string> reportError)
        {
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var identityResult = await _userManager.ChangeEmailAsync(user, newEmail, token);

            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, (User)user, reportError);
            }

            return identityResult.Succeeded;
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

            var user = await _userManager.FindByEmailAsync(userIdentifier) as User;

            if (user == null)
            {
                return await Task.FromResult<IUser>(null);
            }

            user.ResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return user;
        }

        public async Task<bool> ResetPasswordAsync(string emailAddress, string resetToken, string newPassword, Action<string, string> reportError)
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                reportError("UserName", S["A email address is required."]);
                result = false;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                reportError("Password", S["A password is required."]);
                result = false;
            }

            if (string.IsNullOrWhiteSpace(resetToken))
            {
                reportError("Token", S["A token is required."]);
                result = false;
            }

            if (!result)
            {
                return result;
            }

            var user = await _userManager.FindByEmailAsync(emailAddress) as User;

            if (user == null)
            {
                return false;
            }

            var identityResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!identityResult.Succeeded)
            {
                ProcessValidationErrors(identityResult.Errors, user, reportError);
            }

            if (identityResult.Succeeded)
            {
                var context = new PasswordRecoveryContext(user);

                await _passwordRecoveryFormEvents.InvokeAsync((handler, context) => handler.PasswordResetAsync(context), context, _logger);
            }

            return identityResult.Succeeded;
        }

        public Task<ClaimsPrincipal> CreatePrincipalAsync(IUser user)
        {
            if (user == null)
            {
                return Task.FromResult<ClaimsPrincipal>(null);
            }

            return _signInManager.CreateUserPrincipalAsync(user);
        }

        public Task<IUser> GetUserAsync(string userName) => _userManager.FindByNameAsync(userName);

        public Task<IUser> GetUserByUniqueIdAsync(string userIdentifier) => _userManager.FindByIdAsync(userIdentifier);

        public void ProcessValidationErrors(IEnumerable<IdentityError> errors, User user, Action<string, string> reportError)
        {
            foreach (var error in errors)
            {
                switch (error.Code)
                {
                    // Password
                    case "PasswordRequiresDigit":
                        reportError("Password", S["Passwords must have at least one digit character ('0'-'9')."]);
                        break;
                    case "PasswordRequiresLower":
                        reportError("Password", S["Passwords must have at least one lowercase character ('a'-'z')."]);
                        break;
                    case "PasswordRequiresUpper":
                        reportError("Password", S["Passwords must have at least one uppercase character ('A'-'Z')."]);
                        break;
                    case "PasswordRequiresNonAlphanumeric":
                        reportError("Password", S["Passwords must have at least one non letter or digit character."]);
                        break;
                    case "PasswordTooShort":
                        reportError("Password", S["Passwords must be at least {0} characters.", _identityOptions.Value.Password.RequiredLength]);
                        break;
                    case "PasswordRequiresUniqueChars":
                        reportError("Password", S["Passwords must contain at least {0} unique characters.", _identityOptions.Value.Password.RequiredUniqueChars]);
                        break;

                    // CurrentPassword
                    case "PasswordMismatch":
                        reportError("CurrentPassword", S["Incorrect password."]);
                        break;

                    // User name
                    case "InvalidUserName":
                        reportError("UserName", S["User name '{0}' is invalid, can only contain letters or digits.", user.UserName]);
                        break;
                    case "DuplicateUserName":
                        reportError("UserName", S["User name '{0}' is already used.", user.UserName]);
                        break;

                    // Email
                    case "DuplicateEmail":
                        reportError("Email", S["Email '{0}' is already used.", user.Email]);
                        break;
                    case "InvalidEmail":
                        reportError("Email", S["Email '{0}' is invalid.", user.Email]);
                        break;

                    default:
                        reportError(string.Empty, S["Unexpected error: '{0}'.", error.Code]);
                        break;
                }
            }
        }
    }
}
