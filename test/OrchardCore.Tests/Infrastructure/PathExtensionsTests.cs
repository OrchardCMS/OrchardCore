namespace OrchardCore.Infrastructure.Tests;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("../naive/path/traversal", true)]
    [InlineData("more/elaborate/../../../path/traversal", true)]
    [InlineData("safe-location", false)]
    [InlineData("safe/location/", false)]
    [InlineData("safe/location/with/../navigation/for/some/reason/..", false)]
    public void IsOutsideOfBasePath_ShouldDetectPathTraversal(string path, bool isOutside)
    {
        // This test should work on all operating systems, that use Windows or Linux style paths.
        
        // Arrange
        var basePath = PathExtensions.GetTempFileName();
        
        // Act
        var absolutePath = PathExtensions.ResolvePath(PathExtensions.Combine(basePath, path));

        // Assert
        Assert.Equal(isOutside, PathExtensions.IsOutsideOfBasePath(basePath, absolutePath));
    }
    
    [Fact]
    public void IsOutsideOfBasePath_Linux_ShouldDetectPathTraversal()
    {
        // This test is only for Linux.
        if (!OperatingSystem.IsLinux())
        {
            return;
        }
        
        // Arrange
        var basePath = PathExtensions.GetTempFileName();
        
        // Act
        var absolutePath = PathExtensions.ResolvePath(PathExtensions.Combine(basePath, "/root/.config/my-secret"));

        // Assert
        Assert.True(PathExtensions.IsOutsideOfBasePath(basePath, absolutePath));
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

        // Assert
        Assert.True(PathExtensions.IsOutsideOfBasePath(basePath, absolutePath));
    }
}
