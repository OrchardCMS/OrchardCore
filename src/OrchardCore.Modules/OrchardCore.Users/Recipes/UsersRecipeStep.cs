using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trivest.Connect.Security.Recipes
{
    public class UsersRecipeStep : IRecipeStepHandler
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly UserManager<IUser> _userManager;

        public UsersRecipeStep(IUserService userService, ILogger<UsersRecipeStep> logger)
        {
            _logger = logger;
            _userService = userService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Users", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var models = context.Step.ToObject<UsersModel>();

            foreach (var model in models.Users)
            {
                var user = await _userService.GetUserAsync(model.UserName) as User;
                if (user == null)
                {
                    user = new User()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        EmailConfirmed = true,
                        RoleNames = model.Roles
                    };

                    bool valid = true;

                    await _userService.CreateUserAsync(user, model.Password, (key, message) =>
                    {
                        valid = false;
                        _logger.LogError("Failed to create user: {0}", model.UserName);
                    });

                    if (valid)
                    {
                        _logger.LogInformation("User created: {0}", model.UserName);
                    }
                }
                else
                {
                    _logger.LogInformation("User: {0} allready exists", model.UserName);
                }
            }
        }
    }

    public class UsersModel
    {
        public IEnumerable<UserModel> Users { get; set; }
    }

    public class UserModel
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string[] Roles { get; set; }
    }
}
