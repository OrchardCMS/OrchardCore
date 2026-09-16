using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Media.ImageSharpV3.Engine;
using OrchardCore.Media.Processing;
using OrchardCore.Modules;

namespace OrchardCore.Media.ImageSharpV3;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<IImageProcessingEngine, ImageSharpImageProcessingEngine>());
    }
}
