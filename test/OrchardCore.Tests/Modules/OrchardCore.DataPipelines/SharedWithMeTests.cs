using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

public sealed class SharedWithMeTests
{
    private static readonly DateTime _now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CheckAccess_RecipientWithoutToken_IsAllowed()
    {
        // Arrange
        var file = new DataPipelineSharedFile { TokenHash = DataPipelineSharedFileAccess.Hash(DataPipelineSharedFileAccess.CreateToken()), RecipientUserIds = ["alice-id"], ExpiresUtc = _now.AddDays(1) };

        // Act
        var result = DataPipelineSharedFileAccess.Check(file, token: null, User("alice-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.Allowed, result);
    }

    [Fact]
    public void CheckAccess_NotARecipientWithoutToken_IsDenied()
    {
        // Arrange
        var file = new DataPipelineSharedFile { TokenHash = DataPipelineSharedFileAccess.Hash(DataPipelineSharedFileAccess.CreateToken()), RecipientUserIds = ["alice-id"], ExpiresUtc = _now.AddDays(1) };

        // Act
        var result = DataPipelineSharedFileAccess.Check(file, token: null, User("mallory-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.NotARecipient, result);
    }

    [Fact]
    public void CheckAccess_EmptyToken_IsDenied()
    {
        // Arrange
        var file = new DataPipelineSharedFile { TokenHash = DataPipelineSharedFileAccess.Hash(DataPipelineSharedFileAccess.CreateToken()), RecipientUserIds = ["alice-id"], ExpiresUtc = _now.AddDays(1) };

        // Act
        var result = DataPipelineSharedFileAccess.Check(file, string.Empty, User("alice-id"), _now);

        // Assert
        Assert.Equal(DataPipelineSharedFileAccessResult.InvalidToken, result);
    }

    [Fact]
    public async Task ListSharedWith_User_ReturnsTheActiveFilesSharedWithThem()
    {
        // Arrange
        using var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync()).Where(feature => feature.Id == "OrchardCore.DataPipelines").ToList();
            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineSharedFileManager>();

            await manager.SaveAsync(SharedFile("active", ["alice-id", "bob-id"], _now.AddDays(1)));
            await manager.SaveAsync(SharedFile("expired", ["alice-id"], _now.AddDays(-1)));
            await manager.SaveAsync(SharedFile("revoked", ["alice-id"], _now.AddDays(1), revokedUtc: _now.AddHours(-1)));
            await manager.SaveAsync(SharedFile("bob-only", ["bob-id"], _now.AddDays(1)));
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<DataPipelineSharedFileManager>();

            // Act
            var alice = await manager.ListSharedWithAsync("alice-id", _now);
            var bob = await manager.ListSharedWithAsync("bob-id", _now);
            var nobody = await manager.ListSharedWithAsync(null, _now);

            // Assert
            Assert.Equal(["active"], alice.Select(file => file.FileId));
            Assert.Equal(["bob-only", "active"], bob.Select(file => file.FileId));
            Assert.Empty(nobody);
        });
    }

    private static DataPipelineSharedFile SharedFile(string fileId, List<string> recipients, DateTime expiresUtc, DateTime? revokedUtc = null)
        => new()
        {
            FileId = fileId,
            FileName = $"{fileId}.csv",
            TokenHash = DataPipelineSharedFileAccess.Hash(DataPipelineSharedFileAccess.CreateToken()),
            RecipientUserIds = recipients,
            CreatedUtc = fileId == "bob-only" ? _now.AddMinutes(-1) : _now.AddMinutes(-10),
            ExpiresUtc = expiresUtc,
            RevokedUtc = revokedUtc,
        };

    private static ClaimsPrincipal User(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "Test"));
}
