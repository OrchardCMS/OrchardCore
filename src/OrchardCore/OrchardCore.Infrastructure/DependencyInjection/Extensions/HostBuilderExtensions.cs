using Microsoft.Extensions.Hosting;

namespace OrchardCore.DependencyInjection;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseOrchardCoreHost(this IHostBuilder builder)
        => builder.UseServiceProviderFactory(new OrchardCoreServiceProviderFactory());
}
