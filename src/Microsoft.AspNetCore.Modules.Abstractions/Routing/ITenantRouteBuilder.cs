using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Modules.Routing
{
    public interface ITenantRouteBuilder
    {
        IRouteBuilder Build();

        void Configure(IRouteBuilder builder);
    }
}
