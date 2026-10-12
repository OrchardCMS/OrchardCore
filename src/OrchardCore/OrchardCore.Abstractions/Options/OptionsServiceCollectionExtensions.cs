using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
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

    /// <summary>
    /// Registers Orchard Core's signal-backed <see cref="IOptionsChangeTokenSource{TOptions}"/>
    /// for the specified named options instance so the standard <see cref="IOptionsMonitor{TOptions}"/>
    /// observes post-commit update notifications sent with <see cref="IOptionsUpdateNotifier.RequestUpdate{TOptions}(string)"/>
    /// for the same name. Registering the same name more than once has no effect.
    /// </summary>
    /// <typeparam name="TOptions">The options type to observe.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The named options instance to observe, e.g. an authentication scheme name.</param>
    public static IServiceCollection AddSignalOptionsChangeTokenSource<TOptions>(this IServiceCollection services, string name)
        where TOptions : class
    {
        name ??= Options.Options.DefaultName;

        if (name == Options.Options.DefaultName)
        {
            return services.AddSignalOptionsChangeTokenSource<TOptions>();
        }

        // A factory registration can't be compared, so a marker instance tracks the names already registered.
        if (services.Any(descriptor =>
            descriptor.ServiceType == typeof(SignalOptionsChangeTokenSourceName<TOptions>) &&
            descriptor.ImplementationInstance is SignalOptionsChangeTokenSourceName<TOptions> registration &&
            registration.Name == name))
        {
            return services;
        }

        services.AddSingleton(new SignalOptionsChangeTokenSourceName<TOptions>(name));
        services.AddSingleton<IOptionsChangeTokenSource<TOptions>>(serviceProvider =>
            new SignalOptionsChangeTokenSource<TOptions>(serviceProvider.GetRequiredService<ISignal>(), name));

        return services;
    }

    private sealed class SignalOptionsChangeTokenSourceName<TOptions>
    {
        public SignalOptionsChangeTokenSourceName(string name) => Name = name;

        public string Name { get; }
    }
}
