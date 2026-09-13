using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace OrchardCore.Modules;

public class ModuleEmbeddedStaticFileProviderTests : IDisposable
{
    // The application module id is the application assembly name; using this test assembly
    // lets the Application/Module constructors resolve a real, loadable assembly.
    private static readonly string ApplicationName = typeof(ModuleEmbeddedStaticFileProviderTests).Assembly.GetName().Name;

    private readonly string _contentRoot;
    private readonly ModuleEmbeddedStaticFileProvider _provider;

    public ModuleEmbeddedStaticFileProviderTests()
    {
        _contentRoot = Path.Combine(Path.GetTempPath(), "oc-static-traversal-" + Guid.NewGuid().ToString("n"));

        // A public asset served from the application 'wwwroot'.
        Directory.CreateDirectory(Path.Combine(_contentRoot, "wwwroot"));
        File.WriteAllText(Path.Combine(_contentRoot, "wwwroot", "public.txt"), "public");

        // A secret sibling of 'wwwroot' that must never be reachable, mirroring how the tenant
        // Data Protection keys live under 'App_Data' next to (not inside) 'wwwroot'.
        Directory.CreateDirectory(Path.Combine(_contentRoot, "App_Data"));
        File.WriteAllText(Path.Combine(_contentRoot, "App_Data", "secret.txt"), "SECRET");

        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(e => e.ApplicationName).Returns(ApplicationName);
        environment.SetupGet(e => e.ContentRootPath).Returns(_contentRoot);

        var application = new Application(environment.Object, [new Module(ApplicationName, isApplication: true)]);

        var applicationContext = new Mock<IApplicationContext>();
        applicationContext.SetupGet(c => c.Application).Returns(application);

        _provider = new ModuleEmbeddedStaticFileProvider(applicationContext.Object);
    }

    [Fact]
    public void GetFileInfo_ServesApplicationStaticFile()
    {
        var fileInfo = _provider.GetFileInfo($"/{ApplicationName}/public.txt");

        Assert.True(fileInfo.Exists);
        using var stream = fileInfo.CreateReadStream();
        using var reader = new StreamReader(stream);
        Assert.Equal("public", reader.ReadToEnd());
    }

    [Theory]
    // Encoded backslashes ('%5c') survive the server request path normalization and are only
    // turned into '..' traversal while the provider resolves the physical path.
    [InlineData("/{0}/..\\App_Data\\secret.txt")]
    [InlineData("/{0}/../App_Data/secret.txt")]
    [InlineData("/{0}/..\\..\\App_Data\\secret.txt")]
    [InlineData("/{0}/subfolder\\..\\..\\App_Data\\secret.txt")]
    public void GetFileInfo_DoesNotServeFilesOutsideWebRoot(string template)
    {
        var subpath = string.Format(template, ApplicationName);

        var fileInfo = _provider.GetFileInfo(subpath);

        Assert.IsType<NotFoundFileInfo>(fileInfo);
        Assert.False(fileInfo.Exists);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_contentRoot, recursive: true);
        }
        catch (DirectoryNotFoundException)
        {
        }

        GC.SuppressFinalize(this);
    }
}
