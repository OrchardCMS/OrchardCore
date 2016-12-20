using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Roles.Recipes
{
    public class RolesStep : RecipeExecutionStep
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;


        public RolesStep(RoleManager<Role> roleManager,
                            IEnumerable<IPermissionProvider> permissionProviders,
                            ILogger<RolesStep> logger, 
                            IStringLocalizer<RolesStep> stringLocalizer) : base(logger, stringLocalizer)
        {
            _roleManager = roleManager;
            _permissionProviders = permissionProviders;
        }

        public override string Name => "Roles";

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step.ToObject<RolesStepModel>();
            
            foreach (var roleModel in model.Roles)
            {
                if (string.IsNullOrWhiteSpace(roleModel.Name)) 
                    continue;

                var role = await _roleManager.FindByNameAsync(roleModel.Name);
                bool isNewRole = role == null;
                
                if (isNewRole)
                {
                    role = new Role { RoleName = roleModel.Name };                    
                }
                role.RoleClaims.RemoveAll(c => c.ClaimType == Permission.ClaimType);
                role.RoleClaims.AddRange(roleModel.Permissions.Select(p=>new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = p }));
                
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
    }

    public class RolesStepModel
    {
        public IEnumerable<RoleModel> Roles { get; set; }
    }

    public class RoleModel
    {
        public string Name { get; set; }
        public IEnumerable<string> Permissions { get; set; }
    }

    
}
