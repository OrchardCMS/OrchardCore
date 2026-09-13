using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Deployment.Core.Services;
using OrchardCore.Deployment.Operations;
using OrchardCore.Json;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Artifacts;
using OrchardCore.Deployment.Endpoints.Management;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentArtifactEndpointTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OwnedArtifact_RequiresPurposePermission_StreamsBytesAndReleasesLease(bool import)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(() => DateTime.UtcNow);
        var store = new DeploymentArtifactStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
            new ShellSettings { Name = "Tenant" }, Options.Create(new DeploymentArtifactOptions()), clock.Object);
        try
        {
            var context = new DefaultHttpContext { RequestServices = services, User = Principal("application", "same-subject") };
            context.Request.Method = "GET";
            var owner = DeploymentArtifactOwner.Get(context.User);
            var bytes = Encoding.UTF8.GetBytes("artifact content");
            using var input = new MemoryStream(bytes);
            var artifact = await store.CreateAsync(owner, import ? DeploymentArtifactKind.Import : DeploymentArtifactKind.Export,
                "artifact.zip", "application/zip", input, TestContext.Current.CancellationToken);
            var allowed = Authorize(RemoteManagementPermissions.AccessRemoteManagement, import ? DeploymentPermissions.Import : DeploymentPermissions.Export);
            var denied = Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.ManageDeploymentPlan);
            Assert.Equal(403, Status(await DeploymentArtifactEndpoints.GetAsync(context, denied, store, artifact.Id)));
            Assert.Equal(403, Status(await DeploymentArtifactEndpoints.DownloadAsync(context, denied, store, artifact.Id)));
            Assert.Equal(403, Status(await DeploymentArtifactEndpoints.DeleteAsync(context, denied, store, artifact.Id)));
            var metadata = Assert.IsType<Ok<DeploymentArtifactResponse>>(await DeploymentArtifactEndpoints.GetAsync(context, allowed, store, artifact.Id)).Value;
            var json = JsonSerializer.Serialize(metadata);
            Assert.DoesNotContain("same-subject", json);
            Assert.DoesNotContain(root, json);
            Assert.Equal(bytes.Length, metadata.Length);
            context.User = Principal("user", "same-subject");
            Assert.Equal(404, Status(await DeploymentArtifactEndpoints.GetAsync(context, allowed, store, artifact.Id)));
            context.User = Principal("application", "same-subject");
            var download = await DeploymentArtifactEndpoints.DownloadAsync(context, allowed, store, artifact.Id);
            Assert.Equal(409, Status(await DeploymentArtifactEndpoints.DeleteAsync(context, allowed, store, artifact.Id)));
            using var output = new MemoryStream();
            context.Response.Body = output;
            await download.ExecuteAsync(context);
            Assert.Equal(bytes, output.ToArray());
            Assert.True(Assert.IsType<Ok<DeploymentArtifactDeleteResponse>>(await DeploymentArtifactEndpoints.DeleteAsync(context, allowed, store, artifact.Id)).Value.Changed);
            Assert.False(Assert.IsType<Ok<DeploymentArtifactDeleteResponse>>(await DeploymentArtifactEndpoints.DeleteAsync(context, allowed, store, artifact.Id)).Value.Changed);
        }
        finally
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
    }

    [Theory]
    [InlineData("valid", 200)]
    [InlineData("transformed", 200)]
    [InlineData("invalid", 400)]
    [InlineData("pipeline-denied", 400)]
    [InlineData("permission-denied", 403)]
    [InlineData("oversized", 413)]
    [InlineData("bad-name", 400)]
    public async Task Upload_UsesPipelineAndValidationBeforePublishingArtifact(string scenario, int status)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        var staging = Path.Combine(root, "staging");
        Directory.CreateDirectory(staging);
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(DateTime.UtcNow);
        var store = new DeploymentArtifactStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
            new ShellSettings { Name = "Tenant" }, Options.Create(new DeploymentArtifactOptions()), clock.Object);
        var temporary = new Mock<ITempDirectoryProvider>();
        temporary.Setup(value => value.CreateTempSubdirectory()).Returns(() => Directory.CreateDirectory(Path.Combine(staging, Guid.NewGuid().ToString("n"))).FullName);
        var options = Options.Create(new DeploymentPackageOptions());
        var packages = new DeploymentPackageService(temporary.Object, options);
        var handler = new Mock<IFileEventHandler>();
        handler.Setup(value => value.CreatingAsync(It.IsAny<FileCreatingContext>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileCreatingContext _, Stream stream, CancellationToken _) => scenario == "pipeline-denied"
                ? FileCreatingResult.Failed(stream)
                : FileCreatingResult.Success(scenario == "transformed" ? new MemoryStream(Encoding.UTF8.GetBytes("{\"steps\":[]}")) : stream));
        try
        {
            using var input = new MemoryStream(Encoding.UTF8.GetBytes(scenario is "invalid" or "transformed" ? "invalid" : "{\"steps\":[]}"));
            var context = new DefaultHttpContext { RequestServices = services, User = Principal("application", "uploader") };
            context.Request.Body = input;
            context.Request.ContentLength = scenario == "oversized" ? options.Value.MaxUploadBytes + 1 : input.Length;
            var authorization = scenario == "permission-denied" ? Authorize(RemoteManagementPermissions.AccessRemoteManagement)
                : Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.Import);
            var result = await DeploymentArtifactEndpoints.UploadAsync(context, authorization, store, packages,
                new FileCreationService([handler.Object]), options, scenario == "bad-name" ? "../Recipe.json" : "Recipe.json");
            Assert.Equal(status, Status(result));
            Assert.Empty(Directory.GetFileSystemEntries(staging));
            handler.Verify(value => value.CreatingAsync(It.IsAny<FileCreatingContext>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
                scenario is "permission-denied" or "oversized" or "bad-name" ? Times.Never() : Times.Once());
            if (status == 200)
            {
                var metadata = Assert.IsType<Ok<DeploymentArtifactResponse>>(result).Value;
                Assert.Equal("import", metadata.Kind);
                using var lease = await store.OpenAsync(metadata.Id, DeploymentArtifactOwner.Get(context.User));
                Assert.NotNull(lease);
                using var reader = new StreamReader(lease.Stream);
                Assert.Equal("{\"steps\":[]}", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
                Assert.Null(await store.FindAsync(metadata.Id, DeploymentArtifactOwner.Get(Principal("user", "uploader"))));
            }
            else
            {
                Assert.False(Directory.Exists(Path.Combine(root, "Sites")));
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ImportRetry_AfterArtifactDeletion_ReturnsExistingOperationButRejectsChangedTarget()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(() => DateTime.UtcNow);
        var settings = Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" });
        var tenant = new ShellSettings { Name = "Tenant" };
        var operations = new DeploymentOperationStore(settings, tenant, clock.Object);
        var artifacts = new DeploymentArtifactStore(settings, tenant, Options.Create(new DeploymentArtifactOptions()), clock.Object);
        try
        {
            var context = new DefaultHttpContext { RequestServices = services, User = Principal("application", "importer") };
            var owner = DeploymentArtifactOwner.Get(context.User);
            using var input = new MemoryStream(Encoding.UTF8.GetBytes("{\"steps\":[]}"));
            var artifact = await artifacts.CreateAsync(owner, DeploymentArtifactKind.Import, "Recipe.json", "application/json", input, TestContext.Current.CancellationToken);
            var allowed = Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.Import);
            var request = new DeploymentImportRequest { RequestId = "retry", ArtifactId = artifact.Id };
            var accepted = Assert.IsType<Accepted<DeploymentOperationResponse>>(await DeploymentOperationEndpoints.ImportAsync(context, allowed, artifacts, operations, request)).Value;
            using (var claim = await operations.ClaimAsync(accepted.Id, TestContext.Current.CancellationToken))
            {
                await claim.CompleteAsync(DeploymentOperationState.Succeeded, null, null, TestContext.Current.CancellationToken);
            }
            Assert.Equal(ArtifactDeleteResult.Deleted, await artifacts.DeleteAsync(artifact.Id, owner));
            var retry = Assert.IsType<Accepted<DeploymentOperationResponse>>(await DeploymentOperationEndpoints.ImportAsync(context, allowed, artifacts, operations, request)).Value;
            Assert.Equal(accepted.Id, retry.Id);
            Assert.Equal("succeeded", retry.State);
            request.ArtifactId = Guid.NewGuid().ToString("n");
            Assert.Equal(409, Status(await DeploymentOperationEndpoints.ImportAsync(context, allowed, artifacts, operations, request)));
            request.RequestId = "new";
            Assert.Equal(404, Status(await DeploymentOperationEndpoints.ImportAsync(context, allowed, artifacts, operations, request)));
        }
        finally { if (Directory.Exists(root)) { Directory.Delete(root, true); } }
    }

    [Fact]
    public async Task ExportSubmission_RequiresExportPermission_DeduplicatesAndSanitizesStatus()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(() => DateTime.UtcNow);
        var operations = new DeploymentOperationStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
            new ShellSettings { Name = "Tenant" }, clock.Object);
        try
        {
            var context = new DefaultHttpContext { RequestServices = services, User = Principal("application", "submitter") };
            var plans = new Mock<IDeploymentPlanService>();
            plans.Setup(value => value.GetAsync(1)).ReturnsAsync(new DeploymentPlan { Id = 1, Name = "private plan" });
            var options = Options.Create(new DocumentJsonSerializerOptions());
            var denied = Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.ManageDeploymentPlan);
            var allowed = Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.Export);
            var request = new DeploymentExportRequest { RequestId = "retry", PlanId = 1 };
            Assert.Equal(403, Status(await DeploymentOperationEndpoints.ExportAsync(context, denied, plans.Object, operations, options, request)));
            plans.Verify(value => value.GetAsync(It.IsAny<long>()), Times.Never());
            var accepted = Assert.IsType<Accepted<DeploymentOperationResponse>>(await DeploymentOperationEndpoints.ExportAsync(context, allowed, plans.Object, operations, options, request)).Value;
            Assert.Equal("pending", accepted.State);
            var retry = Assert.IsType<Accepted<DeploymentOperationResponse>>(await DeploymentOperationEndpoints.ExportAsync(context, allowed, plans.Object, operations, options, request)).Value;
            Assert.Equal(accepted.Id, retry.Id);
            plans.Setup(value => value.GetAsync(1)).ReturnsAsync(new DeploymentPlan { Id = 1, Name = "changed" });
            Assert.Equal(409, Status(await DeploymentOperationEndpoints.ExportAsync(context, allowed, plans.Object, operations, options, request)));
            Assert.Equal(403, Status(await DeploymentOperationEndpoints.GetAsync(context, denied, operations, accepted.Id)));
            var status = Assert.IsType<Ok<DeploymentOperationResponse>>(await DeploymentOperationEndpoints.GetAsync(context, allowed, operations, accepted.Id)).Value;
            var json = JsonSerializer.Serialize(status);
            Assert.DoesNotContain("submitter", json);
            Assert.DoesNotContain("private plan", json);
            Assert.DoesNotContain("payload", json, StringComparison.OrdinalIgnoreCase);
            context.User = Principal("user", "submitter");
            Assert.Equal(404, Status(await DeploymentOperationEndpoints.GetAsync(context, allowed, operations, accepted.Id)));
        }
        finally { if (Directory.Exists(root)) { Directory.Delete(root, true); } }
    }

    [Fact]
    public void OwnerIdentity_SeparatesEntityKindsAndIssuers_AndRejectsIncompletePrincipals()
    {
        Assert.NotEqual(DeploymentArtifactOwner.Get(Principal("user", "same")), DeploymentArtifactOwner.Get(Principal("application", "same")));
        var first = Principal("user", "same");
        first.Identities.Single().AddClaim(new Claim("iss", "first"));
        var second = Principal("user", "same");
        second.Identities.Single().AddClaim(new Claim("iss", "second"));
        Assert.NotEqual(DeploymentArtifactOwner.Get(first), DeploymentArtifactOwner.Get(second));
        Assert.Null(DeploymentArtifactOwner.Get(Principal("unknown", "same")));
        Assert.Null(DeploymentArtifactOwner.Get(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "same")]))));
    }

    private static ClaimsPrincipal Principal(string kind, string subject) => new(new ClaimsIdentity(
        [new Claim("oc:entyp", kind), new Claim("sub", subject)], "test"));

    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;

    private static IAuthorizationService Authorize(params Permission[] permissions)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => permissions.Any(permission => permission.Name == requirement.Permission.Name))
                    ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
