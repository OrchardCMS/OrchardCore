using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public interface IViewsFeatureAlteration
    {
        void Alter(ViewsFeature viewsFeature);
    }
}
