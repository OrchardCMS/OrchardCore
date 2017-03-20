using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orchard.ResourceManagement;

namespace Orchard.Commons
{
    public class MetaGeneratorMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly IResourceManager _resourceManager;

        public MetaGeneratorMiddleware(RequestDelegate next, IResourceManager resourceManager)
        {
            _next = next;
            _resourceManager = resourceManager;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var metaEntry = new MetaEntry("Generator", "Orchard");
            _resourceManager.RegisterMeta(metaEntry);
            await _next.Invoke(httpContext);
        }
    }
}
