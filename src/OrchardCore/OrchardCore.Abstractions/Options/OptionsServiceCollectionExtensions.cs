using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// Registers Orchard Core's signal-backed <see cref="IOptionsChangeTokenSource{TOptions}"/>
    /// for the default named options instance so the standard <see cref="IOptionsMonitor{TOptions}"/>
    /// observes post-commit update notifications.
    /// </summary>
    /// <typeparam name="TOptions">The options type to observe.</typeparam>
    public static IServiceCollection AddSignalOptionsChangeTokenSource<TOptions>(this IServiceCollection services)
        where TOptions : class
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<TOptions>, SignalOptionsChangeTokenSource<TOptions>>());

        return services;
    }
}
