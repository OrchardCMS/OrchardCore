using System.Reflection;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Modules;
using OrchardCore.Mvc;

namespace OrchardCore.Tests.Mvc;

/// <summary>
/// The razor file providers map a virtual module path onto a physical project folder while in a
/// development environment. A '..' segment, or a path alias of the current platform, must not be
/// able to resolve a file outside of that folder.
/// </summary>
public class RazorFileProviderPathContainmentTests : IDisposable
{
    private const string ModuleName = "Example.Module";

    private readonly string _applicationName = typeof(RazorFileProviderPathContainmentTests).Assembly.GetName().Name;
    private readonly string _root = Directory.CreateTempSubdirectory("razor-file-provider-tests").FullName;
    private readonly string _projectRoot;

    public RazorFileProviderPathContainmentTests()
    {
        _projectRoot = Directory.CreateDirectory(Path.Combine(_root, "project")).FullName;

        Directory.CreateDirectory(Path.Combine(_projectRoot, "Views"));
        Directory.CreateDirectory(Path.Combine(_projectRoot, "Pages"));

        File.WriteAllText(Path.Combine(_projectRoot, "Views", "Index.cshtml"), "@* content *@");
        File.WriteAllText(Path.Combine(_root, "outside.cshtml"), "@* sensitive content *@");

        Directory.CreateDirectory(Path.Combine(_root, "outside"));
    }

    public static TheoryData<string> EscapingSubPaths => new()
    {
        "../outside.cshtml",
        @"..\outside.cshtml",
        "Views/../../outside.cshtml",
        @"Views\..\..\outside.cshtml",
    };

    [Fact]
    public void ModuleProjectRazorFileProvider_GetFileInfo_FileInProject_ReturnsFile()
    {
        var fileInfo = CreateModuleProvider().GetFileInfo($"/{Application.ModulesRoot}{ModuleName}/Views/Index.cshtml");

        Assert.True(fileInfo.Exists);
        Assert.Equal(Path.Combine(_projectRoot, "Views", "Index.cshtml"), fileInfo.PhysicalPath);
    }

    [Theory]
    [MemberData(nameof(EscapingSubPaths))]
    public void ModuleProjectRazorFileProvider_GetFileInfo_PathOutsideProject_ReturnsNotFound(string subpath)
    {
        var fileInfo = CreateModuleProvider().GetFileInfo($"/{Application.ModulesRoot}{ModuleName}/{subpath}");

        Assert.False(fileInfo.Exists);
    }

    [Theory]
    [MemberData(nameof(EscapingSubPaths))]
    public void ModuleProjectRazorFileProvider_Watch_PathOutsideProject_ReturnsNullChangeToken(string filter)
    {
        var changeToken = CreateModuleProvider().Watch($"/{Application.ModulesRoot}{ModuleName}/{filter}");

        Assert.Same(NullChangeToken.Singleton, changeToken);
    }

    [Theory]
    [InlineData("Pages/../../outside")]
    [InlineData(@"Pages\..\..\outside")]
    public void ModuleProjectRazorFileProvider_GetDirectoryContents_PathOutsideProject_ReturnsNotFound(string subpath)
    {
        var contents = CreateModuleProvider().GetDirectoryContents($"/{Application.ModulesRoot}{ModuleName}/{subpath}");

        Assert.False(contents.Exists);
    }

    [Theory]
    [MemberData(nameof(EscapingSubPaths))]
    public void ApplicationViewFileProvider_GetFileInfo_PathOutsideApplication_ReturnsNotFound(string subpath)
    {
        var fileInfo = CreateApplicationProvider().GetFileInfo($"/{Application.ModulesRoot}{_applicationName}/{subpath}");

        Assert.False(fileInfo.Exists);
    }

    [Theory]
    [MemberData(nameof(EscapingSubPaths))]
    public void ApplicationViewFileProvider_Watch_PathOutsideApplication_ReturnsNullChangeToken(string filter)
    {
        var changeToken = CreateApplicationProvider().Watch($"/{Application.ModulesRoot}{_applicationName}/{filter}");

        Assert.Same(NullChangeToken.Singleton, changeToken);
    }

    [Theory]
    [InlineData("Views/../../outside")]
    [InlineData(@"Views\..\..\outside")]
    public void ApplicationViewFileProvider_GetDirectoryContents_PathOutsideApplication_ReturnsNotFound(string subpath)
    {
        var contents = CreateApplicationProvider().GetDirectoryContents($"/{Application.ModulesRoot}{_applicationName}/{subpath}");

        Assert.False(contents.Exists);
    }

    public void Dispose()
    {
        SetModuleRoots(null);
        Directory.Delete(_root, true);
        GC.SuppressFinalize(this);
    }

    private ModuleProjectRazorFileProvider CreateModuleProvider()
    {
        // The module project roots are resolved once from the assets of the loaded module assemblies,
        // which a unit test can't produce, so seed the resolved roots directly.
        SetModuleRoots(new Dictionary<string, string>
        {
            [ModuleName] = _projectRoot + Path.DirectorySeparatorChar,
        });

        return new ModuleProjectRazorFileProvider(Mock.Of<IApplicationContext>());
    }

    private ApplicationViewFileProvider CreateApplicationProvider()
    {
        var environment = Mock.Of<IHostEnvironment>(e =>
            e.ApplicationName == _applicationName &&
            e.ContentRootPath == _projectRoot);

        var application = new Application(environment, [new global::OrchardCore.Modules.Module(_applicationName)]);

        return new ApplicationViewFileProvider(Mock.Of<IApplicationContext>(c => c.Application == application));
    }

    private static void SetModuleRoots(Dictionary<string, string> roots)
    {
        var type = typeof(ModuleProjectRazorFileProvider);

        type.GetField("s_pageFileProviders", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, roots is null ? null : new List<IFileProvider>());

        type.GetField("s_roots", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, roots);
    }
}
