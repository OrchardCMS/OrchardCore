using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public interface IModularTenantRouteBuilder
    {
        IRouteBuilder Build(IApplicationBuilder appBuilder);
    }
}