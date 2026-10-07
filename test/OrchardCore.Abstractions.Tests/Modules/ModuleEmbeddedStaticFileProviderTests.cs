using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace OrchardCore.Modules;

public sealed class ModuleEmbeddedStaticFileProviderTests : IDisposable
{
    private readonly string _applicationName = typeof(ModuleEmbeddedStaticFileProviderTests).Assembly.GetName().Name;
    private readonly string _root = Directory.CreateTempSubdirectory("module-static-file-provider-tests").FullName;

    [Fact]
    public void GetFileInfo_ApplicationPhysicalFile_ReturnsNotFound()
    {
        var webRoot = Directory.CreateDirectory(Path.Combine(_root, Module.WebRootPath));
        var filePath = Path.Combine(webRoot.FullName, "test.txt");
        File.WriteAllText(filePath, "content");

        var fileInfo = CreateProvider().GetFileInfo($"/{_applicationName}/test.txt");

        Assert.False(fileInfo.Exists);
    }

    public void Dispose()
    {
        Directory.Delete(_root, true);
        GC.SuppressFinalize(this);
    }

    private ModuleEmbeddedStaticFileProvider CreateProvider()
    {
        var environment = Mock.Of<IHostEnvironment>(e =>
            e.ApplicationName == _applicationName &&
            e.ContentRootPath == _root);

        var application = new Application(environment, [new Module(_applicationName)]);
        var applicationContext = Mock.Of<IApplicationContext>(c => c.Application == application);

        return new ModuleEmbeddedStaticFileProvider(applicationContext);
    }
}
