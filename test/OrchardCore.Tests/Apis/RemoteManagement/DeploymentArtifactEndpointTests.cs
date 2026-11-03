using System.IO.Compression;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
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
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
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
            Assert.Equal(404, Status(await DeploymentArtifactEndpoints.DeleteAsync(context, allowed, store, artifact.Id)));
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
        var operations = new DeploymentOperationStore(settings, tenant, Options.Create(new DeploymentOperationOptions()), new EphemeralDataProtectionProvider(), clock.Object);
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
    public void ValidatePackage_Endpoint_IsReadOnlyWithoutConfirmation()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        var app = builder.Build();
        app.AddDeploymentOperationEndpoints();

        var metadata = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.RoutePattern.RawText == "api/deployment/operations:validate")
            .Metadata.GetRequiredMetadata<CliOperationMetadata>();

        Assert.Equal(["deployment", "operations"], metadata.CommandGroup);
        Assert.Equal("validate", metadata.Verb);
        Assert.True(metadata.ReadOnly);
        Assert.True(metadata.LongRunning);
        Assert.False(metadata.RequiresConfirmation);
        Assert.Equal(CliInputMode.Options, metadata.InputMode);
    }

    [Fact]
    public async Task ValidatePackage_WithoutImportPermission_IsForbidden()
    {
        await using var scenario = new ValidationScenario();
        var artifact = await scenario.CreateArtifactAsync(DeploymentArtifactKind.Import, "Recipe.json", "{\"steps\":[]}");

        var result = await scenario.ValidateAsync(artifact.Id, Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.ManageDeploymentPlan, DeploymentPermissions.Export));

        Assert.Equal(403, Status(result));
        Assert.Equal(403, Status(await scenario.ValidateAsync(artifact.Id, Authorize(DeploymentPermissions.Import))));
        Assert.Empty(Directory.GetFileSystemEntries(scenario.Staging));
    }

    [Fact]
    public async Task ValidatePackage_ArtifactThatCantBeImported_IsNotFound()
    {
        await using var scenario = new ValidationScenario();
        var export = await scenario.CreateArtifactAsync(DeploymentArtifactKind.Export, "Recipe.json", "{\"steps\":[]}");
        var import = await scenario.CreateArtifactAsync(DeploymentArtifactKind.Import, "Recipe.json", "{\"steps\":[]}");

        Assert.Equal(404, Status(await scenario.ValidateAsync(export.Id)));
        Assert.Equal(404, Status(await scenario.ValidateAsync(Guid.NewGuid().ToString("n"))));
        Assert.Equal(400, Status(await scenario.ValidateAsync(" ")));
        scenario.Context.User = Principal("user", "validator");
        Assert.Equal(404, Status(await scenario.ValidateAsync(import.Id)));
        Assert.Empty(Directory.GetFileSystemEntries(scenario.Staging));
    }

    [Fact]
    public async Task ValidatePackage_ValidZip_ResolvesFilesAndLeavesNothingBehind()
    {
        await using var scenario = new ValidationScenario();
        var artifact = await scenario.CreateArtifactAsync(DeploymentArtifactKind.Import, "package.zip", CreateZip(
            ("Recipe.json", """{"steps":[{"name":"Known","a":"[file:text('files/present.txt')]"}]}"""),
            ("files/present.txt", "content")));
        var before = scenario.Snapshot();

        var value = Assert.IsType<Ok<RecipeValidationResult>>(await scenario.ValidateAsync(artifact.Id)).Value;

        Assert.True(value.Valid);
        Assert.Equal(1, value.StepCount);
        Assert.Empty(value.Findings);
        Assert.Contains("doesn't guarantee", value.Notice);
        Assert.Equal(before, scenario.Snapshot());
        Assert.Empty(Directory.GetFileSystemEntries(scenario.Staging));
    }

    [Fact]
    public async Task ValidatePackage_StepAndFileProblems_AreReportedAndLeaveNothingBehind()
    {
        await using var scenario = new ValidationScenario();
        var artifact = await scenario.CreateArtifactAsync(DeploymentArtifactKind.Import, "package.zip", CreateZip(
            ("Recipe.json", """{"steps":[{"name":"Unknown"},{"name":"Known","a":"[file:text('../../outside.txt')]"}]}""")));
        var before = scenario.Snapshot();

        var value = Assert.IsType<Ok<RecipeValidationResult>>(await scenario.ValidateAsync(artifact.Id)).Value;

        Assert.False(value.Valid);
        Assert.Equal(
            [("unknown-step", 0, "Unknown"), ("file-not-found", 1, "Known")],
            value.Findings.Select(finding => (finding.Code, finding.StepIndex.Value, finding.StepName)));
        Assert.Equal(before, scenario.Snapshot());
        Assert.Empty(Directory.GetFileSystemEntries(scenario.Staging));
    }

    [Theory]
    [InlineData("traversal")]
    [InlineData("no-recipe")]
    [InlineData("invalid-json")]
    [InlineData("not-a-zip")]
    [InlineData("too-many-entries")]
    public async Task ValidatePackage_UnsafePackage_IsInvalidAndLeavesNothingBehind(string kind)
    {
        await using var scenario = new ValidationScenario(new DeploymentPackageOptions { MaxEntries = 2 });
        var package = kind switch
        {
            "traversal" => CreateZip(("../Recipe.json", "{\"steps\":[]}")),
            "no-recipe" => CreateZip(("other.json", "{}")),
            "invalid-json" => CreateZip(("Recipe.json", "{")),
            "too-many-entries" => CreateZip(("Recipe.json", "{\"steps\":[]}"), ("a", "a"), ("b", "b")),
            _ => Encoding.UTF8.GetBytes("not a zip"),
        };
        var artifact = await scenario.CreateArtifactAsync(DeploymentArtifactKind.Import, "package.zip", package);
        var before = scenario.Snapshot();

        var value = Assert.IsType<Ok<RecipeValidationResult>>(await scenario.ValidateAsync(artifact.Id)).Value;

        Assert.False(value.Valid);
        Assert.Equal("package-invalid", Assert.Single(value.Findings).Code);
        Assert.Equal(before, scenario.Snapshot());
        Assert.Empty(Directory.GetFileSystemEntries(scenario.Staging));
    }

    [Fact]
    public async Task ExportSubmission_RequiresExportPermission_DeduplicatesAndSanitizesStatus()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(() => DateTime.UtcNow);
        var operations = new DeploymentOperationStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
            new ShellSettings { Name = "Tenant" }, Options.Create(new DeploymentOperationOptions()), new EphemeralDataProtectionProvider(), clock.Object);
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

    private static byte[] CreateZip(params (string Path, string Content)[] entries)
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in entries)
            {
                using var writer = new StreamWriter(zip.CreateEntry(path).Open());
                writer.Write(content);
            }
        }

        return output.ToArray();
    }

    private sealed class KnownStep : NamedRecipeStepHandler
    {
        public KnownStep()
            : base("Known")
        {
        }

        protected override Task HandleAsync(RecipeExecutionContext context) => throw new InvalidOperationException("A validation must not execute a step.");
    }

    private sealed class ValidationScenario : IAsyncDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        private readonly ServiceProvider _services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        private readonly DeploymentArtifactStore _artifacts;
        private readonly DeploymentPackageService _packages;
        private readonly RecipeValidator _validator;

        public ValidationScenario(DeploymentPackageOptions options = null)
        {
            Staging = Path.Combine(_root, "staging");
            Directory.CreateDirectory(Staging);
            var clock = new Mock<IClock>();
            clock.SetupGet(value => value.UtcNow).Returns(() => DateTime.UtcNow);
            _artifacts = new DeploymentArtifactStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = _root, ShellsContainerName = "Sites" }),
                new ShellSettings { Name = "Tenant" }, Options.Create(new DeploymentArtifactOptions()), clock.Object);
            var temporary = new Mock<ITempDirectoryProvider>();
            temporary.Setup(value => value.CreateTempSubdirectory()).Returns(() => Directory.CreateDirectory(Path.Combine(Staging, Guid.NewGuid().ToString("n"))).FullName);
            _packages = new DeploymentPackageService(temporary.Object, Options.Create(options ?? new DeploymentPackageOptions()));
            IRecipeStepHandler[] handlers = [new KnownStep()];
            _validator = new RecipeValidator(handlers, [], _services.GetRequiredService<IStringLocalizer<RecipeValidator>>());
            Context = new DefaultHttpContext { RequestServices = _services, User = Principal("application", "validator") };
        }

        public string Staging { get; }

        public DefaultHttpContext Context { get; }

        public Task<DeploymentArtifact> CreateArtifactAsync(DeploymentArtifactKind kind, string fileName, string content)
            => CreateArtifactAsync(kind, fileName, Encoding.UTF8.GetBytes(content));

        public async Task<DeploymentArtifact> CreateArtifactAsync(DeploymentArtifactKind kind, string fileName, byte[] content)
        {
            using var input = new MemoryStream(content);

            return await _artifacts.CreateAsync(DeploymentArtifactOwner.Get(Context.User), kind, fileName,
                fileName.EndsWith(".zip", StringComparison.Ordinal) ? "application/zip" : "application/json", input, TestContext.Current.CancellationToken);
        }

        public Task<IResult> ValidateAsync(string artifactId, IAuthorizationService authorization = null)
            => DeploymentOperationEndpoints.ValidateAsync(Context,
                authorization ?? Authorize(RemoteManagementPermissions.AccessRemoteManagement, DeploymentPermissions.Import),
                _artifacts, _packages, _validator, new DeploymentValidateRequest { ArtifactId = artifactId });

        // Every file of the tenant with its content, to tell that nothing was added, changed or removed.
        public string[] Snapshot() => Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(path => path + ":" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))))
            .ToArray();

        public async ValueTask DisposeAsync()
        {
            await _services.DisposeAsync();
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
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
