using System.Diagnostics;
using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestFixture : IAsyncDisposable
{
    private Process _serverProcess;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private bool _disposed;

    public string BaseUrl { get; private set; }
    public IBrowser Browser => _browser;

    private static string ProjectRoot
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static bool IsMvc
        => Environment.GetEnvironmentVariable("ORCHARD_APP") == "mvc";

    private static string AppDir
        => IsMvc
            ? Path.Combine(ProjectRoot, "src", "OrchardCore.Mvc.Web")
            : Path.Combine(ProjectRoot, "src", "OrchardCore.Cms.Web");

    private static string Assembly
        => IsMvc ? "OrchardCore.Mvc.Web.dll" : "OrchardCore.Cms.Web.dll";

    public async Task InitializeAsync()
    {
        BaseUrl = Environment.GetEnvironmentVariable("ORCHARD_URL") ?? "http://localhost:5000";

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ORCHARD_EXTERNAL")))
        {
            AppLifecycleHelper.DeleteAppData(AppDir);

            if (!IsMvc)
            {
                AppLifecycleHelper.CopyMigrationsRecipe(AppDir);
            }

            _serverProcess = AppLifecycleHelper.HostApp(AppDir, Assembly);
            await AppLifecycleHelper.WaitForReadyAsync(BaseUrl, 30_000);
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
    }

    public async Task<IPage> CreatePageAsync()
    {
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = BaseUrl,
        });

        return await context.NewPageAsync();
    }

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

        if (_serverProcess is not null)
        {
            AppLifecycleHelper.KillApp(_serverProcess);
        }

        if (!IsMvc)
        {
            AppLifecycleHelper.DeleteMigrationsRecipe(
                Path.Combine(ProjectRoot, "src", "OrchardCore.Cms.Web"));
        }
    }
}
