using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Html;
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
        private static readonly ShellFileVersionProvider _fileVersionProvider = new ShellFileVersionProvider(
            Enumerable.Empty<IStaticFileProvider>(),
            new FakeWebHostEnvironment(),
            new MemoryCache(Options.Create(new MemoryCacheOptions())));

        private static readonly OptionsWrapper<ResourceManagementOptions> _options = new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions());
        private static readonly ResourceManifestState _resourceManifestState;

        static ResourceManagerBenchmark()
        {
            var manifest1 = new ResourceManifest();
            manifest1.DefineStyle("some1").SetDependencies("dependency1").SetUrl("file://some1.txt").SetVersion("1.0.0");

            var manifest2 = new ResourceManifest();
            manifest2.DefineStyle("some2").SetDependencies("dependency2").SetUrl("file://some2.txt").SetVersion("1.0.0");

            var dependency1 = new ResourceManifest();
            dependency1.DefineStyle("dependency1").SetUrl("file://dependency1.txt").SetVersion("1.0.0");

            var dependency2 = new ResourceManifest();
            dependency2.DefineStyle("dependency2").SetUrl("file://dependency2.txt").SetVersion("1.0.0");

            _resourceManifestState = new ResourceManifestState
            {
                ResourceManifests = new List<ResourceManifest>
                {
                    manifest1, manifest2, dependency1, dependency2
                }
            };
        }

        [Benchmark]
        public void RenderStylesheet()
        {
            var manager = new ResourceManager(
                resourceProviders: null,
                resourceManifestState: _resourceManifestState,
                options: _options,
                fileVersionProvider: _fileVersionProvider);

            manager.RegisterResource("stylesheet", "some1").UseVersion("1.0.0").ShouldAppendVersion(true);
            manager.RegisterResource("stylesheet", "some2").UseVersion("1.0.0").ShouldAppendVersion(true);

            manager.RenderStylesheet(new HtmlContentBuilder());
        }
    }
}
