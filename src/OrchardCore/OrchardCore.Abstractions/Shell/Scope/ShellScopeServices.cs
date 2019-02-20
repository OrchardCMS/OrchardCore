using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Shell.Scope
{
    /// <summary>
    /// Custom 'IServiceProvider' aware of 'ShellScope' switchings.
    /// </summary>
    public class ShellScopeServices : IServiceProvider
    {
        private readonly IServiceProvider _services;

        public ShellScopeServices(IServiceProvider services) => _services = services;

        private IServiceProvider Services => ShellScope.Services ?? _services;

        public object GetService(Type serviceType) => Services?.GetService(serviceType);
    }

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Makes 'RequestServices' aware of 'ShellScope' switchings.
        /// </summary>
        public static HttpContext UseShellScopeServices(this HttpContext context)
        {
            context.RequestServices = new ShellScopeServices(context.RequestServices);
            return context;
        }
    }
}
