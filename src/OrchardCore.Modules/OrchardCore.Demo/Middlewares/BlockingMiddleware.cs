using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Demo
{
    public class BlockingMiddleware
    {
        private readonly RequestDelegate _next;

        public BlockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value == "/middleware")
            {
                await httpContext.Response.WriteAsync("middleware");
            }
            else
            {
                await _next.Invoke(httpContext);
            }
        }
    }
}
