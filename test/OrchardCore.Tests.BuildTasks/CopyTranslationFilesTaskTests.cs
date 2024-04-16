namespace OrchardCore.Build.Tasks.Tests;

public class CopyTranslationFilesTaskTests
{
    private const string LocalizationFolder = "Localization";

    private readonly Mock<IBuildEngine> _buildEngine;
    private readonly List<BuildErrorEventArgs> _errors = [];

    public CopyTranslationFilesTaskTests()
    {
        _buildEngine = new Mock<IBuildEngine>();
        _buildEngine.Setup(be => be.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
            .Callback<BuildErrorEventArgs>(e => _errors.Add(e));
    }

    [Theory]
    [InlineData("ar.po")]
    [InlineData("fr.po")]
    public void ShouldCopyTranslationFilesToLocalizationFolder(string file)
    {
        // Arrange
        var filePath = Path.Combine(LocalizationFolder, file);
        var task = new CopyTranslationFilesTask
        {
            BuildEngine = _buildEngine.Object,
            SourceFilePath = file,
            DestinationFolderPath = LocalizationFolder,
        };

        TryCreateLocalizationFolder();

        // Act
        var result = task.Execute();

        // Assert
        Assert.True(result);
        Assert.Empty(_errors);
        Assert.True(File.Exists(filePath));
    }

    private static void TryCreateLocalizationFolder()
    {
        if (Directory.Exists(LocalizationFolder))
        {
            Directory.Delete(LocalizationFolder, recursive: true);
        }

        Directory.CreateDirectory(LocalizationFolder);
    }
}
