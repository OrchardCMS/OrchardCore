using Orchard.Environment.Extensions.Info;
using Orchard.Environment.Extensions.Info.Physical;
using Xunit;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Linq;

namespace Orchard.Tests.Extensions
{
    public class ExtensionProviderTests
    {
        private IFileProvider RunningTestFileProvider
            = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Extensions", "TestModules"));

        [Fact]
        public void ThatGetExtensionInfoShouldReturnExtensionWhenManifestIsPresent() {
            IExtensionProvider provider = 
                new ExtensionProvider(RunningTestFileProvider, null);

            var extension = provider.GetExtensionInfo("sample1");

            Assert.Equal("Sample1", extension.Id);
            Assert.True(extension.Extension.Exists);
        }
    }
}
