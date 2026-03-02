using OrchardCore.Recipes.Schema;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Recipes;

public sealed class UsersRecipeStep : RecipeImportStep<UsersRecipeStep.UsersStepModel>
{
    private readonly UserManager<IUser> _userManager;
    private readonly ISession _session;

    public UsersRecipeStep(
        UserManager<IUser> userManager,
        ISession session)
    {
        _userManager = userManager;
        _session = session;
    }

    public override string Name => "Users";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Users")
            .Description("Imports user accounts.")
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Users", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Properties(
                            ("UserId", new RecipeStepSchemaBuilder().TypeString()),
                            ("UserName", new RecipeStepSchemaBuilder().TypeString()),
                            ("Email", new RecipeStepSchemaBuilder().TypeString()),
                            ("PasswordHash", new RecipeStepSchemaBuilder().TypeString()),
                            ("EmailConfirmed", new RecipeStepSchemaBuilder().TypeBoolean()),
                            ("IsEnabled", new RecipeStepSchemaBuilder().TypeBoolean()),
                            ("RoleNames", new RecipeStepSchemaBuilder()
                                .TypeArray()
                                .Items(new RecipeStepSchemaBuilder().TypeString())))
                        .AdditionalProperties(true))))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(UsersStepModel model, RecipeExecutionContext context)
    {
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
                    UserId = importedUser.UserId,
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

    public sealed class UsersStepModel
    {
        public UserStepModel[] Users { get; set; }
    }

    public sealed class UserStepModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsEnabled { get; set; }
        public string NormalizedEmail { get; set; }
        public string NormalizedUserName { get; set; }
        public string SecurityStamp { get; set; }
        public string ResetToken { get; set; }
        public int AccessFailedCount { get; set; }
        public bool IsLockoutEnabled { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public IList<string> RoleNames { get; set; }
    }
}
