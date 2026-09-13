using System.Security.Claims;
using OrchardCore.Security.Permissions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Deployment.Operations;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class DeploymentOperationStoreTests
{
    [Fact]
    public async Task Acceptance_RetainsFirstAuthorizationSnapshotAndOmitsTokenMaterial()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("sub", "client"), new Claim("oc:entyp", "application"),
                new Claim(Permission.ClaimType, "Export"), new Claim("access_token", "must-not-persist"),
                new Claim("role", "Administrator"),
            ], "Bearer", "name", "role"));
            var identity = DeploymentExecutionIdentity.Capture(principal);
            Assert.DoesNotContain(identity.Claims, claim => claim.Type == "access_token");
            var token = TestContext.Current.CancellationToken;
            var store = Store(root, "one");
            var operation = await store.CreateAsync("owner", "snapshot", DeploymentOperationKind.Export, "plan", token, identity);
            var retryIdentity = DeploymentExecutionIdentity.Capture(new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(Permission.ClaimType, "ManageUsers")], "Bearer")));
            var retry = await Store(root, "one").CreateAsync("owner", "snapshot", DeploymentOperationKind.Export, "plan", token, retryIdentity);
            Assert.Contains(retry.Identity.Claims, claim => claim.Value == "Export");
            Assert.DoesNotContain(retry.Identity.Claims, claim => claim.Value == "ManageUsers");
            Assert.True(retry.Identity.Restore().Identity.IsAuthenticated);
            Assert.True(retry.Identity.Restore().IsInRole("Administrator"));
            Assert.DoesNotContain("must-not-persist", string.Join("", Directory.GetFiles(root, "operation.json", SearchOption.AllDirectories).Select(File.ReadAllText)));
            Assert.Equal(operation.Id, retry.Id);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Acceptance_DeduplicatesExactRequestAndRejectsChangedPayload()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var store = Store(root, "one");
            var token = TestContext.Current.CancellationToken;
            var operation = await store.CreateAsync("owner", "request", DeploymentOperationKind.Import, "artifact", token);
            var reopened = Store(root, "one");
            Assert.Equal(operation, await reopened.CreateAsync("owner", "request", DeploymentOperationKind.Import, "artifact", token));
            await Assert.ThrowsAsync<DeploymentRequestConflictException>(() => reopened.CreateAsync("owner", "request", DeploymentOperationKind.Import, "different", token));
            Assert.Null(await reopened.FindAsync(operation.Id, "another", token));
            Assert.Null(await Store(root, "two").FindAsync(operation.Id, "owner", token));
            Assert.Null(await reopened.FindAsync("../operation.json", "owner", token));
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Claim_ExecutesOnceAndNeverReplaysInterruptedWork(bool complete)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var token = TestContext.Current.CancellationToken;
            var store = Store(root, "one");
            var operation = await store.CreateAsync("owner", "request", DeploymentOperationKind.Import, "artifact", token);
            using (var claim = await store.ClaimAsync(operation.Id, token))
            {
                Assert.NotNull(claim);
                Assert.Equal(operation.Id, (await store.CreateAsync("owner", "request", DeploymentOperationKind.Import, "artifact", token)).Id);
                Assert.Equal(DeploymentOperationState.Running, (await store.FindAsync(operation.Id, "owner", token)).State);
                Assert.Null(await Store(root, "one").ClaimAsync(operation.Id, token));
                if (complete) { await claim.CompleteAsync(DeploymentOperationState.Succeeded, null, null, token); }
            }
            var reopened = Store(root, "one");
            Assert.Null(await reopened.ClaimAsync(operation.Id, token));
            var result = await reopened.FindAsync(operation.Id, "owner", token);
            Assert.Equal(complete ? DeploymentOperationState.Succeeded : DeploymentOperationState.Uncertain, result.State);
            Assert.Equal(complete ? null : "execution_interrupted", result.ErrorCode);
            Assert.Null(await reopened.ClaimAsync(operation.Id, token));
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Runner_RecordsConfirmedOutcomeAndDoesNotRepeatExecution(bool fail)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var token = TestContext.Current.CancellationToken;
            var store = Store(root, "one");
            var operation = await store.CreateAsync("owner", "request", DeploymentOperationKind.Export, "snapshot", token);
            var executor = new Executor { Fail = fail };
            var runner = new DeploymentOperationRunner(store, executor, NullLogger<DeploymentOperationRunner>.Instance);
            await runner.RunAsync(operation.Id, token);
            await runner.RunAsync(operation.Id, token);
            Assert.Equal(1, executor.Calls);
            var result = await store.FindAsync(operation.Id, "owner", token);
            Assert.Equal(fail ? DeploymentOperationState.Failed : DeploymentOperationState.Succeeded, result.State);
            Assert.Equal(fail ? "execution_failed" : null, result.ErrorCode);
            Assert.Equal(fail ? null : "artifact-id", result.ArtifactId);
            Assert.Empty(await store.PendingAsync(token));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task ConcurrentAcceptance_PublishesOneOperationOrReturnsRetryableBusy()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var token = TestContext.Current.CancellationToken;
            var attempts = Enumerable.Range(0, 16).Select(async _ =>
            {
                try { return await Store(root, "one").CreateAsync("owner", "same", DeploymentOperationKind.Import, "artifact", token); }
                catch (DeploymentOperationBusyException) { return null; }
            });
            var results = await Task.WhenAll(attempts);
            var operation = await Store(root, "one").CreateAsync("owner", "same", DeploymentOperationKind.Import, "artifact", token);
            Assert.Contains(results, result => result is not null);
            Assert.All(results, result => Assert.True(result is null || result.Id == operation.Id));
            Assert.Single(await Store(root, "one").PendingAsync(token));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task ShutdownDuringExecution_RecoversAsUncertainAndDoesNotReplay()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var token = TestContext.Current.CancellationToken;
            var store = Store(root, "one");
            var operation = await store.CreateAsync("owner", "request", DeploymentOperationKind.Import, "artifact", token);
            using var shutdown = new CancellationTokenSource();
            var executor = new CancelledExecutor(shutdown);
            var runner = new DeploymentOperationRunner(store, executor, NullLogger<DeploymentOperationRunner>.Instance);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.RunAsync(operation.Id, shutdown.Token));
            Assert.Equal(DeploymentOperationState.Running, (await store.FindAsync(operation.Id, "owner", token)).State);
            var recovered = Store(root, "one");
            await new DeploymentOperationRunner(recovered, executor, NullLogger<DeploymentOperationRunner>.Instance).RunAsync(operation.Id, token);
            Assert.Equal(1, executor.Calls);
            Assert.Equal(DeploymentOperationState.Uncertain, (await recovered.FindAsync(operation.Id, "owner", token)).State);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task StatusReads_CanObserveAtomicTransitionsWhileExecutorOwnsClaim()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        try
        {
            var token = TestContext.Current.CancellationToken;
            var store = Store(root, "one");
            var operation = await store.CreateAsync("owner", "request", DeploymentOperationKind.Export, "snapshot", token);
            using var claim = await store.ClaimAsync(operation.Id, token);
            var reads = Enumerable.Range(0, 50).Select(async _ =>
            {
                var status = await store.FindAsync(operation.Id, "owner", token);
                Assert.Contains(status.State, new[] { DeploymentOperationState.Running, DeploymentOperationState.Succeeded });
            }).ToArray();
            await claim.CompleteAsync(DeploymentOperationState.Succeeded, "artifact", null, token);
            await Task.WhenAll(reads);
        }
        finally { Directory.Delete(root, true); }
    }

    private sealed class CancelledExecutor : IDeploymentOperationExecutor
    {
        private readonly CancellationTokenSource _shutdown;
        public int Calls { get; private set; }
        public CancelledExecutor(CancellationTokenSource shutdown) => _shutdown = shutdown;
        public async Task<string> ExecuteAsync(DeploymentOperation operation, CancellationToken cancellationToken)
        {
            Calls++;
            await _shutdown.CancelAsync();
            cancellationToken.ThrowIfCancellationRequested();
            return null;
        }
    }

    private sealed class Executor : IDeploymentOperationExecutor
    {
        public bool Fail { get; init; }
        public int Calls { get; private set; }
        public Task<string> ExecuteAsync(DeploymentOperation operation, CancellationToken cancellationToken)
        {
            Calls++;
            return Fail ? Task.FromException<string>(new InvalidOperationException("private failure")) : Task.FromResult("artifact-id");
        }
    }

    private static DeploymentOperationStore Store(string root, string tenant)
    {
        var clock = new Mock<IClock>();
        clock.SetupGet(value => value.UtcNow).Returns(() => DateTime.UtcNow);
        return new DeploymentOperationStore(Options.Create(new ShellOptions { ShellsApplicationDataPath = root, ShellsContainerName = "Sites" }),
            new ShellSettings { Name = tenant }, clock.Object);
    }
}
