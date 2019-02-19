using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Shell.Scope
{
    /// <summary>
    /// Custom 'IServiceProvider' which is aware of the current 'ShellScope'.
    /// Used to replace 'RequestServices' while executing a tenant pipeline.
    /// </summary>
    public class ShellScopeServices : IServiceProvider
    {
        private readonly IServiceProvider _services;

        public ShellScopeServices(IServiceProvider services)
        {
            _services = services;
        }

        public object GetService(Type serviceType)
        {
            var services = ShellScope.Services ?? _services;
            return services.GetService(serviceType);
        }
    }

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Make 'RequestServices' aware of the current 'ShellScope'.
        /// </summary>
        public static void UseShellScopeServices(this HttpContext context)
        {
            context.RequestServices = new ShellScopeServices(context.RequestServices);
        }
    }
}
