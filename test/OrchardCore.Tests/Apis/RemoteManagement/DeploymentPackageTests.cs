using System.IO.Compression;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Deployment.Controllers;
using OrchardCore.Deployment.Services;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.FileStorage;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentPackageTests
{
    [Theory]
    [InlineData("accepted")]
    [InlineData("json")]
    [InlineData("json-invalid")]
    [InlineData("pipeline-denied")]
    [InlineData("invalid")]
    [InlineData("execution-failed")]
    [InlineData("permission-denied")]
    public async Task ExistingUploadAction_PreservesPipelineAndCleansStaging(string scenario)
    {
        await WithService(async (packages, root) =>
        {
            var authorization = new Mock<IAuthorizationService>();
            authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(scenario == "permission-denied" ? AuthorizationResult.Failed() : AuthorizationResult.Success());
            var handler = new Mock<IFileEventHandler>();
            handler.Setup(value => value.CreatingAsync(It.IsAny<FileCreatingContext>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FileCreatingContext _, Stream stream, CancellationToken _) => scenario == "pipeline-denied"
                    ? FileCreatingResult.Failed(stream) : FileCreatingResult.Success(stream));
            var manager = new Mock<IDeploymentManager>();
            manager.Setup(value => value.ImportDeploymentPackageAsync(It.IsAny<IFileProvider>()))
                .Returns(async (IFileProvider provider) =>
                {
                    using var reader = new StreamReader(provider.GetFileInfo("Recipe.json").CreateReadStream());
                    Assert.Equal("{\"steps\":[]}", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
                    if (scenario == "execution-failed") { throw new InvalidOperationException("Import failed"); }
                });
            var controller = new ImportController(manager.Object, authorization.Object, new FileCreationService([handler.Object]),
                packages, Mock.Of<INotifier>(), Mock.Of<ILogger<ImportController>>(), Mock.Of<IHtmlLocalizer<ImportController>>(),
                new StringLocalizer<ImportController>(new NullStringLocalizerFactory()))
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            };
            using var input = new MemoryStream(Encoding.UTF8.GetBytes(scenario is "invalid" or "json-invalid" ? "{\"steps\":[{}]}" : "{\"steps\":[]}"));
            var file = new FormFile(input, 0, input.Length, "importedPackage", "Recipe.json")
            {
                Headers = new HeaderDictionary(), ContentType = "application/json",
            };
            var result = scenario.StartsWith("json", StringComparison.Ordinal)
                ? await controller.Json(new ImportJsonViewModel { Json = scenario == "json" ? "{\"steps\":[]}" : "{\"steps\":[{}]}" })
                : await controller.Import(file);
            if (scenario == "permission-denied") { Assert.IsType<ForbidResult>(result); }
            else { Assert.IsType<RedirectToActionResult>(result); }
            manager.Verify(value => value.ImportDeploymentPackageAsync(It.IsAny<IFileProvider>()),
                scenario is "accepted" or "json" or "execution-failed" ? Times.Once() : Times.Never());
            handler.Verify(value => value.CreatingAsync(It.IsAny<FileCreatingContext>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
                scenario is "permission-denied" or "json" or "json-invalid" ? Times.Never() : Times.Once());
            Assert.Empty(Directory.GetFileSystemEntries(root));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValidatedOriginal_CanBePersistedWithoutRepackingAndIsRemovedOnDispose(bool zip)
    {
        await WithService(async (service, root) =>
        {
            using var input = zip ? Zip(("Recipe.json", "{\"steps\":[]}"), ("nested/file.txt", "payload"))
                : new MemoryStream(Encoding.UTF8.GetBytes("{\"steps\":[]}"));
            var expected = input.ToArray();
            var package = await service.StageAsync(input, zip ? "package.zip" : "Recipe.json", TestContext.Current.CancellationToken);
            using (var original = package.OpenRead())
            {
                using var copy = new MemoryStream();
                await original.CopyToAsync(copy, TestContext.Current.CancellationToken);
                Assert.Equal(expected, copy.ToArray());
            }
            package.Dispose();
            package.Dispose();
            Assert.Throws<ObjectDisposedException>(() => package.OpenRead());
            Assert.Empty(Directory.GetFileSystemEntries(root));
            Assert.True(input.CanRead);
        });
    }

    [Fact]
    public async Task CancelledStaging_CleansTemporaryFilesAndLeavesInputOwnedByCaller()
    {
        await WithService(async (service, root) =>
        {
            using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"steps\":[]}"));
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.StageAsync(input, "Recipe.json", cancellation.Token));
            Assert.Empty(Directory.GetFileSystemEntries(root));
            Assert.True(input.CanRead);
        });
    }

    [Fact]
    public async Task SymlinkEntry_IsRejectedWithoutLeavingFiles()
    {
        await WithService(async (service, root) =>
        {
            using var input = Zip(("Recipe.json", "{\"steps\":[]}"), ("link", "target"));
            using (var zip = new ZipArchive(input, ZipArchiveMode.Update, leaveOpen: true))
            {
                zip.GetEntry("link").ExternalAttributes = 0xA1FF << 16;
            }
            input.Position = 0;
            await Assert.ThrowsAsync<InvalidDataException>(() => service.StageAsync(input, "package.zip", TestContext.Current.CancellationToken));
            Assert.Empty(Directory.GetFileSystemEntries(root));
        });
    }

    [Theory]
    [InlineData("../escape.txt")]
    [InlineData("/rooted.txt")]
    [InlineData("folder\\escape.txt")]
    [InlineData("C:/escape.txt")]
    [InlineData("folder/./escape.txt")]
    [InlineData("folder//escape.txt")]
    public async Task UnsafeArchivePath_IsRejectedAndStagingRemoved(string path)
    {
        await WithService(async (service, root) =>
        {
            using var input = Zip(("Recipe.json", "{\"steps\":[]}"), (path, "payload"));
            await Assert.ThrowsAsync<InvalidDataException>(() => service.StageAsync(input, "package.zip", TestContext.Current.CancellationToken));
            Assert.Empty(Directory.GetFileSystemEntries(root));
            Assert.True(input.CanRead);
        });
    }

    [Fact]
    public async Task ValidZip_PreservesFilesAndOwnsCleanup()
    {
        await WithService(async (service, root) =>
        {
            using var input = Zip(("Recipe.json", "{\"steps\":[{\"name\":\"settings\"}]}"), ("nested/file.txt", "payload"));
            using (var package = await service.StageAsync(input, "package.ZIP", TestContext.Current.CancellationToken))
            {
                using var reader = new StreamReader(package.FileProvider.GetFileInfo("nested/file.txt").CreateReadStream());
                Assert.Equal("payload", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
            }
            Assert.Empty(Directory.GetFileSystemEntries(root));
            Assert.True(input.CanRead);
        });
    }

    [Theory]
    [InlineData("duplicate")]
    [InlineData("missing-recipe")]
    [InlineData("invalid-recipe")]
    [InlineData("expanded-limit")]
    [InlineData("upload-limit")]
    [InlineData("entry-limit")]
    public async Task InvalidPackageOrLimit_IsRejectedWithoutResidualFiles(string scenario)
    {
        var options = new DeploymentPackageOptions();
        if (scenario == "expanded-limit") { options.MaxExpandedBytes = 5; }
        if (scenario == "upload-limit") { options.MaxUploadBytes = 5; }
        if (scenario == "entry-limit") { options.MaxEntries = 1; }
        await WithService(async (service, root) =>
        {
            using var input = scenario switch
            {
                "duplicate" => Zip(("Recipe.json", "{\"steps\":[]}"), ("recipe.json", "{}")),
                "missing-recipe" => Zip(("other.json", "{}")),
                "invalid-recipe" => Zip(("Recipe.json", "{\"steps\":[{}]}")),
                _ => Zip(("Recipe.json", "{\"steps\":[]}"), ("file.txt", "payload")),
            };
            await Assert.ThrowsAsync<InvalidDataException>(() => service.StageAsync(input, "package.zip", TestContext.Current.CancellationToken));
            Assert.Empty(Directory.GetFileSystemEntries(root));
        }, options);
    }

    private static MemoryStream Zip(params (string Path, string Value)[] files)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, value) in files)
            {
                using var entry = zip.CreateEntry(path).Open();
                entry.Write(Encoding.UTF8.GetBytes(value));
            }
        }
        stream.Position = 0;
        return stream;
    }

    private static async Task WithService(Func<DeploymentPackageService, string, Task> test, DeploymentPackageOptions options = null)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            var temporary = new Mock<ITempDirectoryProvider>();
            temporary.Setup(provider => provider.CreateTempSubdirectory()).Returns(() =>
            {
                var folder = Path.Combine(root, Guid.NewGuid().ToString("n"));
                Directory.CreateDirectory(folder);
                return folder;
            });
            await test(new DeploymentPackageService(temporary.Object, Options.Create(options ?? new())), root);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
