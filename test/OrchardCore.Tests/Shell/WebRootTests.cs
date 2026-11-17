#nullable enable

using Microsoft.AspNetCore.Hosting;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Shell;

public sealed class WebRootTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("web-root-tests").FullName;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetWebRootPathOrDefault_MissingWebRoot_ReturnsWwwRootUnderContentRoot(string? webRootPath)
    {
        var environment = CreateEnvironment(webRootPath);

        Assert.Equal(Path.Combine(_root, "wwwroot"), environment.GetWebRootPathOrDefault());
    }

    [Fact]
    public void GetWebRootPathOrDefault_ConfiguredWebRoot_ReturnsWebRoot()
    {
        var webRootPath = Path.Combine(_root, "static");
        var environment = CreateEnvironment(webRootPath);

        Assert.Equal(webRootPath, environment.GetWebRootPathOrDefault());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("static")]
    public async Task RemovingAsync_MissingOrCustomWebRoot_DeletesTenantFolder(string? webRoot)
    {
        var webRootPath = string.IsNullOrEmpty(webRoot) ? webRoot : Path.Combine(_root, webRoot);
        var tenantFolder = Path.Combine(_root, string.IsNullOrEmpty(webRoot) ? "wwwroot" : webRoot, "Tenant1");
        Directory.CreateDirectory(Path.Combine(tenantFolder, "media-cache"));
        await File.WriteAllTextAsync(
            Path.Combine(tenantFolder, "media-cache", "image.png"), "cached image", TestContext.Current.CancellationToken);

        var handler = new ShellWebRootRemovingHandler(
            CreateEnvironment(webRootPath),
            Mock.Of<IStringLocalizer<ShellWebRootRemovingHandler>>(),
            NullLogger<ShellWebRootRemovingHandler>.Instance);
        using var shellSettings = new ShellSettings { Name = "Tenant1", State = TenantState.Disabled };
        var context = new ShellRemovingContext { ShellSettings = shellSettings };

        await handler.RemovingAsync(context);

        Assert.True(context.Success, context.ErrorMessage);
        Assert.False(Directory.Exists(tenantFolder));
    }

    public void Dispose()
    {
        Directory.Delete(_root, recursive: true);
    }

    private IWebHostEnvironment CreateEnvironment(string? webRootPath)
        => Mock.Of<IWebHostEnvironment>(env => env.ContentRootPath == _root && env.WebRootPath == webRootPath);
}
