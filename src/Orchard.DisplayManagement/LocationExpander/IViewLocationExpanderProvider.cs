using Microsoft.AspNetCore.Mvc.Razor;

namespace Orchard.DisplayManagement.LocationExpander
{
    public interface IViewLocationExpanderProvider : IViewLocationExpander
    {
        double Priority { get; }
    }
}
