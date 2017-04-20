using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemingViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            feature.Views["/_ViewStart.cshtml"] = typeof(ThemeViewStart);
            feature.Views["/Views/Shared/_Layout.cshtml"] = typeof(ThemeLayout);
        }
    }
}
