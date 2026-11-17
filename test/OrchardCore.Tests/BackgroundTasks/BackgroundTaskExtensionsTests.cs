using OrchardCore.BackgroundTasks;

namespace OrchardCore.Tests.BackgroundTasks;

public class BackgroundTaskExtensionsTests
{
    [Theory]
    [InlineData("ChunkFileUploadBackgroundTask", "Chunk File Upload")]
    [InlineData("IndexingTask", "Indexing")]
    [InlineData("OpenIdCleaner", "Open Id Cleaner")]
    [InlineData("HTTPCacheBackgroundTask", "HTTP Cache")]
    [InlineData("Purge2FATokensTask", "Purge2 FA Tokens")]
    [InlineData("BackgroundTask", "Background")]
    [InlineData("Task", "Task")]
    public void GetTitleFromTypeName_TypeName_ReturnsReadableTitle(string typeName, string expectedTitle)
    {
        Assert.Equal(expectedTitle, BackgroundTaskExtensions.GetTitleFromTypeName(typeName));
    }

    [Fact]
    public void GetDefaultSettings_AttributeWithoutTitle_UsesReadableTypeName()
    {
        var settings = new UntitledSampleBackgroundTask().GetDefaultSettings();

        Assert.Equal("Untitled Sample", settings.Title);
        Assert.Equal(typeof(UntitledSampleBackgroundTask).FullName, settings.Name);
        Assert.Equal("0 0 * * *", settings.Schedule);
    }

    [Fact]
    public void GetDefaultSettings_AttributeWithTitle_UsesTitle()
    {
        var settings = new TitledSampleBackgroundTask().GetDefaultSettings();

        Assert.Equal("Sample Title", settings.Title);
    }

    [Fact]
    public void GetDefaultSettings_NoAttribute_UsesReadableTypeName()
    {
        var settings = new NoAttributeSampleTask().GetDefaultSettings();

        Assert.Equal("No Attribute Sample", settings.Title);
        Assert.Equal(typeof(NoAttributeSampleTask).FullName, settings.Name);
    }

    [BackgroundTask(Schedule = "0 0 * * *")]
    private sealed class UntitledSampleBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [BackgroundTask(Title = "Sample Title")]
    private sealed class TitledSampleBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoAttributeSampleTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
