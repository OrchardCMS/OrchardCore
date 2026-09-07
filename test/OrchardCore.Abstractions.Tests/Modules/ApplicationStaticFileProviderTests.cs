using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace OrchardCore.Modules;

public sealed class ApplicationStaticFileProviderTests : IDisposable
{
    private readonly string _applicationName = typeof(ApplicationStaticFileProviderTests).Assembly.GetName().Name;
    private readonly string _root = Directory.CreateTempSubdirectory("application-static-file-provider-tests").FullName;

    [Fact]
    public void GetFileInfo_ApplicationFile_DelegatesWithoutModulePrefix()
    {
        var expected = Mock.Of<IFileInfo>();
        var webRootFileProvider = new Mock<IFileProvider>();
        webRootFileProvider
            .Setup(provider => provider.GetFileInfo("assets/site.css"))
            .Returns(expected);

        var fileInfo = CreateProvider(webRootFileProvider.Object)
            .GetFileInfo($"/{_applicationName}/assets/site.css");

        Assert.Same(expected, fileInfo);
        webRootFileProvider.Verify(
            provider => provider.GetFileInfo("assets/site.css"),
            Times.Once);
    }

    [Fact]
    public void GetFileInfo_DifferentModule_ReturnsNotFound()
    {
        var webRootFileProvider = new Mock<IFileProvider>();

        var fileInfo = CreateProvider(webRootFileProvider.Object)
            .GetFileInfo("/Different.Module/assets/site.css");

        Assert.False(fileInfo.Exists);
        webRootFileProvider.Verify(
            provider => provider.GetFileInfo(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void GetFileInfo_CustomWebRoot_ReturnsFile()
    {
        var webRoot = Directory.CreateDirectory(Path.Combine(_root, "custom-web-root"));
        var filePath = Path.Combine(webRoot.FullName, "test.txt");
        File.WriteAllText(filePath, "content");

        using var webRootFileProvider = new PhysicalFileProvider(webRoot.FullName);
        var fileInfo = CreateProvider(webRootFileProvider)
            .GetFileInfo($"/{_applicationName}/test.txt");

        Assert.True(fileInfo.Exists);
        Assert.Equal(filePath, fileInfo.PhysicalPath);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("..\\outside.txt")]
    public void GetFileInfo_PathOutsideWebRoot_ReturnsNotFound(string subpath)
    {
        var webRoot = Directory.CreateDirectory(Path.Combine(_root, Module.WebRootPath));
        File.WriteAllText(Path.Combine(_root, "outside.txt"), "sensitive content");

        using var webRootFileProvider = new PhysicalFileProvider(webRoot.FullName);
        var fileInfo = CreateProvider(webRootFileProvider)
            .GetFileInfo($"/{_applicationName}/{subpath}");

        Assert.False(fileInfo.Exists);
    }

    public void Dispose()
    {
        Directory.Delete(_root, true);
        GC.SuppressFinalize(this);
    }

    private ApplicationStaticFileProvider CreateProvider(IFileProvider webRootFileProvider)
    {
        var environment = Mock.Of<IWebHostEnvironment>(e =>
            e.ApplicationName == _applicationName &&
            e.ContentRootPath == _root &&
            e.WebRootFileProvider == webRootFileProvider);

        var application = new Application(environment, [new Module(_applicationName)]);
        var applicationContext = Mock.Of<IApplicationContext>(c => c.Application == application);

        return new ApplicationStaticFileProvider(applicationContext, environment);
    }
}
