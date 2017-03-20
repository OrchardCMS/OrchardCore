using Microsoft.AspNetCore.Mvc.Razor;

namespace OrchardCore.Mvc.Modules.LocationExpander
{
    public interface IViewLocationExpanderProvider : IViewLocationExpander
    {
        int Priority { get; }
    }
}
