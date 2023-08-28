using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Sms;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Recipes;

public class UsersStep : IRecipeStepHandler
{
    private readonly UserManager<IUser> _userManager;
    private readonly ISession _session;

    public UsersStep(
        UserManager<IUser> userManager,
        ISession session)
    {
        _userManager = userManager;
        _session = session;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, "Users", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<UsersStepModel>();

        foreach (var importedUser in model.Users)
        {
            if (string.IsNullOrWhiteSpace(importedUser.UserName))
            {
                continue;
            }

            var iUser = await _userManager.FindByIdAsync(importedUser.UserId);

            if (iUser is not User user)
            {
                user = new User { UserId = importedUser.UserId };
            }
            else
            {
                user = iUser as User;
            }

            user.Email = importedUser.Email;
            user.UserName = importedUser.UserName;
            user.EmailConfirmed = importedUser.EmailConfirmed;
            user.PasswordHash = importedUser.PasswordHash;
            user.IsEnabled = importedUser.IsEnabled;
            user.NormalizedEmail = importedUser.NormalizedEmail;
            user.NormalizedUserName = importedUser.NormalizedUserName;
            user.SecurityStamp = importedUser.SecurityStamp;
            user.ResetToken = importedUser.ResetToken;

            _session.Save(user);
        }
    }

    public class UsersStepModel
    {
        public UsersStepUserModel[] Users { get; set; }
    }
}

public class UsersStepUserModel
{
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
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool IsLockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public IList<string> RoleNames { get; set; }
}
