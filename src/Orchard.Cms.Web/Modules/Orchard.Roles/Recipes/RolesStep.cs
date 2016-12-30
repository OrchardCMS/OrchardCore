using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace Orchard.Roles.Recipes
{
    /// <summary>
    /// This recipe step creates a set of roles.
    /// </summary>
    public class RolesStep : IRecipeStepHandler
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        public RolesStep(
            RoleManager<Role> roleManager,
            ITypeFeatureProvider typeFeatureProvider,
            IEnumerable<IPermissionProvider> permissionProviders)
        {
            _roleManager = roleManager;
            _typeFeatureProvider = typeFeatureProvider;
            _permissionProviders = permissionProviders;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Roles", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<RolesStepModel>();
            
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
