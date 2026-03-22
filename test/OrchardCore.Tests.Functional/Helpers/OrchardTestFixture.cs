using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestFixture : IAsyncDisposable
{
    private static int _traceCounter;

    private static readonly bool _tracingEnabled =
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PLAYWRIGHT_TRACING"));

    private static readonly string _traceDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "traces");

    private readonly bool _isMvc;
    private readonly string _instanceId;

    private OrchardTestServer _server;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private bool _disposed;

    public string BaseUrl { get; private set; }
    public IBrowser Browser => _browser;

    public OrchardTestFixture(bool isMvc = false, string instanceId = null)
    {
        _isMvc = isMvc;
        _instanceId = instanceId;
    }

    private static string ProjectRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private string AppDir =>
        _isMvc
            ? Path.Combine(ProjectRoot, "src", "OrchardCore.Mvc.Web")
            : Path.Combine(ProjectRoot, "src", "OrchardCore.Cms.Web");

    private string AppDataPath =>
        string.IsNullOrEmpty(_instanceId)
            ? Path.Combine(AppDir, "App_Data_Tests")
            : Path.Combine(AppDir, $"App_Data_Tests_{_instanceId}");

    public async Task InitializeAsync()
    {
        // Clean previous test data.
        if (Directory.Exists(AppDataPath))
        {
            Directory.Delete(AppDataPath, recursive: true);
        }

        // Copy test recipes if needed.
        if (!_isMvc)
        {
            AppLifecycleHelper.CopyRecipe(AppDir, "migrations.recipe.json");
        }

        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("ORCHARD_EXTERNAL")))
        {
            _server = _isMvc
                ? await OrchardTestServer.StartMvcAsync(AppDir, AppDataPath)
                : await OrchardTestServer.StartCmsAsync(AppDir, AppDataPath, _instanceId);

            BaseUrl = _server.ServerAddress;
        }
        else
        {
            BaseUrl = System.Environment.GetEnvironmentVariable("ORCHARD_URL")
                ?? "http://localhost:5000";
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true }
        );
    }

    public async Task<IPage> CreatePageAsync(string traceName = null)
    {
        // Capture the test display name at call time (inside the test) so it can be used
        // when the page close event fires (potentially after the test has completed).
        var capturedTraceName = traceName ?? SanitizeFileName(TestContext.Current?.Test?.TestDisplayName);

        var context = await _browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = BaseUrl }
        );

        context.SetDefaultTimeout(60_000);

        if (_tracingEnabled)
        {
            await context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });
        }

        var page = await context.NewPageAsync();

        page.Close += async (_, _) =>
        {
            try
            {
                if (_tracingEnabled)
                {
                    var traceIndex = Interlocked.Increment(ref _traceCounter);
                    var fileName = string.IsNullOrEmpty(capturedTraceName)
                        ? $"trace-{traceIndex}.zip"
                        : $"trace-{capturedTraceName}-{traceIndex}.zip";

                    Directory.CreateDirectory(_traceDir);

                    await context.Tracing.StopAsync(new TracingStopOptions
                    {
                        Path = Path.Combine(_traceDir, fileName),
                    });
                }

                await context.CloseAsync();
            }
            catch (Exception)
            {
                // Context may already be closed (e.g., when the browser is being disposed).
            }
        };

        return page;
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => Array.IndexOf(invalidChars, c) >= 0 ? '_' : c));
    }

    public void AssertNoLoggedIssues() => _server?.AssertNoLoggedIssues();

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (_server is not null)
        {
            await _server.DisposeAsync();
        }

        // Clean up copied recipes.
        if (!_isMvc)
        {
            AppLifecycleHelper.DeleteRecipe(AppDir, "migrations.recipe.json");
        }

        // Clear SQLite connection pool to release file locks, then clean up test data.
        global::Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (Directory.Exists(AppDataPath))
        {
            Directory.Delete(AppDataPath, recursive: true);
        }
    }
}
