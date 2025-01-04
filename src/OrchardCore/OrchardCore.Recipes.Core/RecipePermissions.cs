using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes;

public static class RecipePermissions
{
    public static readonly Permission ManageRecipes = new Permission("ManageRecipes", "Manage Recipes", isSecurityCritical: true);
}
