using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace OrchardCore.Modules;

public sealed class ModuleProjectStaticFileProviderTests : IDisposable
{
    private const string ModuleName = "Example.Module";

    private readonly string _root = Directory.CreateTempSubdirectory("module-project-static-file-provider-tests").FullName;
    private readonly string _webRoot;

    public ModuleProjectStaticFileProviderTests()
    {
        _webRoot = Directory.CreateDirectory(Path.Combine(_root, Module.WebRootPath)).FullName;

        File.WriteAllText(Path.Combine(_webRoot, "test.txt"), "content");
        File.WriteAllText(Path.Combine(_root, "outside.txt"), "sensitive content");

        // A sibling folder whose name starts with the module project "wwwroot" folder name.
        var sibling = Directory.CreateDirectory(Path.Combine(_root, Module.WebRootPath + "-other")).FullName;
        File.WriteAllText(Path.Combine(sibling, "sibling.txt"), "sensitive content");
    }

    [Fact]
    public void GetFileInfo_FileUnderWebRoot_ReturnsFile()
    {
        var fileInfo = CreateProvider().GetFileInfo($"/{ModuleName}/test.txt");

        Assert.True(fileInfo.Exists);
        Assert.Equal(Path.Combine(_webRoot, "test.txt"), fileInfo.PhysicalPath);
    }

    [Fact]
    public void GetFileInfo_UnknownModule_ReturnsNotFound()
    {
        var fileInfo = CreateProvider().GetFileInfo("/Different.Module/test.txt");

        Assert.False(fileInfo.Exists);
    }

    [Theory]
    [MemberData(nameof(PathsOutsideWebRoot))]
    public void GetFileInfo_PathOutsideWebRoot_ReturnsNotFound(string subpath)
    {
        var fileInfo = CreateProvider().GetFileInfo($"/{ModuleName}/{subpath}");

        Assert.False(fileInfo.Exists);
    }

    [Fact]
    public void Watch_FileUnderWebRoot_ReturnsChangeToken()
    {
        var changeToken = CreateProvider().Watch($"/{ModuleName}/test.txt");

        Assert.NotSame(NullChangeToken.Singleton, changeToken);
    }

    [Theory]
    [MemberData(nameof(PathsOutsideWebRoot))]
    public void Watch_PathOutsideWebRoot_ReturnsNullChangeToken(string filter)
    {
        var changeToken = CreateProvider().Watch($"/{ModuleName}/{filter}");

        Assert.Same(NullChangeToken.Singleton, changeToken);
    }

    public static TheoryData<string> PathsOutsideWebRoot => new()
    {
        "../outside.txt",
        @"..\outside.txt",
        "./../outside.txt",
        "sub/../../outside.txt",
        // A sibling folder must not be reachable through the "wwwroot" prefix.
        $"../{Module.WebRootPath}-other/sibling.txt",
        // A rooted path must not replace the module project "wwwroot" folder.
        "C:/Windows/win.ini",
        "/etc/passwd",
    };

    public void Dispose()
    {
        SetRoots(null);
        Directory.Delete(_root, true);
        GC.SuppressFinalize(this);
    }

    private ModuleProjectStaticFileProvider CreateProvider()
    {
        // The module project roots are resolved once from the assets of the loaded module assemblies,
        // which a unit test can't produce, so seed the resolved roots directly.
        SetRoots(new Dictionary<string, string>
        {
            [ModuleName] = _webRoot + Path.DirectorySeparatorChar,
        });

        return new ModuleProjectStaticFileProvider(Mock.Of<IApplicationContext>());
    }

    private static void SetRoots(Dictionary<string, string> roots)
        => typeof(ModuleProjectStaticFileProvider)
            .GetField("s_roots", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, roots);
}
