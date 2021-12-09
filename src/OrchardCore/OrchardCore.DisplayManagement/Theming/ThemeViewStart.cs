using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// A precompiled view that resets the View's Layout to the <see cref="ThemeLayout"/>
    /// class to ensure the Layout shape is used.
    /// </summary>
    public class ThemeViewStart : RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            // Checking if the current request has an active theme before assigning a Layout shape
            var themeManager = Context.RequestServices.GetRequiredService<IThemeManager>();

            var theme = await themeManager.GetThemeAsync();

            if (theme == null)
            {
                return;
            }

            Layout = ThemingViewsFeatureProvider.ThemeLayoutFileName;
        }
    }
}
