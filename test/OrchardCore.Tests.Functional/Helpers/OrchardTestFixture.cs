using System.Diagnostics;
using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestFixture : IAsyncDisposable
{
    private Process _serverProcess;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private bool _disposed;
    private bool _migrationsRecipeCopied;
    private bool _mediaRecipeCopied;
    private bool _mediaTusRecipeCopied;
    private bool _mediaTusRedisRecipeCopied;
    private bool _mediaTusAzureRecipeCopied;
    private bool _mediaTusRedisAzureRecipeCopied;

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
                _migrationsRecipeCopied = AppLifecycleHelper.CopyMigrationsRecipe(AppDir);
                _mediaRecipeCopied = AppLifecycleHelper.CopyRecipe(AppDir, "media.recipe.json");
                _mediaTusRecipeCopied = AppLifecycleHelper.CopyRecipe(AppDir, "media-tus.recipe.json");
                _mediaTusRedisRecipeCopied = AppLifecycleHelper.CopyRecipe(AppDir, "media-tus-redis.recipe.json");
                _mediaTusAzureRecipeCopied = AppLifecycleHelper.CopyRecipe(AppDir, "media-tus-azure.recipe.json");
                _mediaTusRedisAzureRecipeCopied = AppLifecycleHelper.CopyRecipe(AppDir, "media-tus-redis-azure.recipe.json");
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

        var page = await context.NewPageAsync();

        page.Close += async (_, _) =>
        {
            try
            {
                await context.CloseAsync();
            }
            catch (Exception)
            {
                // Context may already be closed (e.g., when the browser is being disposed).
            }
        };

        return page;
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

        var cmsAppDir = Path.Combine(ProjectRoot, "src", "OrchardCore.Cms.Web");

        if (!IsMvc && _migrationsRecipeCopied)
        {
            AppLifecycleHelper.DeleteMigrationsRecipe(cmsAppDir);
        }

        if (!IsMvc && _mediaRecipeCopied)
        {
            AppLifecycleHelper.DeleteRecipe(cmsAppDir, "media.recipe.json");
        }

        if (!IsMvc && _mediaTusRecipeCopied)
        {
            AppLifecycleHelper.DeleteRecipe(cmsAppDir, "media-tus.recipe.json");
        }

        if (!IsMvc && _mediaTusRedisRecipeCopied)
        {
            AppLifecycleHelper.DeleteRecipe(cmsAppDir, "media-tus-redis.recipe.json");
        }

        if (!IsMvc && _mediaTusAzureRecipeCopied)
        {
            AppLifecycleHelper.DeleteRecipe(cmsAppDir, "media-tus-azure.recipe.json");
        }

        if (!IsMvc && _mediaTusRedisAzureRecipeCopied)
        {
            AppLifecycleHelper.DeleteRecipe(cmsAppDir, "media-tus-redis-azure.recipe.json");
        }
    }
}
