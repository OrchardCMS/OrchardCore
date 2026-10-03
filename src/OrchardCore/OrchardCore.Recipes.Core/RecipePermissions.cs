using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes;

public static class RecipePermissions
{
    public static readonly Permission ManageRecipes = new Permission("ManageRecipes", new LocalizationSource("Manage Recipes", typeof(RecipePermissions)), isSecurityCritical: true);
}
