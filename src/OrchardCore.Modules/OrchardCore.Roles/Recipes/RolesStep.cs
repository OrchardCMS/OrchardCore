using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
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

        public RolesStep(RoleManager<IRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Roles", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<RolesStepModel>();
            
            foreach (var importedRole in model.Data)
            {
                if (string.IsNullOrWhiteSpace(importedRole.NormalizedRoleName)) 
                    continue;

                var role = (Role) await _roleManager.FindByNameAsync(importedRole.NormalizedRoleName);
                var isNewRole = role == null;
                
                if (isNewRole)
                {
                    role = new Role { RoleName = importedRole.RoleName };                    
                }
                role.RoleClaims.Clear();
                role.RoleClaims.AddRange(importedRole.RoleClaims.Select(c=>new RoleClaim { ClaimType = c.ClaimType, ClaimValue = c.ClaimValue }));
                
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
            public Role[] Data { get; set; }
        }
    }    
}
