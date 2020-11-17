using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Email;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Provides validation services for user accounts.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    public class UserAccountValidator<TUser> : IUserValidator<TUser> where TUser : class, IUser
    {
        private readonly IdentityErrorDescriber _identityErrorDescriber;
        private readonly IEmailAddressValidator _emailAddressValidator;

        public UserAccountValidator(IdentityErrorDescriber identityErrorDescriber, IEmailAddressValidator emailAddressValidator)
        {
            _identityErrorDescriber = identityErrorDescriber;
            _emailAddressValidator = emailAddressValidator;
        }

        /// <summary>
        /// Validates the specified <paramref name="user"/> as an asynchronous operation.
        /// </summary>
        /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
        /// <param name="user">The user to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the validation operation.</returns>
        public virtual async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var errors = new List<IdentityError>();

            await ValidateInternalAsync(manager, user, errors);

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateInternalAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
        {
            // Validate username.
            var userName = await manager.GetUserNameAsync(user);
            if (String.IsNullOrWhiteSpace(userName))
            {
                errors.Add(_identityErrorDescriber.InvalidUserName(userName));
                return;
            }

            if (!String.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                userName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors.Add(_identityErrorDescriber.InvalidUserName(userName));
                return;
            }

            // Validate that the username is unique when compared against other usernames.
            var other = await manager.FindByNameAsync(userName);
            if (other != null && !String.Equals(await manager.GetUserIdAsync(other), await manager.GetUserIdAsync(user), StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(_identityErrorDescriber.DuplicateUserName(userName));
                return;
            }

            // Validate email.
            var email = await manager.GetEmailAsync(user);
            if (String.IsNullOrWhiteSpace(email))
            {
                errors.Add(_identityErrorDescriber.InvalidEmail(email));
                return;
            }

            if (!_emailAddressValidator.Validate(email))
            {
                errors.Add(_identityErrorDescriber.InvalidEmail(email));
                return;
            }

            // Validate that if the user name is an email address, that it should match the email address.
            if (_emailAddressValidator.Validate(userName) && !String.Equals(userName, email, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new IdentityError { Code = "EmailAndUserNameShouldMatch", Description = "When the user name is an email address it should match the email address"});
                return ;
            }

            // Validate that the email address is unique when compared against other email addresses.
            other = await manager.FindByEmailAsync(email);
            if (other != null && !String.Equals(await manager.GetUserIdAsync(other), await manager.GetUserIdAsync(user), StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(_identityErrorDescriber.DuplicateEmail(email));
                return;
            }

            // Validate that the email address does not match an existing user name.
            other = await manager.FindByNameAsync(email);
            if (other != null && !String.Equals(await manager.GetUserIdAsync(other), await manager.GetUserIdAsync(user), StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(_identityErrorDescriber.DuplicateEmail(email));
                return;
            }

            // TODO Remove. This validation is no longer required as the 'EmailAndUserNameShouldMatch' validation removes the possibility of user hijacking.

            // Validate that the user name does not match an existing email address.
            // other = await manager.FindByEmailAsync(userName);
            // if (other != null && !String.Equals(await manager.GetUserIdAsync(other), await manager.GetUserIdAsync(user), StringComparison.OrdinalIgnoreCase))
            // {
            //     errors.Add(_identityErrorDescriber.DuplicateUserName(userName));
            // }
        }
    }
}
