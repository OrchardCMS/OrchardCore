using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Recipes.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Environment.Extensions;

namespace Orchard.Roles.Recipes
{
    public class RolesStep : RecipeExecutionStep
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;


        public RolesStep(RoleManager<Role> roleManager,
                            ITypeFeatureProvider typeFeatureProvider,
                            IEnumerable<IPermissionProvider> permissionProviders,
                            ILoggerFactory loggerFactory, 
                            IStringLocalizer<RolesStep> stringLocalizer) : base(loggerFactory, stringLocalizer)
        {
            _roleManager = roleManager;
            _typeFeatureProvider = typeFeatureProvider;
            _permissionProviders = permissionProviders;
        }

        public override string Name
        {
            get { return "Roles"; }
        }

        public override async Task ExecuteAsync(RecipeExecutionContext recipeContext)
        {
            var model = recipeContext.RecipeStep.Step.ToObject<RolesStepModel>();
            var validPermissions = GetValidPermissions();

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
                foreach (var permission in roleModel.Permissions)
                {
                    if (validPermissions.Contains(permission))
                    {
                        role.RoleClaims.Add(new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = permission });
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

        private ICollection<string> GetValidPermissions()
        {
            var result = new HashSet<string>();
            foreach (var permissionProvider in _permissionProviders)
            {
                var feature = _typeFeatureProvider.GetFeatureForDependency(permissionProvider.GetType());
                var featureName = feature.Id;
                var permissions = permissionProvider.GetPermissions();
                foreach (var permission in permissions)
                {
                    result.Add(permission.Name);
                }
            }
            return result;
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
