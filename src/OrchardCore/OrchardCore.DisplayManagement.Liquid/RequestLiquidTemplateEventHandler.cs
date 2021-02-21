using System;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid
{
    // TODO this code remains for backwards compatability. It can be removed in a futre release.
    /// <summary>
    /// Provides access to <see cref="HttpRequest"/> properties if the template is running in
    /// a web request.
    /// </summary>
    public class RequestLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private HttpContext _httpContext;

        public RequestLiquidTemplateEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            // Reuse the value as the service can be resolved by multiple templates
            _httpContext = _httpContext ?? _serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;

            if (_httpContext != null)
            {
                context.SetValue("Request", _httpContext.Request);
                context.SetValue("HttpContext", _httpContext);
            }

            return Task.CompletedTask;
        }
    }
}
