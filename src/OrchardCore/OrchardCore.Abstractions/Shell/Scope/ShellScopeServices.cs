using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Environment.Shell.Scope;

/// <summary>
/// Makes an 'IServiceProvider' aware of the current 'ShellScope'.
/// </summary>
public class ShellScopeServices(IServiceProvider services) : IKeyedServiceProvider
{
    private readonly IServiceProvider _services = services;

    private IServiceProvider Services
        => ShellScope.Services ?? _services;

    public object GetKeyedService(Type serviceType, object serviceKey)
        => Services.GetKeyedService(serviceType, serviceKey);

    public object GetRequiredKeyedService(Type serviceType, object serviceKey)
        => Services.GetRequiredKeyedService(serviceType, serviceKey);

    public object GetService(Type serviceType)
        => Services?.GetService(serviceType);
}
