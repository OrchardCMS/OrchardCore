using OrchardCore.DataOrchestrator.Services;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class EtlLocalFilePathResolverTests
{
    [Theory]
    [InlineData("Imports/products.xlsx")]
    [InlineData("/Imports/products.xlsx")]
    [InlineData("Uploads\\products.xlsx")]
    public void IsValidRelativeFilePath_AllowsTenantRelativePaths(string filePath)
    {
        Assert.True(EtlLocalFilePathResolver.IsValidRelativeFilePath(filePath));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../products.xlsx")]
    [InlineData("Imports/../products.xlsx")]
    [InlineData("C:\\Temp\\products.xlsx")]
    [InlineData("\\\\server\\share\\products.xlsx")]
    public void IsValidRelativeFilePath_RejectsUnsafePaths(string filePath)
    {
        Assert.False(EtlLocalFilePathResolver.IsValidRelativeFilePath(filePath));
    }

    [Fact]
    public void ResolveFilePath_KeepsPathUnderTenantBase()
    {
        var basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"), "App_Data", "Sites", "Default", "DataOrchestrator");
        var fullPath = EtlLocalFilePathResolver.ResolveFilePath(basePath, "Uploads/products.xlsx");

        Assert.StartsWith(Path.GetFullPath(basePath), fullPath, StringComparison.Ordinal);
        Assert.EndsWith(Path.Combine("Uploads", "products.xlsx"), fullPath, StringComparison.Ordinal);
    }

    [Fact]
    public void GetFilesBasePath_UsesTenantAppDataFolder()
    {
        var appDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"), "App_Data");
        var shellOptions = new ShellOptions
        {
            ShellsApplicationDataPath = appDataPath,
        };
        var shellSettings = new ShellSettings
        {
            Name = "Default",
        };

        var basePath = EtlLocalFilePathResolver.GetFilesBasePath(shellOptions, shellSettings);

        Assert.Equal(
            Path.GetFullPath(Path.Combine(appDataPath, "Sites", "Default", "DataOrchestrator")),
            basePath);
    }
}
