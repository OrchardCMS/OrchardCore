using System.Diagnostics;
using Npgsql;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class PostgresConnectionPoolTests
{
    [Fact]
    public async Task DisposeAsync_IsolatedHosts_ReleasesOnlyDisposedHostConnections()
    {
        if (!OrchardTestServer.IsPostgresConfigured)
        {
            Assert.Skip("Postgres is not configured. Set OrchardCore__DatabaseProvider and OrchardCore__ConnectionString to run this test.");
        }

        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var contentRoot = Path.Combine(projectRoot, "src", "OrchardCore.Cms.Web");
        var appDataRoot = Path.Combine(contentRoot, $"App_Data_Tests_PoolLifecycle_{Guid.NewGuid():N}");
        string otherConnectionString = null;

        try
        {
            await using var otherServer = await OrchardTestServer.StartCmsAsync(
                contentRoot,
                Path.Combine(appDataRoot, "Other"),
                $"PoolOther_{Guid.NewGuid():N}",
                builder => otherConnectionString = builder.Configuration["OrchardCore:ConnectionString"]);

            var otherBackend = await GetPooledBackendAsync(otherConnectionString);

            for (var i = 0; i < 3; i++)
            {
                string connectionString = null;
                await using var server = await OrchardTestServer.StartCmsAsync(
                    contentRoot,
                    Path.Combine(appDataRoot, $"Host{i}"),
                    $"PoolHost_{Guid.NewGuid():N}",
                    builder => connectionString = builder.Configuration["OrchardCore:ConnectionString"]);

                // Database validation and YesSql use this connection string's process-wide pool.
                await GetPooledBackendAsync(connectionString);

                var observerConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
                {
                    Database = "postgres",
                    Pooling = false,
                };
                await using var observer = new NpgsqlConnection(observerConnectionString.ConnectionString);
                await observer.OpenAsync(TestContext.Current.CancellationToken);

                Assert.True(await CountConnectionsAsync(observer, connectionString) > 0);

                await server.DisposeAsync();

                var stopwatch = Stopwatch.StartNew();
                var remainingConnections = await CountConnectionsAsync(observer, connectionString);
                while (remainingConnections > 0 && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
                {
                    await Task.Delay(50, TestContext.Current.CancellationToken);
                    remainingConnections = await CountConnectionsAsync(observer, connectionString);
                }

                Assert.Equal(0L, remainingConnections);
                Assert.Equal(otherBackend, await GetPooledBackendAsync(otherConnectionString));
            }

            otherServer.AssertNoLoggedIssues();
        }
        finally
        {
            if (Directory.Exists(appDataRoot))
            {
                Directory.Delete(appDataRoot, recursive: true);
            }
        }
    }

    private static async Task<int> GetPooledBackendAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        return connection.ProcessID;
    }

    private static async Task<long> CountConnectionsAsync(NpgsqlConnection observer, string connectionString)
    {
        await using var command = observer.CreateCommand();
        command.CommandText = "SELECT count(*) FROM pg_stat_activity WHERE datname = @database AND backend_type = 'client backend'";
        command.Parameters.AddWithValue("database", new NpgsqlConnectionStringBuilder(connectionString).Database);
        return (long)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);
    }
}
