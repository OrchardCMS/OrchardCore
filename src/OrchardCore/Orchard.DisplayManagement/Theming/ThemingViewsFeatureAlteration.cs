using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemingViewsFeatureAlteration : IViewsFeatureAlteration
    {
        public void Alter(ViewsFeature viewsFeature)
        {
            viewsFeature.Views["/_ViewStart.cshtml"] = typeof(ThemeViewStart);
            viewsFeature.Views["/Views/Shared/_Layout.cshtml"] = typeof(ThemeLayout);
        }
    }
}
