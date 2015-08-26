//using Microsoft.AspNet.Builder;
//using Microsoft.AspNet.Http;
//using Microsoft.Framework.Logging;
//using Orchard.Hosting.Extensions;
//using Orchard.Hosting.Extensions.Loaders;
//using System.Threading.Tasks;

//namespace Orchard.Hosting {
//    public class OrchardLoggingMiddleware {
//        private readonly RequestDelegate _next;
//        private readonly IExtensionLoader _loader;
//        private readonly IExtensionManager _manager;
//        private readonly ILoggerFactory _loggerFactory;

//        public OrchardLoggingMiddleware(
//            RequestDelegate next,
//            IExtensionLoader loader,
//            IExtensionManager manager,
//            ILoggerFactory loggerFactory) {

//            _next = next;
//        }

//        public async Task Invoke(HttpContext httpContext) {
//            await _next.Invoke(httpContext);
//        }
//    }
//}