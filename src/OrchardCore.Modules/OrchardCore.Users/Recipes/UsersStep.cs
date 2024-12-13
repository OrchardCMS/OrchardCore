using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Recipes;

public sealed class UsersStep : NamedRecipeStepHandler
{
    private readonly UserManager<IUser> _userManager;
    private readonly ISession _session;

    public UsersStep(
        UserManager<IUser> userManager,
        ISession session)
        : base("Users")
    {
        _userManager = userManager;
        _session = session;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
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
                user = new User
                {
                    UserId = importedUser.UserId
                };
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
            user.AccessFailedCount = importedUser.AccessFailedCount;
            user.IsLockoutEnabled = importedUser.IsLockoutEnabled;
            user.TwoFactorEnabled = importedUser.TwoFactorEnabled;
            user.PhoneNumber = importedUser.PhoneNumber;
            user.PhoneNumberConfirmed = importedUser.PhoneNumberConfirmed;
            user.RoleNames = importedUser.RoleNames;

            await _session.SaveAsync(user);
        }
    }
}
