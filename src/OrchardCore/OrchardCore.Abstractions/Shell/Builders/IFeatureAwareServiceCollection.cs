using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>
/// A tenant <see cref="IServiceCollection"/> that knows which feature is registering services.
/// </summary>
/// <remarks>
/// The service collection passed to the <c>ConfigureServices()</c> method of a module startup implements this
/// interface, so a registration can find out which feature it belongs to, for instance to only expose something
/// while that feature is enabled. The host level service collection doesn't implement it.
/// </remarks>
public interface IFeatureAwareServiceCollection : IServiceCollection
{
    /// <summary>
    /// Gets the feature whose startup is currently registering services, or <see langword="null"/> when services are
    /// not registered by a feature startup.
    /// </summary>
    IFeatureInfo CurrentFeature { get; }
}
