using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Orchard.Hosting.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Demo
{
    public class DemoMiddlewareProvider : IMiddlewareProvider
    {
        public IEnumerable<MiddlewareRegistration> GetMiddlewares()
        {
            yield return new MiddlewareRegistration
            {
                Configure = builder => builder.UseMiddleware<BlockingMiddleware>(),
                Priority = "1"
            };

            yield return new MiddlewareRegistration
            {
                Configure = builder => builder.UseMiddleware<NonBlockingMiddleware>(),
                Priority = "2"
            };

        }
    }

    public class NonBlockingMiddleware
    {
        private readonly RequestDelegate _next;

        public NonBlockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.Add("Orchard", "2.0");
            await _next.Invoke(httpContext);
        }
    }

    public class BlockingMiddleware
    {
        private readonly RequestDelegate _next;

        public BlockingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path == "/middleware")
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
