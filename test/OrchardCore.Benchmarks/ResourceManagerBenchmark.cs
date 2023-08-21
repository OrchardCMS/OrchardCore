using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Benchmark.Support;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Mvc;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Benchmark
{
    [MemoryDiagnoser]
    public class ResourceManagerBenchmark
    {
        private static readonly ShellFileVersionProvider _fileVersionProvider = new(
            Enumerable.Empty<IStaticFileProvider>(),
            new FakeWebHostEnvironment(),
            new MemoryCache(Options.Create(new MemoryCacheOptions())));

        private static readonly OptionsWrapper<ResourceManagementOptions> _options;

        static ResourceManagerBenchmark()
        {
            var options = new ResourceManagementOptions();

            var manifest1 = new ResourceManifest();
            manifest1.DefineStyle("some1").SetDependencies("dependency1").SetUrl("file://some1.txt").SetVersion("1.0.0");
            options.ResourceManifests.Add(manifest1);

            var manifest2 = new ResourceManifest();
            manifest2.DefineStyle("some2").SetDependencies("dependency2").SetUrl("file://some2.txt").SetVersion("1.0.0");
            options.ResourceManifests.Add(manifest2);

            var dependency1 = new ResourceManifest();
            dependency1.DefineStyle("dependency1").SetUrl("file://dependency1.txt").SetVersion("1.0.0");
            options.ResourceManifests.Add(dependency1);

            var dependency2 = new ResourceManifest();
            dependency2.DefineStyle("dependency2").SetUrl("file://dependency2.txt").SetVersion("1.0.0");
            options.ResourceManifests.Add(dependency2);

            _options = new OptionsWrapper<ResourceManagementOptions>(options);
        }

        [Benchmark]
#pragma warning disable CA1822 // Mark members as static
        public void RenderStylesheet()
#pragma warning restore CA1822 // Mark members as static
        {
            var manager = new ResourceManager(
                options: _options,
                fileVersionProvider: _fileVersionProvider);

            manager.RegisterResource("stylesheet", "some1").UseVersion("1.0.0").ShouldAppendVersion(true);
            manager.RegisterResource("stylesheet", "some2").UseVersion("1.0.0").ShouldAppendVersion(true);
            using var sw = new StringWriter();
            manager.RenderStylesheet(sw);
        }
    }
}
