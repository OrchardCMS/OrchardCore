using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace OrchardCore.Routing
{
    public class EndpointFeature : IEndpointFeature
    {
        public Endpoint Endpoint { get; set; }
    }
}
