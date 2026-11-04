using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Deployment.Remote.Controllers;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Documents;
using OrchardCore.Deployment.Controllers;
using OrchardCore.Deployment.Steps;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Services;
using OrchardCore.FileStorage;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingRemoteExport_SendsArchiveAndDisposesItOnSuccessOrFailure(bool failSend)
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var features = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await features.EnableFeaturesAsync((await features.GetAvailableFeaturesAsync()).Where(feature =>
                feature.Id == "OrchardCore.Deployment"), force: true);
        });
        long id = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plan = new DeploymentPlan { Name = "Remote sample", DeploymentSteps =
                [new CustomFileDeploymentStep { FileName = "sample.txt", FileContent = "remote bytes" }], };
            await scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>().SaveAsync(plan);
            id = plan.Id;
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var documents = new Mock<IDocumentManager<RemoteInstanceList>>();
            documents.Setup(value => value.GetOrCreateImmutableAsync()).ReturnsAsync(new RemoteInstanceList
            {
                RemoteInstances = [new RemoteInstance { Id = "target", Url = "http://localhost/unused", ClientName = "test", ApiKey = "test" }],
            });
            Stream sent = null;
            using var handler = new ArchiveHandler(async request =>
            {
                var multipart = Assert.IsType<MultipartFormDataContent>(request.Content);
                var part = Assert.Single(multipart, part => part.Headers.ContentDisposition.FileName is not null);
                Assert.EndsWith(".zip", part.Headers.ContentDisposition.FileName.Trim('"'));
                sent = await part.ReadAsStreamAsync(TestContext.Current.CancellationToken);
                using var zip = new ZipArchive(sent, ZipArchiveMode.Read, leaveOpen: true);
                using var json = await JsonDocument.ParseAsync(zip.GetEntry("Recipe.json").Open(), cancellationToken: TestContext.Current.CancellationToken);
                Assert.Equal("", json.RootElement.GetProperty("name").GetString());
                using var reader = new StreamReader(zip.GetEntry("sample.txt").Open());
                Assert.Equal("remote bytes", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
                if (failSend) { throw new InvalidOperationException("Simulated send failure"); }
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            });
            using var client = new HttpClient(handler);
            var clients = new Mock<IHttpClientFactory>();
            clients.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(client);
            var controller = new ExportRemoteInstanceController(Authorize(true),
                scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>(), new RemoteInstanceService(documents.Object),
                scope.ServiceProvider.GetRequiredService<IDeploymentArchiveService>(), Mock.Of<INotifier>(), clients.Object,
                Mock.Of<IHtmlLocalizer<ExportRemoteInstanceController>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider } },
            };
            if (failSend)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Execute(id, "target", "/"));
            }
            else
            {
                Assert.IsType<LocalRedirectResult>(await controller.Execute(id, "target", "/"));
            }
            Assert.NotNull(sent);
            Assert.False(sent.CanRead);
        });
    }

    private sealed class ArchiveHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => send(request);
    }

    [Fact]
    public async Task AdminDownload_UsesSharedArchiveAndStreamsRecipeMetadataAndFiles()
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            await manager.EnableFeaturesAsync((await manager.GetAvailableFeaturesAsync()).Where(feature =>
                feature.Id == "OrchardCore.Deployment"), force: true);
        });
        long id = 0;
        await site.UsingTenantScopeAsync(async scope =>
        {
            var plan = new DeploymentPlan
            {
                Name = "Controller export",
                DeploymentSteps = [new RecipeFileDeploymentStep { RecipeName = "Admin recipe", Author = "Author", Tags = "one,two" },
                    new CustomFileDeploymentStep { FileName = "nested/admin.txt", FileContent = "admin bytes" }],
            };
            await scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>().SaveAsync(plan);
            id = plan.Id;
        });
        await site.UsingTenantScopeAsync(async scope =>
        {
            var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            http.Request.Method = "GET";
            using var output = new MemoryStream();
            http.Response.Body = output;
            var controller = new ExportFileController(Authorize(true),
                scope.ServiceProvider.GetRequiredService<global::YesSql.ISession>(),
                scope.ServiceProvider.GetRequiredService<IDeploymentArchiveService>())
            {
                ControllerContext = new ControllerContext { HttpContext = http },
            };
            var result = Assert.IsType<FileStreamResult>(await controller.Execute(id));
            Assert.Equal("application/zip", result.ContentType);
            Assert.EndsWith(".zip", result.FileDownloadName);
            await result.ExecuteResultAsync(controller.ControllerContext);
            Assert.False(result.FileStream.CanRead);
            output.Position = 0;
            using var zip = new ZipArchive(output, ZipArchiveMode.Read, leaveOpen: true);
            using var json = await JsonDocument.ParseAsync(zip.GetEntry("Recipe.json").Open(), cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("Admin recipe", json.RootElement.GetProperty("name").GetString());
            Assert.Equal("Author", json.RootElement.GetProperty("author").GetString());
            Assert.Equal(2, json.RootElement.GetProperty("tags").GetArrayLength());
            using var reader = new StreamReader(zip.GetEntry("nested/admin.txt").Open());
            Assert.Equal("admin bytes", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        });
    }

    [Fact]
    public async Task AdminDownload_DeniedBeforePlanReadOrArchiveCreation()
    {
        var session = new Mock<global::YesSql.ISession>(MockBehavior.Strict);
        var archives = new Mock<IDeploymentArchiveService>(MockBehavior.Strict);
        var controller = new ExportFileController(Authorize(false), session.Object, archives.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        Assert.IsType<ForbidResult>(await controller.Execute(1));
        session.VerifyNoOtherCalls();
        archives.VerifyNoOtherCalls();
    }

    private static IAuthorizationService Authorize(bool allowed)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }

    [Fact]
    public async Task ConcurrentExports_PreserveRecipeAndFiles_AndCleanUpOnDispose()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            var temporary = new Mock<ITempDirectoryProvider>();
            temporary.Setup(provider => provider.GetRootDirectory()).Returns(root);
            var source = new Mock<IDeploymentSource>();
            source.Setup(value => value.ProcessDeploymentStepAsync(It.IsAny<DeploymentStep>(), It.IsAny<DeploymentPlanResult>()))
                .Returns(async (DeploymentStep _, DeploymentPlanResult result) =>
                    await result.FileBuilder.SetFileAsync("nested/sample.txt", Encoding.UTF8.GetBytes("payload")));
            var manager = new DeploymentManager([source.Object], [], []);
            var service = new DeploymentArchiveService(manager, temporary.Object);
            var plan = new DeploymentPlan { Name = "Same name", DeploymentSteps = [new UnknownDeploymentStep()] };
            var first = await service.CreateAsync(plan, new RecipeDescriptor { Name = "First", Tags = ["test"] });
            var second = await service.CreateAsync(plan, new RecipeDescriptor { Name = "Second" });
            await using (first)
            await using (second)
            {
                Assert.Empty(Directory.GetDirectories(root));
                using var zip = new ZipArchive(first, ZipArchiveMode.Read, leaveOpen: true);
                using var json = await JsonDocument.ParseAsync(zip.GetEntry("Recipe.json").Open(), cancellationToken: TestContext.Current.CancellationToken);
                Assert.Equal("First", json.RootElement.GetProperty("name").GetString());
                Assert.Equal("test", json.RootElement.GetProperty("tags")[0].GetString());
                using var reader = new StreamReader(zip.GetEntry("nested/sample.txt").Open());
                Assert.Equal("payload", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
                using var other = new ZipArchive(second, ZipArchiveMode.Read, leaveOpen: true);
                using var otherJson = await JsonDocument.ParseAsync(other.GetEntry("Recipe.json").Open(), cancellationToken: TestContext.Current.CancellationToken);
                Assert.Equal("Second", otherJson.RootElement.GetProperty("name").GetString());
            }
            Assert.Empty(Directory.GetFileSystemEntries(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task FailedExport_RemovesStagedFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            var temporary = new Mock<ITempDirectoryProvider>();
            temporary.Setup(provider => provider.GetRootDirectory()).Returns(root);
            var manager = new Mock<IDeploymentManager>();
            manager.Setup(value => value.ExecuteDeploymentPlanAsync(It.IsAny<DeploymentPlan>(), It.IsAny<DeploymentPlanResult>()))
                .Returns(async (DeploymentPlan _, DeploymentPlanResult result) =>
                {
                    await result.FileBuilder.SetFileAsync("partial.txt", Encoding.UTF8.GetBytes("partial"));
                    throw new InvalidOperationException("Source failed");
                });
            var service = new DeploymentArchiveService(manager.Object, temporary.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new DeploymentPlan(), new RecipeDescriptor()));
            Assert.Empty(Directory.GetFileSystemEntries(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
