using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.ImageSharp.Services;
using OrchardCore.Media.Processing;
using OrchardCore.Modules;

namespace OrchardCore.Media.ImageSharp;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // The default engine. The OrchardCore.Media feature depends on this one, so features
        // depending on OrchardCore.Media, like OrchardCore.Media.NetVips, can replace it.
        services.AddSingleton<IImageProcessingEngine, ImageSharpImageProcessingEngine>();
    }
}
