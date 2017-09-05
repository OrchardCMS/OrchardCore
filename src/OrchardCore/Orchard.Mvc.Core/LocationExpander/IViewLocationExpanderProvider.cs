using Microsoft.AspNetCore.Mvc.Razor;

namespace OrchardCore.Mvc.LocationExpander
{
    public interface IViewLocationExpanderProvider : IViewLocationExpander
    {
        int Priority { get; }
    }
}
