namespace OrchardCore.Infrastructure.Tests;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("safe-location")]
    [InlineData("safe/location/")]
    [InlineData("safe/location/with/../navigation/for/some/reason/..")]
    public void IsOutsideOfBasePath_ValidateSafePath(string path)
    {
        // This test should work on all operating systems, that use Windows or Linux style paths.
        
        // Arrange
        var basePath = PathExtensions.GetTempFileName();
        
        // Act
        var absolutePath = PathExtensions.ResolvePath(PathExtensions.Combine(basePath, path));
        var windowsAbsolutePath = OperatingSystem.IsWindows()
            ? PathExtensions.ResolvePath(PathExtensions.Combine(basePath, path.Replace('/', '\\')))
            : path;
        var isOutside = PathExtensions.IsOutsideOfBasePath(basePath, absolutePath);
        var windowsIsOutside = PathExtensions.IsOutsideOfBasePath(basePath, windowsAbsolutePath);

        // Assert
        Assert.False(isOutside, absolutePath);
        Assert.False(windowsIsOutside, absolutePath);
    }
    
    [Theory]
    [InlineData("/root/.config/my-secret")]
    [InlineData("../naive/path/traversal")]
    [InlineData("more/elaborate/../../../path/traversal")]
    public void IsOutsideOfBasePath_Linux_ShouldDetectPathTraversal(string path)
    {
        // This test is only for Linux.
        if (!OperatingSystem.IsLinux())
        {
            return;
        }
        
        // Arrange
        var basePath = PathExtensions.GetTempFileName();
        
        // Act
        var absolutePath = PathExtensions.ResolvePath(PathExtensions.Combine(basePath, path));
        var isOutside = PathExtensions.IsOutsideOfBasePath(basePath, absolutePath);

        // Assert
        Assert.True(isOutside, absolutePath);
    }
    
    [Theory]
    [InlineData("C:\\windows\\system32\\drivers\\etc\\hosts")]
    [InlineData("..\\naive\\path\\traversal")]
    [InlineData("more\\elaborate\\..\\..\\..\\path\\traversal")]
    public void IsOutsideOfBasePath_Windows_ShouldDetectPathTraversal(string path)
    {
        // This test is only for Windows paths.
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
        
        // Arrange
        var basePath = PathExtensions.GetTempFileName();
        
        // Act
        var absolutePath = PathExtensions.ResolvePath(PathExtensions.Combine(basePath, path));
        var isOutside = PathExtensions.IsOutsideOfBasePath(basePath, absolutePath);

        // Assert
        Assert.True(isOutside, absolutePath);
    }
}
