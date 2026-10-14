using System.Security.Claims;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class SharedLinkTests
{
    private static readonly DateTime _now = new(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CheckAccess_RecipientWithValidToken_IsAllowed()
    {
        // Arrange
        var (link, token) = Share("alice-id");

        // Act
        var result = SharedLinkAccess.Check(link, token, User("alice-id"), _now);

        // Assert
        Assert.Equal(SharedLinkAccessResult.Allowed, result);
    }

    [Fact]
    public void CheckAccess_UserWhoIsNotARecipient_IsDenied()
    {
        // Arrange
        var (link, token) = Share("alice-id");

        // Act
        var result = SharedLinkAccess.Check(link, token, User("mallory-id"), _now);

        // Assert
        Assert.Equal(SharedLinkAccessResult.NotARecipient, result);
    }

    [Fact]
    public void CheckAccess_WrongOrMissingToken_IsDenied()
    {
        // Arrange
        var (link, _) = Share("alice-id");

        // Act
        var wrong = SharedLinkAccess.Check(link, SharedLinkAccess.CreateToken(), User("alice-id"), _now);
        var missing = SharedLinkAccess.Check(link, null, User("alice-id"), _now);
        var empty = SharedLinkAccess.Check(link, string.Empty, User("alice-id"), _now);

        // Assert
        Assert.Equal(SharedLinkAccessResult.InvalidToken, wrong);
        Assert.Equal(SharedLinkAccessResult.InvalidToken, missing);
        Assert.Equal(SharedLinkAccessResult.InvalidToken, empty);
    }

    [Fact]
    public void CheckAccess_ExpiredOrRevokedLink_IsDenied()
    {
        // Arrange
        var (link, token) = Share("alice-id");

        // Act
        var expired = SharedLinkAccess.Check(link, token, User("alice-id"), link.ExpiresUtc.AddSeconds(1));
        link.RevokedUtc = _now;
        var revoked = SharedLinkAccess.Check(link, token, User("alice-id"), _now);

        // Assert
        Assert.Equal(SharedLinkAccessResult.Expired, expired);
        Assert.Equal(SharedLinkAccessResult.Expired, revoked);
    }

    [Fact]
    public void CheckAccess_UsedAsManyTimesAsAllowed_IsDenied()
    {
        // Arrange
        var (link, token) = Share("alice-id");
        link.MaxUses = 2;
        link.Uses = 2;

        // Act
        var result = SharedLinkAccess.Check(link, token, User("alice-id"), _now);

        // Assert
        Assert.Equal(SharedLinkAccessResult.UsedUp, result);
        Assert.False(link.IsActive(_now));
    }

    [Fact]
    public void CreateToken_EveryCall_IsUniqueAndOnlyItsHashIsStored()
    {
        // Act
        var first = SharedLinkAccess.CreateToken();
        var second = SharedLinkAccess.CreateToken();
        var hash = SharedLinkAccess.Hash(first);

        // Assert
        Assert.NotEqual(first, second);
        Assert.True(first.Length >= 40);
        Assert.Equal(64, hash.Length);
        Assert.DoesNotContain(first, hash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UseLink_UntilMaxUses_CountsEachUseThenStops()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var (link, token) = Share("alice-id");
        link.MaxUses = 2;

        await context.UsingTenantScopeAsync(async scope =>
        {
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(link, cancellationToken: TestContext.Current.CancellationToken);
        });

        var results = new List<SharedLinkAccessResult>();

        // Act
        for (var i = 0; i < 3; i++)
        {
            await context.UsingTenantScopeAsync(async scope =>
            {
                var manager = scope.ServiceProvider.GetRequiredService<ISharedLinkManager>();
                var (result, _) = await manager.UseAsync(link.LinkId, token, User("alice-id"), TestContext.Current.CancellationToken);
                results.Add(result);
            });
        }

        // Assert
        Assert.Equal([SharedLinkAccessResult.Allowed, SharedLinkAccessResult.Allowed, SharedLinkAccessResult.UsedUp], results);

        await context.UsingTenantScopeAsync(async scope =>
        {
            var stored = await scope.ServiceProvider.GetRequiredService<ISharedLinkManager>().GetAsync(link.LinkId, TestContext.Current.CancellationToken);
            Assert.Equal(2, stored.Uses);
            Assert.NotNull(stored.LastUsedUtc);
        });
    }

    [Fact]
    public async Task ListLinks_Search_MatchesTheTitleOrTheSourceName()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var now = DateTime.UtcNow;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            await session.SaveAsync(new SharedLink { LinkId = "report", Kind = "Test", ResourceId = "1", Title = "report.xlsx", SourceName = "Other", CreatedUtc = now.AddMinutes(-2), ExpiresUtc = now.AddDays(1) }, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveAsync(new SharedLink { LinkId = "orders", Kind = "Test", ResourceId = "2", Title = "data.csv", SourceName = "Orders export", CreatedUtc = now.AddMinutes(-1), ExpiresUtc = now.AddDays(1) }, cancellationToken: TestContext.Current.CancellationToken);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<ISharedLinkManager>();

            // Act
            var (byTitle, _) = await manager.ListAsync("REPORT", 0, 10, TestContext.Current.CancellationToken);
            var (bySource, _) = await manager.ListAsync("orders", 0, 10, TestContext.Current.CancellationToken);
            var (all, count) = await manager.ListAsync(null, 0, 10, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(["report"], byTitle.Select(link => link.LinkId));
            Assert.Equal(["orders"], bySource.Select(link => link.LinkId));
            Assert.Equal(["orders", "report"], all.Select(link => link.LinkId));
            Assert.Equal(2, count);
        });
    }

    [Fact]
    public async Task ShareDownloadLink_Files_SharesEachFileWithTheRecipients()
    {
        // Arrange
        var manager = new FakeSharedLinkManager();
        var settings = new ShareDownloadLinkStepSettings { Recipients = ["alice", "bob@example.com"], ExpiresAfterDays = 3, MaxDownloads = 5 };
        using var files = new TemporaryFolder();
        using var host = new StepTestHost(new ShareDownloadLinkStep(new PassThroughStringLocalizer<ShareDownloadLinkStep>()), settings, Services(manager, files.Path));
        host.WithFiles(host.CreateFile("report.csv", "x"), host.CreateFile("report.xlsx", "y"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, manager.Requests.Count);
        Assert.All(manager.Requests, request => Assert.Equal(DataPipelineFileLinkHandler.LinkKind, request.Kind));
        Assert.All(manager.Requests, request => Assert.Equal(["alice", "bob@example.com"], request.Recipients));
        Assert.All(manager.Requests, request => Assert.Equal(TimeSpan.FromDays(3), request.Lifetime));
        Assert.All(manager.Requests, request => Assert.Equal(5, request.MaxUses));
        Assert.All(manager.Requests, request => Assert.True(request.NotifyByEmail));
        Assert.Equal(["report.csv", "report.xlsx"], manager.Requests.Select(request => request.Title));

        // Each file was copied, out of the run's folder, for its link.
        Assert.All(manager.Requests, request => Assert.Single(Directory.GetFiles(Path.Combine(files.Path, "Sites", "Default", "DataPipelines", "SharedFiles", request.ResourceId))));
        Assert.Equal(2, host.Run.Deliveries.Count);
        Assert.All(host.Run.Deliveries, delivery => Assert.DoesNotContain("token", delivery.Description + delivery.Url, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(host.Log, entry => entry.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task ShareDownloadLink_RecipientNotEmailed_WarnsWithTheirName()
    {
        // Arrange
        var manager = new FakeSharedLinkManager { NotEmailed = ["bob"] };
        var settings = new ShareDownloadLinkStepSettings { Recipients = ["alice", "bob"] };
        using var files = new TemporaryFolder();
        using var host = new StepTestHost(new ShareDownloadLinkStep(new PassThroughStringLocalizer<ShareDownloadLinkStep>()), settings, Services(manager, files.Path));
        host.WithFiles(host.CreateFile("report.csv", "x"));

        // Act
        await host.ExecuteAsync(TestContext.Current.CancellationToken);

        // Assert
        var warning = Assert.Single(host.Log, entry => entry.Level == LogLevel.Warning);
        Assert.Contains("bob", warning.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("alice", warning.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShareDownloadLink_NoRecipients_ReportsError()
    {
        // Arrange
        using var files = new TemporaryFolder();
        using var host = new StepTestHost(new ShareDownloadLinkStep(new PassThroughStringLocalizer<ShareDownloadLinkStep>()), new ShareDownloadLinkStepSettings(), Services(new FakeSharedLinkManager(), files.Path));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    [Fact]
    public async Task FileLinkHandler_DeletedLink_DeletesTheCopyOfTheFile()
    {
        // Arrange
        using var files = new TemporaryFolder();
        using var services = Services(new FakeSharedLinkManager(), files.Path);
        using var host = new StepTestHost(new ShareDownloadLinkStep(new PassThroughStringLocalizer<ShareDownloadLinkStep>()), new ShareDownloadLinkStepSettings(), services);
        var handler = services.GetRequiredService<DataPipelineFileLinkHandler>();
        var resourceId = await handler.StoreAsync(host.CreateFile("report.csv", "x"), TestContext.Current.CancellationToken);
        var directory = Path.Combine(files.Path, "Sites", "Default", "DataPipelines", "SharedFiles", resourceId);

        // Act
        await handler.DeletedAsync(new SharedLink { ResourceId = resourceId, Title = "report.csv" });

        // Assert
        Assert.False(Directory.Exists(directory));
    }

    private static (SharedLink Link, string Token) Share(string userId)
    {
        var token = SharedLinkAccess.CreateToken();

        return (new SharedLink
        {
            LinkId = Guid.NewGuid().ToString("n")[..26],
            Kind = "Test",
            ResourceId = "resource",
            Title = "report.csv",
            TokenHash = SharedLinkAccess.Hash(token),
            RecipientUserIds = [userId],
            CreatedUtc = _now,
            ExpiresUtc = DateTime.UtcNow.AddDays(7),
        }, token);
    }

    private static ClaimsPrincipal User(string userId)
    {
        return new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "Test"));
    }

    private static ServiceProvider Services(ISharedLinkManager manager, string appDataPath)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddSingleton(manager);
        services.AddSingleton(Options.Create(new ShellOptions { ShellsApplicationDataPath = appDataPath, ShellsContainerName = "Sites" }));
        services.AddSingleton(new ShellSettings { Name = "Default" });
        services.AddSingleton<IIdGenerator, global::OrchardCore.Entities.DefaultIdGenerator>();
        services.AddSingleton<DataPipelineFileLinkHandler>();

        return services.BuildServiceProvider();
    }

    private static async Task<BlogContext> CreateContextAsync()
    {
        var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync()).Where(feature => feature.Id == "OrchardCore.DataPipelines").ToList();
            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        return context;
    }

    private sealed class TemporaryFolder : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "oc-shared-links-" + Guid.NewGuid().ToString("n"));

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private sealed class FakeSharedLinkManager : ISharedLinkManager
    {
        public List<SharedLinkRequest> Requests { get; } = [];

        // The recipients the link could not be emailed to.
        public List<string> NotEmailed { get; init; } = [];

        public Task<SharedLinkResult> CreateAsync(SharedLinkRequest request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            return Task.FromResult(new SharedLinkResult
            {
                Link = new SharedLink { LinkId = Guid.NewGuid().ToString("n"), Title = request.Title, RecipientNames = [.. request.Recipients], ExpiresUtc = _now.Add(request.Lifetime) },
                Url = "https://example.com/SharedLinks/x?token=secret",
                Notified = request.Recipients.Except(NotEmailed).ToList(),
            });
        }

        public Task<SharedLink> GetAsync(string linkId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<(SharedLinkAccessResult Result, SharedLink Link)> UseAsync(string linkId, string token, ClaimsPrincipal user, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task RevokeAsync(SharedLink link, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task DeleteAsync(SharedLink link, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<(IReadOnlyList<SharedLink> Links, int Count)> ListAsync(string search, int skip, int take, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
