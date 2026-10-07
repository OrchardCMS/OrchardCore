#nullable enable

using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Media.ImageSharpV3.Engine;
using OrchardCore.Media.Processing;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public sealed class ImageSharpV3StartupTests
{
    [Fact]
    public void ConfigureServices_ReplacesDefaultEngineWithImageSharp_Succeeds()
    {
        var services = new ServiceCollection();

        // Mirror the default registration from the OrchardCore.Media module.
        services.AddSingleton<IImageProcessingEngine, VipsImageProcessingEngine>();

        // The ImageSharpV3 feature Startup runs after the Media module and replaces the engine.
        new global::OrchardCore.Media.ImageSharpV3.Startup().ConfigureServices(services);

        using var provider = services.BuildServiceProvider();

        var engine = provider.GetRequiredService<IImageProcessingEngine>();

        Assert.IsType<ImageSharpImageProcessingEngine>(engine);

        // A single engine must be registered after the replacement.
        Assert.Single(services, d => d.ServiceType == typeof(IImageProcessingEngine));
    }
}
