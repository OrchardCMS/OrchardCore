using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules
{
    public interface IModularTenantRouteBuilder
    {
        IRouteBuilder Build();

        void Configure(IRouteBuilder builder);
    }
}
