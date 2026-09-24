using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using MediaStartup = OrchardCore.Media.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaPathTests
{
    public static TheoryData<string> InvalidAssetsPaths => new()
    {
        null,
        "",
        " ",
        ".",
        "..",
        "/",
        "/application",
        @"\application",
        @"\\server\share",
        "C:/application",
        @"C:\application",
        "C:application",
        "../../../",
        "../OtherTenant/Media",
        "Media/../../../../",
        @"Media\..\..\..\..",
        "Media/../Media2",
        "Media/./Images",
        "Media//Images",
        "Media/.. /Images",
        "Media/.../Images",
        "Media /Images",
        "Media\0",
    };

    [Theory]
    [MemberData(nameof(InvalidAssetsPaths))]
    public void Validate_InvalidAssetsPath_Fails(string assetsPath)
    {
        var options = new MediaOptions
        {
            AssetsPath = assetsPath,
            AllowedFileExtensions = [],
            RestrictedFileExtensions = [],
        };

        var result = new MediaOptionsValidator().Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains("AssetsPath", result.FailureMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidAssetsPaths))]
    public void GetMediaPath_InvalidAssetsPath_Throws(string assetsPath)
    {
        using var settings = new ShellSettings { Name = "Tenant" };

        Assert.Throws<ArgumentException>(() => MediaStartup.GetMediaPath(CreateShellOptions(), settings, assetsPath));
    }

    [Theory]
    [InlineData("Media", "Media")]
    [InlineData("Media/", "Media")]
    [InlineData(@"Media\", "Media")]
    [InlineData("Assets/Media", "Assets/Media")]
    [InlineData(@"Assets\Media", "Assets/Media")]
    [InlineData("My Assets/Media", "My Assets/Media")]
    public void GetMediaPath_ValidAssetsPath_ResolvesInsideEachTenant(string assetsPath, string relativePath)
    {
        var shellOptions = CreateShellOptions();
        var options = new MediaOptions
        {
            AssetsPath = assetsPath,
            AllowedFileExtensions = [],
            RestrictedFileExtensions = [],
        };

        Assert.True(new MediaOptionsValidator().Validate(Options.DefaultName, options).Succeeded);

        foreach (var tenant in new[] { "Default", "OtherTenant" })
        {
            using var settings = new ShellSettings { Name = tenant };
            var expected = Path.GetFullPath(Path.Combine(
                shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, tenant, relativePath));

            Assert.Equal(expected, MediaStartup.GetMediaPath(shellOptions, settings, assetsPath));
        }
    }

    public static TheoryData<string> InvalidTenantNames => new()
    {
        "",
        ".",
        "..",
        "../..",
        "../OtherTenant",
        "Tenant/../..",
        "/",
        "/Tenant",
    };

    [Theory]
    [MemberData(nameof(InvalidTenantNames))]
    public void GetMediaPath_TenantNameOutsideTenantsDirectory_Throws(string tenantName)
    {
        using var settings = new ShellSettings { Name = tenantName };

        Assert.Throws<ArgumentException>(() => MediaStartup.GetMediaPath(CreateShellOptions(), settings, "Media"));
    }

    [Fact]
    public void GetMediaPath_TenantNameEscapingToTheApplicationDirectory_Throws()
    {
        // A tenant name that is not validated at creation time must not be able to redirect the tenant
        // media root to the application's assembly directory.
        using var settings = new ShellSettings { Name = "../.." };

        Assert.Throws<ArgumentException>(() => MediaStartup.GetMediaPath(CreateShellOptions(), settings, "bin/Release/net10.0"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResolveMediaServices_InvalidConfiguredRoot_FailsOptionsValidation(bool resolveFileProvider)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore_Media:AssetsPath"] = "../../../../",
                ["OrchardCore_Media:AllowedFileExtensions:0"] = ".dll",
            })
            .Build();
        var shellConfiguration = new Mock<IShellConfiguration>();
        shellConfiguration.Setup(c => c.GetSection("OrchardCore_Media"))
            .Returns(configuration.GetSection("OrchardCore_Media"));

        var services = new ServiceCollection();
        services.AddSingleton(shellConfiguration.Object);
        using var settings = new ShellSettings { Name = "Tenant" };
        services.AddSingleton(settings);
        services.Configure<ShellOptions>(options =>
        {
            options.ShellsApplicationDataPath = CreateShellOptions().ShellsApplicationDataPath;
            options.ShellsContainerName = "Sites";
        });
        new MediaStartup().ConfigureServices(services);

        using var provider = services.BuildServiceProvider();
        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService(resolveFileProvider ? typeof(IMediaFileProvider) : typeof(IMediaFileStore)));

        Assert.Contains("AssetsPath", exception.Message);
    }

    private static ShellOptions CreateShellOptions() => new()
    {
        ShellsApplicationDataPath = Path.Combine(Path.GetTempPath(), "media-path-tests", "App_Data"),
        ShellsContainerName = "Sites",
    };
}
