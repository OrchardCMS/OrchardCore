namespace OrchardCore.Build.Tasks.Tests;

public class CreateWebRootFolderTaskTests
{
    private const string WebRootFolder = "wwwroot";

    private static readonly string _placeholderFile = $"{WebRootFolder}\\.placeholder";

    private readonly Mock<IBuildEngine> _buildEngine;
    private readonly List<BuildErrorEventArgs> _errors = [];

    public CreateWebRootFolderTaskTests()
    {
        _buildEngine = new Mock<IBuildEngine>();
        _buildEngine.Setup(be => be.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(e => _errors.Add(e));

        TryDeleteLocalizationFolder();
    }

    [Fact]
    public void ShouldCreateLocalizationFolder()
    {
        // Arrange
        var task = new CreateWebRootFolderTask
        {
            BuildEngine = _buildEngine.Object
        };

        // Act
        var result = task.Execute();

        // Assert
        Assert.True(result);
        Assert.Empty(_errors);
        Assert.True(Directory.Exists(WebRootFolder));
        Assert.True(File.Exists(_placeholderFile));
    }

    [Fact]
    public void ShouldNotCreateLocalizationFolderIfExists()
    {
        // Arrange
        var task = new CreateWebRootFolderTask
        {
            BuildEngine = _buildEngine.Object
        };
        Directory.CreateDirectory(WebRootFolder);

        var createdTime1 = new DirectoryInfo(_placeholderFile).CreationTimeUtc;

        // Act & Assert
        task.Execute();
        Assert.True(Directory.Exists(WebRootFolder));

        var createdTime2 = new DirectoryInfo(_placeholderFile).CreationTimeUtc;
        Assert.Equal(createdTime1, createdTime2);
    }

    [Fact]
    public void IgnorePlaceholderFileIfExists()
    {
        // Arrange
        var task = new CreateWebRootFolderTask
        {
            BuildEngine = _buildEngine.Object
        };

        // Act & Assert
        task.Execute();

        var createdTime1 = new FileInfo(_placeholderFile).CreationTimeUtc;
        task.Execute();

        var createdTime2 = new FileInfo(_placeholderFile).CreationTimeUtc;
        Assert.Equal(createdTime1, createdTime2);
    }

    private static void TryDeleteLocalizationFolder()
    {
        if (Directory.Exists(WebRootFolder))
        {
            Directory.Delete(WebRootFolder, true);
        }
    }
}
