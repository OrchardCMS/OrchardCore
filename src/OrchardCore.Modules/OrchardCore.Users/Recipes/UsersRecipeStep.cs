using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Recipes
{
    public class UsersRecipeStep : IRecipeStepHandler
    {
        private readonly UserManager<IUser> _userManager;
        private readonly RoleManager<IRole> _roleManager;
        private readonly ILogger _logger;

        public UsersRecipeStep(UserManager<IUser> userManager, RoleManager<IRole> roleManager, ILogger<UsersRecipeStep> logger)
        {
            _logger = logger;

            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Users", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var models = context.Step.ToObject<UsersModel>();

            var roles = _roleManager.Roles.Select(r => r.RoleName);
            var rolesToCreate = models.Users.SelectMany(u => u.Roles).Distinct().Except(roles);

            try
            {
                // Make sure the roles exist
                foreach (var roleToCreate in rolesToCreate)
                {
                    var role = new Role()
                    {
                        RoleName = roleToCreate,
                        NormalizedRoleName = _roleManager.KeyNormalizer.NormalizeName(roleToCreate)
                    };
                    await _roleManager.CreateAsync(role);
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize roles");
            }

            // Create or update the users
            foreach (var model in models.Users)
            {
                try
                {
                    var user = await _userManager.FindByNameAsync(model.UserName) as User;
                    if (user == null)
                    {
                        user = new User()
                        {
                            UserName = model.UserName,
                            NormalizedUserName = _userManager.NormalizeName(model.UserName),
                            Email = model.Email,
                            NormalizedEmail = _userManager.NormalizeEmail(model.Email),
                            EmailConfirmed = true
                        };

                        await _userManager.CreateAsync(user, model.Password);
                        _logger.LogInformation("User created: {UserName}", model.UserName);

                        await _userManager.UpdateSecurityStampAsync(user);
                    }
                    else
                    {
                        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                        await _userManager.ResetPasswordAsync(user, resetPasswordToken, model.Password);
                        _logger.LogInformation("Password updated for user: {UserName}", model.UserName);

                        await _userManager.SetEmailAsync(user, model.Email);
                        _logger.LogInformation("Email updated for user: {UserName}", model.UserName);
                    }

                    foreach (var roleName in model.Roles)
                    {
                        await _userManager.AddToRoleAsync(user, roleName);
                        _logger.LogInformation("Assigned Role: {RoleName} to User: {UserName}", roleName, model.UserName);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Failed to create or update user");
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
