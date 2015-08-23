using OrchardVNext.Abstractions.Environment;
using OrchardVNext.FileSystem.VirtualPath;
using Xunit;

namespace OrchardVNext.Tests.FileSystems.VirtualPath {
    public class DefaultVirtualPathProviderTests
    {
        [Fact]
        public void TryFileExistsTest()
        {
            StubDefaultVirtualPathProvider defaultVirtualPathProvider = new StubDefaultVirtualPathProvider();

            Assert.True(defaultVirtualPathProvider.TryFileExists("~/a.txt"));
            Assert.False(defaultVirtualPathProvider.TryFileExists("~/../a.txt"));
            Assert.True(defaultVirtualPathProvider.TryFileExists("~/a/../a.txt"));
            Assert.True(defaultVirtualPathProvider.TryFileExists("~/a/b/../a.txt"));
            Assert.True(defaultVirtualPathProvider.TryFileExists("~/a/b/../../a.txt"));
            Assert.False(defaultVirtualPathProvider.TryFileExists("~/a/b/../../../a.txt"));
            Assert.False(defaultVirtualPathProvider.TryFileExists("~/a/../../b/c.txt"));
        }

        [Fact]
        public void RejectMalformedVirtualPathTests()
        {
            StubDefaultVirtualPathProvider defaultVirtualPathProvider = new StubDefaultVirtualPathProvider();

            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a.txt"));
            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("/a.txt"));

            Assert.True(defaultVirtualPathProvider.IsMalformedVirtualPath("~/../a.txt"));
            Assert.True(defaultVirtualPathProvider.IsMalformedVirtualPath("/../a.txt"));

            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/../a.txt"));
            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/../a.txt"));

            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/b/../a.txt"));
            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/b/../a.txt"));

            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/b/../../a.txt"));
            Assert.False(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/b/../../a.txt"));

            Assert.True(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/b/../../../a.txt"));
            Assert.True(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/b/../../../a.txt"));

            Assert.True(defaultVirtualPathProvider.IsMalformedVirtualPath("~/a/../../b//.txt"));
            Assert.True(defaultVirtualPathProvider.IsMalformedVirtualPath("/a/../../b//.txt"));
        }
    }

    internal class StubDefaultVirtualPathProvider : DefaultVirtualPathProvider
    {
        public StubDefaultVirtualPathProvider() : base(new StubHostEnvironment(), null)
        {
        }

        public override bool FileExists(string path)
        {
            return true;
        }
    }

    internal class StubHostEnvironment : IHostEnvironment
    {
        public string MapPath(string virtualPath)
        {
            return string.Empty;
        }
    }
}