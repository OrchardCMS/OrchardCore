using System;
using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission RecipeExecutor = new Permission("RecipeExecutor", "Execute recipes from the admin UI");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                RecipeExecutor
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}
