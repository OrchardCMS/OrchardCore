using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Info.Extensions;
using Orchard.Environment.Extensions.Info.Features;
using Orchard.Environment.Extensions.Info.Manifests;
using System.IO;
using Xunit;

namespace Orchard.Tests.Extensions
{
    public class ExtensionProviderTests
    {
        private static IFileProvider RunningTestFileProvider
            = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Extensions", "TestModules"));

        private IExtensionProvider Provider = 
                new ExtensionProvider(
                    RunningTestFileProvider, 
                    new ManifestBuilder(new ManifestProvider(RunningTestFileProvider)),
                    new FeatureManager());

        [Fact]
        public void ThatGetExtensionInfoShouldReturnExtensionWhenManifestIsPresent() {
            var extension = Provider.GetExtensionInfo("sample1");

            Assert.Equal("Sample1", extension.Id);
            Assert.True(extension.ExtensionFileInfo.Exists);
        }

        [Fact]
        public void ThatGetExtensionInfoShouldReturnNullWhenManifestDoesNotExist()
        {
            var extension = Provider.GetExtensionInfo("sample2");

            Assert.Null(extension);
        }
    }
}
