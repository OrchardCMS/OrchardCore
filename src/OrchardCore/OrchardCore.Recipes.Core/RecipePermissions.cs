using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Recipes;

public static class RecipePermissions
{
    public static readonly Permission ManageRecipes = new Permission("ManageRecipes", LocalizedString.Create("Manage Recipes", typeof(RecipePermissions)), isSecurityCritical: true);
}
