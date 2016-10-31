using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Tests.Stubs;
using System.IO;
using Xunit;
using System;

namespace Orchard.Tests.Extensions
{
    public class ExtensionProviderTests
    {
        private static IFileProvider RunningTestFileProvider
            = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Extensions", "TestModules"));

        private static IHostingEnvironment HostingEnvrionment
            = new StubHostingEnvironment { ContentRootFileProvider = RunningTestFileProvider };

        private IExtensionProvider Provider =
                    new ExtensionProvider(
                        HostingEnvrionment,
                        new ManifestBuilder(new ManifestProvider(HostingEnvrionment),  new StubManifestOptions(new ManifestOption { ManifestFileName = "Module.txt" })),
                        new FeatureManager());

        [Fact]
        public void ThatGetExtensionInfoShouldReturnExtensionWhenManifestIsPresent()
        {
            var extension = Provider.GetExtensionInfo("Sample1");

            Assert.Equal("Sample1", extension.Id);
            Assert.True(extension.ExtensionFileInfo.Exists);
        }

        [Fact]
        public void ThatGetExtensionInfoShouldReturnNullWhenManifestDoesNotExist()
        {
            var extension = Provider.GetExtensionInfo("Sample2");

            Assert.Null(extension);
        }
    }

    public class StubManifestOptions : IOptions<ManifestOptions>
    {
        private ManifestOption _option;
        public StubManifestOptions(ManifestOption option) {
            _option = option;
        }

        public ManifestOptions Value
        {
            get
            {
                var options = new ManifestOptions();
                options.ManifestConfigurations.Add(_option);
                return options;
            }
        }
    }
}