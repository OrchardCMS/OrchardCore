extern alias CmsWeb;
extern alias MvcWeb;

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

    private IDisposable _factory;
    private Action _assertNoLoggedErrors;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private bool _disposed;

    public string BaseUrl { get; private set; }
    public IBrowser Browser => _browser;

    private readonly string _instanceId;

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
            if (_isMvc)
            {
                var factory = new OrchardWebApplicationFactory<MvcWeb::Program>(AppDataPath);
                _ = factory.Services;
                BaseUrl = factory.ServerAddress;
                _factory = factory;
                _assertNoLoggedErrors = factory.AssertNoLoggedErrors;
            }
            else
            {
                var factory = new OrchardWebApplicationFactory<CmsWeb::Program>(AppDataPath);
                _ = factory.Services;
                BaseUrl = factory.ServerAddress;
                _factory = factory;
                _assertNoLoggedErrors = factory.AssertNoLoggedErrors;
            }
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
        var context = await _browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = BaseUrl }
        );

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
                    var fileName = string.IsNullOrEmpty(traceName)
                        ? $"trace-{traceIndex}.zip"
                        : $"trace-{traceName}-{traceIndex}.zip";

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

    public void AssertNoLoggedErrors() => _assertNoLoggedErrors?.Invoke();

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
        _factory?.Dispose();

        // Clean up copied recipes.
        if (!_isMvc)
        {
            AppLifecycleHelper.DeleteRecipe(AppDir, "migrations.recipe.json");
        }
    }
}
