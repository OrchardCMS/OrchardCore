using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Recipes
{
    /// <summary>
    /// This recipe step creates a set of roles.
    /// </summary>
    public class RolesStep : IRecipeStepHandler
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public RolesStep(
            RoleManager<IRole> roleManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _roleManager = roleManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Roles", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<RolesStepModel>(_jsonSerializerOptions);

            foreach (var importedRole in model.Roles)
            {
                if (string.IsNullOrWhiteSpace(importedRole.Name))
                {
                    continue;
                }

                var role = (Role)await _roleManager.FindByNameAsync(importedRole.Name);
                var isNewRole = role == null;

                if (isNewRole)
                {
                    role = new Role { RoleName = importedRole.Name };
                }

                role.RoleDescription = importedRole.Description;
                role.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
                role.RoleClaims.AddRange(importedRole.Permissions.Select(p => new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = p }));

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

        public class RolesStepModel
        {
            public RolesStepRoleModel[] Roles { get; set; }
        }
    }

    public class RolesStepRoleModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Permissions { get; set; }
    }
}
