#nullable enable

using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.ImageSharp.Services;
using OrchardCore.Media.NetVips.Services;
using OrchardCore.Media.Processing;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class ImageProcessingEngineStartupTests
{
    [Fact]
    public void ConfigureServices_ImageSharpFeature_RegistersDefaultEngine()
    {
        var services = new ServiceCollection();

        new global::OrchardCore.Media.ImageSharp.Startup().ConfigureServices(services);

        using var provider = services.BuildServiceProvider();

        Assert.IsType<ImageSharpImageProcessingEngine>(provider.GetRequiredService<IImageProcessingEngine>());
    }

    [Fact]
    public void ConfigureServices_NetVipsFeature_ReplacesDefaultEngine()
    {
        var services = new ServiceCollection();

        // The OrchardCore.Media feature depends on the ImageSharp feature, and the NetVips feature
        // depends on OrchardCore.Media, so their startups run in this order.
        new global::OrchardCore.Media.ImageSharp.Startup().ConfigureServices(services);
        new global::OrchardCore.Media.NetVips.Startup().ConfigureServices(services);

        using var provider = services.BuildServiceProvider();

        Assert.IsType<VipsImageProcessingEngine>(provider.GetRequiredService<IImageProcessingEngine>());
        Assert.Single(services, d => d.ServiceType == typeof(IImageProcessingEngine));
    }
}
