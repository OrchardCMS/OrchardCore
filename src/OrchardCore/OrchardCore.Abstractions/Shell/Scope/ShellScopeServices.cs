using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Environment.Shell.Scope
{
    /// <summary>
    /// Custom 'IServiceProvider' which is aware of the current 'ShellScope'.
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
        /// Make 'RequestServices' aware of the current 'ShellScope'.
        /// </summary>
        public static void UseShellScopeServices(this HttpContext context)
            => context.RequestServices = new ShellScopeServices(context.RequestServices);
    }
}
