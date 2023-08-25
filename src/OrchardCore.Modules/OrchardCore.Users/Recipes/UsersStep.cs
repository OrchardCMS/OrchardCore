using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using YesSql;

namespace OrchardCore.Users.Recipes
{
    public class UsersStep : IRecipeStepHandler {
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _userService;
        private ISession _session;
        public UsersStep(UserManager<IUser> userManager, IUserService userService, ISession session)
        {
            _userManager = userManager;
            _userService = userService;
            _session = session;
        }
        
        public async Task ExecuteAsync(RecipeExecutionContext context) {

            if (!String.Equals(context.Name, "Users", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var model = context.Step.ToObject<UsersStepModel>();

            foreach (var importedUser in model.Users) {
                if (String.IsNullOrWhiteSpace(importedUser.UserName))
                    continue;

                var iUser = await _userManager.FindByIdAsync(importedUser.UserId);    
                User user;
                var isNewUser = iUser == null;

                if (isNewUser) {
                    user = new User { UserId = importedUser.UserId };
                } else {
                    user = iUser as User;
                }

                user.Email = importedUser.Email;
                user.UserName = importedUser.UserName;
                //user.Id = importedUser.Id; // This is problematic because it's an identity column
                user.EmailConfirmed = importedUser.EmailConfirmed;
                user.PasswordHash = importedUser.PasswordHash;
                user.IsEnabled = importedUser.IsEnabled;
                user.NormalizedEmail = importedUser.NormalizedEmail;
                user.NormalizedUserName = importedUser.NormalizedUserName;
                user.SecurityStamp = importedUser.SecurityStamp;
                user.ResetToken = importedUser.ResetToken;

                if (isNewUser) {
                    _session.Save(user);
                } else {
                    _session.Save(user);
                }
            }
        }

        public class UsersStepModel {
            public UsersStepUserModel[] Users { get; set; }
        }
    }

    public class UsersStepUserModel {

        public long Id { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string SecurityStamp { get; set; }
        public string ResetToken { get; set; }
    }
}