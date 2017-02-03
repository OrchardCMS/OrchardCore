using Microsoft.AspNetCore.Mvc.Razor;

namespace Microsoft.AspNetCore.Mvc.Modules.LocationExpander
{
    public interface IViewLocationExpanderProvider : IViewLocationExpander
    {
        double Priority { get; }
    }
}
