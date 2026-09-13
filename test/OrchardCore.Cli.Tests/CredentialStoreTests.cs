namespace OrchardCore.Cli.Tests;

public class CredentialStoreTests
{
    [Fact]
    public async Task FileCredentialStore_SaveGetDelete_RoundTripsPlaintextToken()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(FileCredentialStore_SaveGetDelete_RoundTripsPlaintextToken)));
        var store = new FileCredentialStore(paths);
        var token = new StoredToken
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            Issuer = "https://example.com/",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
        };

        await store.SaveAsync("Primary", token, TestContext.Current.CancellationToken);

        var path = paths.GetCredentialFilePath("primary");
        var payload = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
        var stored = await store.GetAsync("PRIMARY", TestContext.Current.CancellationToken);

        Assert.Contains("access-token", payload, StringComparison.Ordinal);
        Assert.Contains("refresh-token", payload, StringComparison.Ordinal);
        Assert.NotNull(stored);
        Assert.Equal(token.AccessToken, stored.AccessToken);
        Assert.Equal(token.RefreshToken, stored.RefreshToken);
        Assert.True(await store.DeleteAsync("primary", TestContext.Current.CancellationToken));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task FileCredentialStore_SaveOnUnix_UsesOwnerOnlyPermissions()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(FileCredentialStore_SaveOnUnix_UsesOwnerOnlyPermissions)));
        var store = new FileCredentialStore(paths);
        await store.SaveAsync("primary", new StoredToken(), TestContext.Current.CancellationToken);

        Assert.Equal(
            UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
            File.GetUnixFileMode(paths.CredentialsDirectory));
        Assert.Equal(
            UnixFileMode.UserRead | UnixFileMode.UserWrite,
            File.GetUnixFileMode(paths.GetCredentialFilePath("primary")));
    }

    [Fact]
    public void CreateDefault_UsesPlatformDefault()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(CreateDefault_UsesPlatformDefault)));

        var store = CredentialStoreFactory.CreateDefault(paths);

        Assert.Equal(OperatingSystem.IsWindows() ? "windows-credential-manager" : "plaintext-file", store.DisplayName);
    }

    [Fact]
    public void CreateDefaultPaths_OnUnix_UsesOrchardCoreCredentialDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        var paths = CliPaths.CreateDefault();

        Assert.Equal(
            Path.Combine(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.UserProfile), ".orchardcore", "credentials"),
            paths.CredentialsDirectory);
    }
}
