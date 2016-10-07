using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Info;
using System.IO;
using Xunit;

namespace Orchard.Tests.Extensions
{
    public class ExtensionProviderTests
    {
        private IFileProvider RunningTestFileProvider
            = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Extensions", "TestModules"));

        [Fact]
        public void ThatGetExtensionInfoShouldReturnExtensionWhenManifestIsPresent() {
            IExtensionProvider provider = 
                new ExtensionProvider(RunningTestFileProvider, new ManifestProvider(RunningTestFileProvider));

            var extension = provider.GetExtensionInfo("sample1");

            Assert.Equal("Sample1", extension.Id);
            Assert.True(extension.ExtensionFileInfo.Exists);
        }
    }
}
