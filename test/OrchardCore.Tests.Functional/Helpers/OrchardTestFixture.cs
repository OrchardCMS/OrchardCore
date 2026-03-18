using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestFixture : IAsyncDisposable
{
    // Shared server state keyed by app type (CMS vs MVC) to prevent parallel init races
    // while allowing different app types to run their own servers.
    private static readonly SemaphoreSlim _serverLock = new(1, 1);
    private static readonly Dictionary<string, SharedServerState> _servers = [];

    private readonly bool _isMvc;
    private readonly int _port;

    private IPlaywright _playwright;
    private IBrowser _browser;
    private bool _disposed;

    public string BaseUrl { get; private set; }
    public IBrowser Browser => _browser;

    public OrchardTestFixture(bool isMvc = false, int port = 5000)
    {
        _isMvc = isMvc;
        _port = port;
    }

    private static string ProjectRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private string AppDir =>
        _isMvc
            ? Path.Combine(ProjectRoot, "src", "OrchardCore.Mvc.Web")
            : Path.Combine(ProjectRoot, "src", "OrchardCore.Cms.Web");

    private string Assembly => _isMvc ? "OrchardCore.Mvc.Web.dll" : "OrchardCore.Cms.Web.dll";

    private string ServerKey => _isMvc ? "mvc" : "cms";

    public async Task InitializeAsync()
    {
        await _serverLock.WaitAsync();
        try
        {
            if (!_servers.TryGetValue(ServerKey, out var state))
            {
                state = new SharedServerState();
                _servers[ServerKey] = state;
            }

            state.RefCount++;

            if (state.BaseUrl == null)
            {
                state.BaseUrl = Environment.GetEnvironmentVariable("ORCHARD_URL")
                    ?? $"http://localhost:{_port}";

                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ORCHARD_EXTERNAL")))
                {
                    AppLifecycleHelper.DeleteAppData(AppDir);

                    if (!_isMvc)
                    {
                        CopyRecipeIfNeeded(state, "migrations.recipe.json");
                    }

                    state.ServerProcess = AppLifecycleHelper.HostApp(AppDir, Assembly, state.BaseUrl);
                    await AppLifecycleHelper.WaitForReadyAsync(state.BaseUrl, 30_000);
                }
            }

            BaseUrl = state.BaseUrl;
        }
        finally
        {
            _serverLock.Release();
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions { Headless = true }
        );
    }

    public async Task<IPage> CreatePageAsync()
    {
        var context = await _browser.NewContextAsync(
            new BrowserNewContextOptions { BaseURL = BaseUrl }
        );

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

        await _serverLock.WaitAsync();
        try
        {
            if (!_servers.TryGetValue(ServerKey, out var state))
            {
                return;
            }

            state.RefCount--;

            if (state.RefCount > 0)
            {
                return;
            }

            if (state.ServerProcess is not null)
            {
                AppLifecycleHelper.KillApp(state.ServerProcess);
            }

            if (!_isMvc)
            {
                foreach (var recipe in state.CopiedRecipes)
                {
                    AppLifecycleHelper.DeleteRecipe(AppDir, recipe);
                }
            }

            _servers.Remove(ServerKey);
        }
        finally
        {
            _serverLock.Release();
        }
    }

    private void CopyRecipeIfNeeded(SharedServerState state, string recipeFileName)
    {
        if (AppLifecycleHelper.CopyRecipe(AppDir, recipeFileName))
        {
            state.CopiedRecipes.Add(recipeFileName);
        }
    }

    private sealed class SharedServerState
    {
        public Process ServerProcess;
        public string BaseUrl;
        public int RefCount;
        public readonly List<string> CopiedRecipes = [];
    }
}
