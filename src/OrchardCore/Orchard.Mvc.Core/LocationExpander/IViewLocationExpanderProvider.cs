using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.Mvc.LocationExpander
{
    public interface IViewLocationExpanderProvider : IViewLocationExpander
    {
        int Priority { get; }
    }
}
