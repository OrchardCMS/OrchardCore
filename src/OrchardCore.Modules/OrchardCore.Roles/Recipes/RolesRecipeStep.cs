using OrchardCore.Recipes.Schema;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Recipes;

public sealed class RolesRecipeStep : RecipeImportStep<RolesRecipeStep.RolesStepModel>
{
    private readonly RoleManager<IRole> _roleManager;
    private readonly ISystemRoleProvider _systemRoleProvider;

    public RolesRecipeStep(
        RoleManager<IRole> roleManager,
        ISystemRoleProvider systemRoleProvider)
    {
        _roleManager = roleManager;
        _systemRoleProvider = systemRoleProvider;
    }

    public override string Name => "Roles";

    protected override JsonSchema BuildSchema()
    {
        return new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .TypeObject()
            .Title("Roles")
            .Description("Creates or updates roles with permissions.")
            .Required("name", "Roles")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(Name)
                    .Description("The name of the recipe step.")),
                ("Roles", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Items(new RecipeStepSchemaBuilder()
                        .TypeObject()
                        .Required("Name")
                        .Properties(
                            ("Name", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("Description", new RecipeStepSchemaBuilder()
                                .TypeString()),
                            ("Permissions", new RecipeStepSchemaBuilder()
                                .TypeArray()
                                .Items(new RecipeStepSchemaBuilder().TypeString())),
                            ("PermissionBehavior", new RecipeStepSchemaBuilder()
                                .TypeString()
                                .Enum("Add", "Replace", "Remove")
                                .Description("How permissions are merged: Add (default), Replace, or Remove.")))
                        .AdditionalProperties(true))
                    .MinItems(1)))
            .AdditionalProperties(true)
            .Build();
    }

    protected override async Task ImportAsync(RolesStepModel model, RecipeExecutionContext context)
    {
        foreach (var roleEntry in model.Roles)
        {
            var roleName = roleEntry.Name?.Trim();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                continue;
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            var isNewRole = role == null;

            if (isNewRole)
            {
                role = new Role
                {
                    RoleName = roleName,
                };
            }

            if (role is Role r)
            {
                r.RoleDescription = roleEntry.Description;

                if (roleEntry.PermissionBehavior == PermissionBehavior.Replace)
                {
                    r.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
                }

                if (!_systemRoleProvider.IsAdminRole(roleName))
                {
                    if (roleEntry.PermissionBehavior == PermissionBehavior.Remove)
                    {
                        var permissions = r.RoleClaims.Where(c => c.ClaimType == Permission.ClaimType && roleEntry.Permissions.Contains(c.ClaimValue)).ToArray();

                        foreach (var permission in permissions)
                        {
                            r.RoleClaims.Remove(permission);
                        }
                    }
                    else
                    {
                        var permissions = roleEntry.Permissions.Select(RoleClaim.Create)
                            .Where(newClaim => !r.RoleClaims.Exists(existingClaim => existingClaim.ClaimType == newClaim.ClaimType && existingClaim.ClaimValue == newClaim.ClaimValue));

                        r.RoleClaims.AddRange(permissions);
                    }
                }
            }

            if (isNewRole)
            {
                await _roleManager.CreateAsync(role);
            }
            else
            {
                await _roleManager.UpdateAsync(role);
            }
        }
    }

    public sealed class RolesStepModel
    {
        public RolesStepRoleModel[] Roles { get; set; }
    }

    public sealed class RolesStepRoleModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Permissions { get; set; }
        public PermissionBehavior PermissionBehavior { get; set; }
    }

    public enum PermissionBehavior
    {
        Replace,
        Add,
        Remove,
    }
}
