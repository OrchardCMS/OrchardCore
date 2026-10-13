using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace OrchardCore.Tests.Functional.Helpers;

public sealed class OrchardTestFixture : IAsyncDisposable
{
    private static int s_traceCounter;

    private static readonly bool s_tracingEnabled =
        !string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("PLAYWRIGHT_TRACING"));

    private static readonly string s_traceDir =
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "traces");

    private readonly bool _isMvc;
    private readonly string _instanceId;
    private readonly Action<OrchardCoreBuilder> _configureOrchardCore;

    private OrchardTestServer _server;
    private IPlaywright _playwright;
    private IBrowser _browser;
    private bool _disposed;

    public string BaseUrl { get; private set; }
    public IBrowser Browser => _browser;

    public OrchardTestFixture(bool isMvc = false, string instanceId = null, Action<OrchardCoreBuilder> configureOrchardCore = null)
    {
        _isMvc = isMvc;
        _instanceId = instanceId;
        _configureOrchardCore = configureOrchardCore;
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

        if (string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("ORCHARD_EXTERNAL")))
        {
            _server = _isMvc
                ? await OrchardTestServer.StartMvcAsync(AppDir, AppDataPath)
                : await OrchardTestServer.StartCmsAsync(AppDir, AppDataPath, _instanceId, _configureOrchardCore);

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
            new BrowserNewContextOptions
            {
                BaseURL = BaseUrl,
                // Bootstrap 5's own CSS wraps every fade/collapse transition in a
                // `@media (prefers-reduced-motion: no-preference)` block (its built-in
                // accessibility opt-out) and its JS reads the computed transition-duration
                // before deciding whether to wait for `transitionend` at all. Emulating
                // `prefers-reduced-motion: reduce` here makes that duration compute to 0,
                // so modal/collapse show-hide becomes synchronous instead of racing a real
                // CSS transition - the actual root cause of modal-hide assertions
                // (ShortcodeModalTests, PredefinedListEditorTests) flaking intermittently on
                // GitHub Actions CI under heavy runner contention: raising the assertion/wait
                // timeout only made the race window bigger, it didn't remove the race.
                ReducedMotion = ReducedMotion.Reduce,
                Locale = "en-US",
            }
        );

        if (s_tracingEnabled)
        {
            await context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });
        }

        // sortable-menu.ts (the nested Menu/Taxonomy hierarchy drag-and-drop editor)
        // configures SortableJS with animation: 150, and SortableJS ignores dragover
        // events entirely while one of its own reorder animations is still running.
        // That 150ms of real interaction latency is purely cosmetic in production but
        // was the actual root cause behind TaxonomyHierarchyTests/MenuHierarchyTests
        // intermittently failing to ever reach the expected drop order under CI/CPU
        // contention (SortableMenuHelper's drag helpers had to retry-nudge the pointer
        // to progress the drag, and a retry cadence tight enough to be fast risked
        // landing mid-animation and being silently dropped, while a cadence loose
        // enough to always clear the animation made the whole polling loop slower to
        // reach its own attempt budget). Same fix shape as ReducedMotion above: remove
        // the latency at its source for every test page, rather than working around
        // its timing from the outside.
        //
        // Sortable is loaded as a plain global <script> (declared as a ResourceManager
        // dependency), and sortable-menu.ts's initSortableMenu() runs as an ES module
        // that executes as soon as the document finishes parsing - BEFORE
        // DOMContentLoaded fires, per the module-script spec - so by the time
        // DOMContentLoaded would fire, Sortable.create has already been called with
        // its real animation: 150. A property trap on `window.Sortable` itself, rather
        // than a DOMContentLoaded listener, is the only timing-independent way to patch
        // it exactly when the global is assigned, regardless of script load order.
        await context.AddInitScriptAsync(
            """
            (() => {
                let sortableValue;

                Object.defineProperty(window, 'Sortable', {
                    configurable: true,
                    get: () => sortableValue,
                    set: (value) => {
                        if (value && typeof value.create === 'function' && !value.__animationPatched) {
                            const originalCreate = value.create;
                            value.create = (el, options) => originalCreate(el, { ...options, animation: 0 });
                            value.__animationPatched = true;
                        }

                        sortableValue = value;
                    },
                });
            })();
            """);

        var page = await context.NewPageAsync();

        page.Close += async (_, _) =>
        {
            try
            {
                if (s_tracingEnabled)
                {
                    var traceIndex = Interlocked.Increment(ref s_traceCounter);
                    var fileName = string.IsNullOrEmpty(capturedTraceName)
                        ? $"trace-{traceIndex}.zip"
                        : $"trace-{capturedTraceName}-{traceIndex}.zip";

                    Directory.CreateDirectory(s_traceDir);

                    await context.Tracing.StopAsync(new TracingStopOptions
                    {
                        Path = Path.Combine(s_traceDir, fileName),
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

        // Path.GetInvalidFileNameChars() is OS-dependent - on Linux (the CI runner), '"' isn't
        // actually invalid for a filesystem path, so a trace file for a [Theory]/[InlineData]
        // test whose display name contains a quoted string argument (xUnit wraps string
        // parameters in literal quotes when building it, e.g. `(adminUrl: "/Admin/Templates", ...)`)
        // writes to disk just fine there. actions/upload-artifact enforces its own stricter,
        // cross-platform-safe character set on top of that regardless of OS though, so the same
        // name that saved locally can still fail the artifact upload step later - denylist that
        // full set explicitly instead of relying on the OS's (looser) rules.
        var invalidChars = Path.GetInvalidFileNameChars()
            .Concat(['"', ':', '<', '>', '|', '*', '?', '\u000D', '\u000A'])
            .Distinct()
            .ToArray();

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

        if (Directory.Exists(AppDataPath))
        {
            ClearSqlitePools();
            await DeleteAppDataDirectoryAsync();
        }
    }

    private void ClearSqlitePools()
    {
        global::Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        foreach (var dbPath in Directory.EnumerateFiles(AppDataPath, "*.db", SearchOption.AllDirectories))
        {
            global::Microsoft.Data.Sqlite.SqliteConnection.ClearPool(
                new global::Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}"));
        }
    }

    private async Task DeleteAppDataDirectoryAsync()
    {
        const int maxAttempts = 15;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(AppDataPath, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                ClearSqlitePools();
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt));
            }
        }
    }
}
