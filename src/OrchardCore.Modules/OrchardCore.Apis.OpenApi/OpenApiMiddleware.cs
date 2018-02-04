using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

namespace OrchardCore.Apis.OpenApi
{
    public class OpenApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly OpenApiSettings _settings;

        public OpenApiMiddleware(
            RequestDelegate next,
            OpenApiSettings settings)
        {
            _next = next;
            _settings = settings;
        }

        public Task Invoke(HttpContext context)
        {
            if (!IsOpenApiRequest(context))
            {
                return _next(context);
            }
            else
            {
                return ExecuteAsync(context);
            }
        }

        private bool IsOpenApiRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_settings.Path)
                && String.Equals(context.Request.Method, "GET", StringComparison.OrdinalIgnoreCase);
        }

        private Task ExecuteAsync(HttpContext context)
        {
            var document = new OpenApiDocument();

            document.Info = new OpenApiInfo
            {

            };

            document.Servers = new List<OpenApiServer>
            {

            };

            document.Paths = new OpenApiPaths
            {
                [""] = new OpenApiPathItem {  }
            };

            return Task.FromResult(document);
        }
    }
}
