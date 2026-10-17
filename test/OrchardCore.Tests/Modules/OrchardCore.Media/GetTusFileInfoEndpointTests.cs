using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.FileStorage;
using OrchardCore.FileStorage.FileSystem;
using OrchardCore.Media;
using OrchardCore.Media.Core;
using OrchardCore.Media.Core.Helpers;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class GetTusFileInfoEndpointTests
{
    private static readonly MethodInfo _handleAsyncMethod = typeof(GetTusFileInfoEndpoint)
        .GetMethod("HandleAsync", BindingFlags.NonPublic | BindingFlags.Static);

    [Fact]
    public async Task HandleAsync_UploadOutsideAuthorizedFolder_DoesNotReturnFileAndKeepsEntry()
    {
        // A user holding only ManageMedia (e.g. Editor, folder-scoped via ManageMediaFolder) knows
        // another user's tus uploadId and requests its info. Even though ManageMedia succeeds, the
        // folder-scoped check on the entry's actual file path must still deny access, and the
        // metadata entry must be left intact (not destroyed as a side effect of a denied request).
        using var testContext = await TestFixture.CreateAsync();

        var victimPath = await testContext.CreateFileAsync("_users/victim", "secret.pdf");
        await testContext.TusMetadataStore.SetAsync("upload-1", new TusUploadEntry { MediaFilePath = victimPath }, TestContext.Current.CancellationToken);

        var authorizationService = CreateAuthorizationService(authorizedFolders: []);

        var result = await testContext.InvokeAsync(authorizationService, "upload-1");

        Assert.IsType<ProblemHttpResult>(result);

        // The entry must still be retrievable: a denied request must not have removed it.
        var entryAfter = await testContext.TusMetadataStore.GetAsync("upload-1", TestContext.Current.CancellationToken);
        Assert.NotNull(entryAfter);
    }

    [Fact]
    public async Task HandleAsync_UploadInAuthorizedFolder_ReturnsFileAndRemovesEntry()
    {
        using var testContext = await TestFixture.CreateAsync();

        var ownPath = await testContext.CreateFileAsync("_users/me", "photo.jpg");
        await testContext.TusMetadataStore.SetAsync("upload-2", new TusUploadEntry { MediaFilePath = ownPath }, TestContext.Current.CancellationToken);

        var authorizationService = CreateAuthorizationService(authorizedFolders: ["_users/me"]);

        var result = await testContext.InvokeAsync(authorizationService, "upload-2");

        Assert.IsType<Ok<FileStoreEntryDto>>(result);

        var entryAfter = await testContext.TusMetadataStore.GetAsync("upload-2", TestContext.Current.CancellationToken);
        Assert.Null(entryAfter);
    }

    [Fact]
    public async Task HandleAsync_UnknownUploadId_ReturnsNotFound()
    {
        using var testContext = await TestFixture.CreateAsync();

        var authorizationService = CreateAuthorizationService(authorizedFolders: []);

        var result = await testContext.InvokeAsync(authorizationService, "missing-upload");

        Assert.IsType<ProblemHttpResult>(result);
    }

    private static IAuthorizationService CreateAuthorizationService(string[] authorizedFolders)
    {
        var authorized = authorizedFolders.ToHashSet(StringComparer.Ordinal);
        var authorizationService = new Mock<IAuthorizationService>();

        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Returns<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, resource, requirements) =>
            {
                var permissionRequirement = requirements.OfType<PermissionRequirement>().FirstOrDefault();

                if (permissionRequirement is null)
                {
                    return Task.FromResult(AuthorizationResult.Failed());
                }

                if (permissionRequirement.Permission.Name == MediaPermissions.ManageMedia.Name)
                {
                    return Task.FromResult(AuthorizationResult.Success());
                }

                if (permissionRequirement.Permission.Name == MediaPermissions.ManageMediaFolder.Name
                    && resource is string path
                    && authorized.Any(folder =>
                        string.Equals(path, folder, StringComparison.Ordinal)
                        || path.StartsWith(folder + "/", StringComparison.Ordinal)))
                {
                    return Task.FromResult(AuthorizationResult.Success());
                }

                return Task.FromResult(AuthorizationResult.Failed());
            });

        return authorizationService.Object;
    }

    private sealed class TestFixture : IDisposable
    {
        private readonly string _root;

        public IMediaFileStore FileStore { get; private init; }

        public DistributedTusUploadMetadataStore TusMetadataStore { get; private init; }

        private TestFixture(string root, IMediaFileStore fileStore, DistributedTusUploadMetadataStore tusMetadataStore)
        {
            _root = root;
            FileStore = fileStore;
            TusMetadataStore = tusMetadataStore;
        }

        public static Task<TestFixture> CreateAsync()
        {
            var root = Directory.CreateTempSubdirectory("tus-file-info-tests").FullName;

            var fileStore = new DefaultMediaFileStore(
                new FileSystemStore(root, NullLogger<FileSystemStore>.Instance),
                "/media", "", [], [],
                new FileSizeHelper(Mock.Of<IStringLocalizer<FileSizeHelper>>()),
                NullLogger<DefaultMediaFileStore>.Instance);

            IDistributedCache cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
            var tusMetadataStore = new DistributedTusUploadMetadataStore(
                cache,
                new ShellSettings { Name = "Default" },
                Options.Create(new MediaOptions { TemporaryFileLifetime = TimeSpan.FromMinutes(5) }));

            return Task.FromResult(new TestFixture(root, fileStore, tusMetadataStore));
        }

        public async Task<string> CreateFileAsync(string folder, string fileName)
        {
            var path = FileStore.Combine(folder, fileName);
            await FileStore.TryCreateDirectoryAsync(folder);
            using var stream = new MemoryStream("content"u8.ToArray());
            return await FileStore.CreateFileFromStreamAsync(path, stream);
        }

        public async Task<IResult> InvokeAsync(IAuthorizationService authorizationService, string uploadId)
        {
            var services = new ServiceCollection();

            var problemLocalizer = new Mock<IStringLocalizer<ProblemDetailsApiLocalization>>();
            problemLocalizer.Setup(l => l[It.IsAny<string>()])
                .Returns((string key) => new LocalizedString(key, key));
            services.AddSingleton(problemLocalizer.Object);

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity("Test")),
                RequestServices = services.BuildServiceProvider(),
            };

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            var fileVersionProvider = Mock.Of<IFileVersionProvider>();

            var task = (Task<IResult>)_handleAsyncMethod.Invoke(
                null,
                [httpContext, authorizationService, FileStore, contentTypeProvider, fileVersionProvider, TusMetadataStore, uploadId]);

            return await task;
        }

        public void Dispose()
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
