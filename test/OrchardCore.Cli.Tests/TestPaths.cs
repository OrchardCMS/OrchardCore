namespace OrchardCore.Cli.Tests;

internal static class TestPaths
{
    public static string CreateScratchDirectory(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestScratch", name, Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(path);
        return path;
    }
}

internal sealed class UnsupportedCredentialStore : ICredentialStore
{
    public string DisplayName => "unsupported";

    public Task SaveAsync(string contextName, StoredToken token, CancellationToken cancellationToken) =>
        throw new CliException("Credentials can't be stored in this test.");

    public Task<StoredToken?> GetAsync(string contextName, CancellationToken cancellationToken) => Task.FromResult<StoredToken?>(null);

    public Task<bool> DeleteAsync(string contextName, CancellationToken cancellationToken) => Task.FromResult(false);
}
