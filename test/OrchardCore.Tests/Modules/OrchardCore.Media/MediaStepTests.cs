using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Media;
using OrchardCore.Media.Core;
using OrchardCore.Media.Recipes;
using OrchardCore.Recipes.Models;
using MediaStartup = OrchardCore.Media.Startup;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaStepTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../Application.dll")]
    [InlineData(@"..\Application.dll")]
    [InlineData("images/../../Application.dll")]
    [InlineData(@"images\..\..\Application.dll")]
    [InlineData("/Application.dll")]
    [InlineData(@"\Application.dll")]
    [InlineData("C:/Application.dll")]
    [InlineData(@"C:\Application.dll")]
    [InlineData("C:Application.dll")]
    [InlineData(@"\\server\share\Application.dll")]
    [InlineData("images/../Application.dll")]
    [InlineData("images/./Application.dll")]
    [InlineData("images//Application.dll")]
    [InlineData("images/.. /Application.dll")]
    [InlineData("images/.../Application.dll")]
    [InlineData("images /Application.dll")]
    [InlineData("images/Application.dll ")]
    [InlineData("images/Application.dll.")]
    [InlineData("images/Application.dll/")]
    [InlineData("images/\0Application.dll")]
    public async Task ExecuteAsync_InvalidTarget_RejectsBeforeReadingAnySource(string targetPath)
    {
        foreach (var source in new[] { "Base64", "SourcePath", "SourceUrl" })
        {
            var store = new Mock<IMediaFileStore>(MockBehavior.Strict);
            var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            var fileProvider = new Mock<IFileProvider>(MockBehavior.Strict);
            var step = CreateStep(store.Object, httpClientFactory.Object);
            var context = CreateContext(targetPath, source, "must not be read");
            context.RecipeDescriptor = new RecipeDescriptor { FileProvider = fileProvider.Object };

            await step.ExecuteAsync(context);

            Assert.Contains("relative file path", Assert.Single(context.Errors));
            store.VerifyNoOtherCalls();
            httpClientFactory.VerifyNoOtherCalls();
            fileProvider.VerifyNoOtherCalls();
        }
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPathAlias_RejectsBeforeWriting()
    {
        var store = new Mock<IMediaFileStore>(MockBehavior.Strict);
        var context = CreateContext("../Application.dll", "Base64", "must not be read", usePathAlias: true);

        await CreateStep(store.Object).ExecuteAsync(context);

        Assert.Contains("relative file path", Assert.Single(context.Errors));
        store.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("image.JPG")]
    [InlineData("images/image.jpg")]
    [InlineData(@"images\image.jpg")]
    [InlineData("styles/site.css")]
    [InlineData("scripts/site.js")]
    [InlineData("images/icon.SVG")]
    public async Task ExecuteAsync_AllowedRelativeTarget_WritesToMediaStore(string targetPath)
    {
        var bytes = "media content"u8.ToArray();
        var store = new Mock<IMediaFileStore>(MockBehavior.Strict);
        store.Setup(s => s.CreateFileFromStreamAsync(targetPath, It.IsAny<Stream>(), true))
            .Callback<string, Stream, bool>((_, stream, _) =>
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                Assert.Equal(bytes, copy.ToArray());
            })
            .ReturnsAsync(targetPath);
        var context = CreateContext(targetPath, "Base64", Convert.ToBase64String(bytes));

        await CreateStep(store.Object).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        store.Verify(s => s.CreateFileFromStreamAsync(targetPath, It.IsAny<Stream>(), true), Times.Once);
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_UnconfiguredExtension_RejectsBeforeReadingSource()
    {
        var store = new Mock<IMediaFileStore>(MockBehavior.Strict);
        var context = CreateContext("application.exe", "Base64", "must not be read");

        await CreateStep(store.Object).ExecuteAsync(context);

        Assert.Contains("File extension not allowed", Assert.Single(context.Errors));
        store.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_AssemblyExtensionAllowed_OnlyOverwritesTenantMedia()
    {
        var root = Directory.CreateTempSubdirectory("media-step-tests").FullName;

        try
        {
            var applicationAssembly = Path.Combine(root, "Application.dll");
            await File.WriteAllTextAsync(applicationAssembly, "application assembly", TestContext.Current.CancellationToken);
            using var settings = new ShellSettings { Name = "Tenant" };
            var mediaRoot = MediaStartup.GetMediaPath(new ShellOptions
            {
                ShellsApplicationDataPath = Path.Combine(root, "App_Data"),
                ShellsContainerName = "Sites",
            }, settings, "Media");
            var siblingDirectory = Directory.CreateDirectory(mediaRoot + "-other").FullName;
            var siblingAssembly = Path.Combine(siblingDirectory, "Application.dll");
            await File.WriteAllTextAsync(siblingAssembly, "other directory", TestContext.Current.CancellationToken);
            Directory.CreateDirectory(mediaRoot);
            var mediaFile = Path.Combine(mediaRoot, "Application.dll");
            await File.WriteAllTextAsync(mediaFile, "old media", TestContext.Current.CancellationToken);

            var store = new DefaultMediaFileStore(
                new FileSystemStore(mediaRoot, NullLogger<FileSystemStore>.Instance),
                "/media", "", [], [],
                NullLogger<DefaultMediaFileStore>.Instance);
            var context = CreateContext("Application.dll", "Base64", Convert.ToBase64String("new media"u8));
            var files = (JsonArray)context.Step["Files"];
            foreach (var target in new[] { "../../../../Application.dll", "../Media-other/Application.dll", applicationAssembly })
            {
                files.Insert(0, new JsonObject
                {
                    ["TargetPath"] = target,
                    ["Base64"] = Convert.ToBase64String("replacement"u8),
                });
            }

            await CreateStep(store).ExecuteAsync(context);

            Assert.Equal(3, context.Errors.Count);
            Assert.Equal("application assembly", await File.ReadAllTextAsync(applicationAssembly, TestContext.Current.CancellationToken));
            Assert.Equal("other directory", await File.ReadAllTextAsync(siblingAssembly, TestContext.Current.CancellationToken));
            Assert.Equal("new media", await File.ReadAllTextAsync(mediaFile, TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static MediaStep CreateStep(
        IMediaFileStore store,
        IHttpClientFactory httpClientFactory = null)
    {
        var localizer = new Mock<IStringLocalizer<MediaStep>>();
        localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, string.Format(key, args)));

        return new MediaStep(
            store,
            Options.Create(new MediaOptions
            {
                AssetsPath = "Media",
                AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".dll", ".css", ".js", ".svg" },
            }),
            httpClientFactory ?? Mock.Of<IHttpClientFactory>(),
            localizer.Object);
    }

    private static RecipeExecutionContext CreateContext(string targetPath, string source, string value, bool usePathAlias = false) => new()
    {
        Name = "media",
        Step = new JsonObject
        {
            ["Files"] = new JsonArray(new JsonObject
            {
                [usePathAlias ? "Path" : "TargetPath"] = targetPath,
                [source] = value,
            }),
        },
    };
}
