using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Modules
{
    public interface IModularTenantRouteBuilder
    {
        IRouteBuilder Build();

        void Configure(IRouteBuilder builder);
    }
}
