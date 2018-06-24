using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Themes.Services
{
    public interface ISiteThemeService
    {
        Task<IExtensionInfo> GetSiteThemeAsync();
        Task SetSiteThemeAsync(string themeName);
        Task SetSiteThemeAsync(string themeName, RecipeDescriptor recipeDescriptor);
        Task<string> GetCurrentThemeNameAsync();
    }
}
