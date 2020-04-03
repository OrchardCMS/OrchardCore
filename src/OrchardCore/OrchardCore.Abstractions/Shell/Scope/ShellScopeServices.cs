using System;

namespace OrchardCore.Environment.Shell.Scope
{
    public class ShellScopeServices : IServiceProvider
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Makes an 'IServiceProvider' aware of the current 'ShellScope'.
        /// </summary>
        public ShellScopeServices(IServiceProvider services) => _services = services;

        private IServiceProvider Services => ShellScope.Services ?? _services;

        public object GetService(Type serviceType) => Services?.GetService(serviceType);
    }
}
