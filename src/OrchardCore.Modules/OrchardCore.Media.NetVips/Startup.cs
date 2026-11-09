using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Media.NetVips.Services;
using OrchardCore.Media.Processing;
using OrchardCore.Modules;

namespace OrchardCore.Media.NetVips;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Runs after the OrchardCore.Media feature it depends on, and its default ImageSharp engine.
        services.Replace(ServiceDescriptor.Singleton<IImageProcessingEngine, VipsImageProcessingEngine>());
    }
}
