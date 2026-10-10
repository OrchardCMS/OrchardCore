using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Tests.Modules.OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class SharedFileTests
{
    private static readonly DateTime _now = new(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CheckAccess_RecipientWithValidToken_IsAllowed()
    {
        // Arrange
        var (file, token) = Share("alice-id");

        // Act
        var result = DataPipelineSharedFileAccess.Check(file, token, User("alice-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.Allowed, result);
    }

    [Fact]
    public void CheckAccess_UserWhoIsNotARecipient_IsDenied()
    {
        // Arrange
        var (file, token) = Share("alice-id");

        // Act
        var result = DataPipelineSharedFileAccess.Check(file, token, User("mallory-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.NotARecipient, result);
    }

    [Fact]
    public void CheckAccess_WrongToken_IsDenied()
    {
        // Arrange
        var (file, _) = Share("alice-id");

        // Act
        var result = DataPipelineSharedFileAccess.Check(file, DataPipelineSharedFileAccess.CreateToken(), User("alice-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.InvalidToken, result);
    }

    [Fact]
    public void CheckAccess_ExpiredOrRevokedLink_IsDenied()
    {
        // Arrange
        var (file, token) = Share("alice-id");

        // Act
        var expired = DataPipelineSharedFileAccess.Check(file, token, User("alice-id"), file.ExpiresUtc.AddSeconds(1));
        file.RevokedUtc = _now;
        var revoked = DataPipelineSharedFileAccess.Check(file, token, User("alice-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.Expired, expired);
        Assert.Equal(DataPipelineSharedFileAccessResult.Expired, revoked);
    }

    [Fact]
    public void CreateToken_EveryCall_IsUniqueAndOnlyItsHashIsStored()
    {
        // Act
        var first = DataPipelineSharedFileAccess.CreateToken();
        var second = DataPipelineSharedFileAccess.CreateToken();
        var hash = DataPipelineSharedFileAccess.Hash(first);

        // Assert
        Assert.NotEqual(first, second);
        Assert.True(first.Length >= 40);
        Assert.Equal(64, hash.Length);
        Assert.DoesNotContain(first, hash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ShareDownloadLink_Files_SharesEachFileWithTheRecipients()
    {
        // Arrange
        var manager = new FakeSharedFileManager();
        var settings = new ShareDownloadLinkStepSettings { Recipients = ["alice", "bob@example.com"], ExpiresAfterDays = 3 };
        using var host = new StepTestHost(new ShareDownloadLinkStep(new PassThroughStringLocalizer<ShareDownloadLinkStep>()), settings, Services(manager));
        host.WithFiles(host.CreateFile("report.csv", "x"), host.CreateFile("report.xlsx", "y"));

        // Act
        await host.ExecuteAsync();

        // Assert
        Assert.Equal(2, manager.Shared.Count);
        Assert.All(manager.Shared, shared => Assert.Equal(["alice", "bob@example.com"], shared.Recipients));
        Assert.All(manager.Shared, shared => Assert.Equal(TimeSpan.FromDays(3), shared.Lifetime));
        Assert.Equal(2, host.Run.Deliveries.Count);
        Assert.All(host.Run.Deliveries, delivery => Assert.DoesNotContain("token", delivery.Description + delivery.Url, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ShareDownloadLink_NoRecipients_ReportsError()
    {
        // Arrange
        using var host = new StepTestHost(new ShareDownloadLinkStep(new PassThroughStringLocalizer<ShareDownloadLinkStep>()), new ShareDownloadLinkStepSettings(), Services(new FakeSharedFileManager()));

        // Act
        var description = await host.DescribeAsync();

        // Assert
        Assert.Contains(description.Issues, issue => issue.Severity == DataPipelineIssueSeverity.Error);
    }

    private static (DataPipelineSharedFile File, string Token) Share(string userId)
    {
        var token = DataPipelineSharedFileAccess.CreateToken();

        return (new DataPipelineSharedFile
        {
            FileId = "file",
            TokenHash = DataPipelineSharedFileAccess.Hash(token),
            RecipientUserIds = [userId],
            CreatedUtc = _now,
            ExpiresUtc = _now.AddDays(7),
        }, token);
    }

    private static ClaimsPrincipal User(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "Test"));

    private static ServiceProvider Services(IDataPipelineSharedFileManager manager)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddSingleton(manager);

        return services.BuildServiceProvider();
    }

    private sealed class FakeSharedFileManager : IDataPipelineSharedFileManager
    {
        public List<(string FileName, IReadOnlyList<string> Recipients, TimeSpan Lifetime)> Shared { get; } = [];

        public Task<DataPipelineSharedFileResult> ShareAsync(DataPipelineFile file, DataPipelineSharedFileRequest request, CancellationToken cancellationToken = default)
        {
            Shared.Add((file.FileName, request.Recipients, request.Lifetime));

            return Task.FromResult(new DataPipelineSharedFileResult
            {
                SharedFile = new DataPipelineSharedFile { FileId = Guid.NewGuid().ToString("n"), FileName = file.FileName, RecipientNames = [.. request.Recipients], ExpiresUtc = _now.Add(request.Lifetime) },
                Url = "https://example.com/DataPipelines/Files/x?token=secret",
            });
        }
    }
}
