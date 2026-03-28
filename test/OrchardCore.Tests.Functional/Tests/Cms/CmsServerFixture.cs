using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

/// <summary>
/// Shared CMS server used by all recipe-based test classes.
/// Starts ONE CMS server with the SaaS recipe (enabling multi-tenancy),
/// then creates all child tenants upfront during initialization.
/// Uses lazy async initialization so the first fixture to access it triggers setup.
/// </summary>
public static class CmsServer
{
    /// <summary>
    /// All recipe tenants to create during initialization.
    /// </summary>
    private static readonly string[] _recipes = ["Agency", "Blog", "ComingSoon", "Headless", "Migrations"];

    private static readonly SemaphoreSlim _initLock = new(1, 1);
    private static OrchardTestFixture _testFixture;
    private static Dictionary<string, string> _tenantPrefixes;
    private static Exception _initError;
    private static int _refCount;

    public static IBrowser Browser => _testFixture?.Browser;
    public static string BaseUrl => _testFixture?.BaseUrl;

    /// <summary>
    /// Gets the URL prefix for a recipe tenant (e.g., "blog" for the Blog recipe).
    /// </summary>
    public static string GetPrefix(string recipeName) => _tenantPrefixes[recipeName];

    /// <summary>
    /// Ensures the shared server is initialized. Safe to call from multiple fixtures concurrently.
    /// </summary>
    public static async Task AcquireAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            _refCount++;
            if (_testFixture is not null)
            {
                if (_initError is not null)
                {
                    throw new InvalidOperationException("CmsServer initialization failed previously.", _initError);
                }

                return;
            }

            var fixture = new OrchardTestFixture(instanceId: nameof(CmsServer), autoSetupRecipe: "SaaS");
            _tenantPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                await fixture.InitializeAsync();
            }
            catch (Exception ex)
            {
                _testFixture = fixture; // Mark as attempted so we don't retry.
                _initError = ex;
                throw;
            }

            _testFixture = fixture;

            // Trigger AutoSetup for the Default/SaaS tenant and create all recipe tenants.
            // Use a longer timeout for initialization since concurrent server startups
            // (e.g., MVC) can cause resource contention.
            var page = await _testFixture.CreatePageAsync();
            page.SetDefaultTimeout(60_000);
            try
            {
                await page.GotoAsync("/");
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                await page.LoginAsync();
                await page.SetPageSizeAsync(string.Empty, "100");

                foreach (var recipe in _recipes)
                {
                    var prefix = recipe.ToLowerInvariant();
                    var tenant = new TenantInfo
                    {
                        Name = prefix,
                        Prefix = prefix,
                        SetupRecipe = recipe,
                        TablePrefix = prefix,
                    };

                    await page.CreateTenantAsync(tenant);
                    await page.VisitTenantSetupPageAsync(tenant);
                    await page.SiteSetupAsync(tenant);
                    _tenantPrefixes[recipe] = prefix;
                }
            }
            finally
            {
                await page.CloseAsync();
            }
        }
        finally
        {
            _initLock.Release();
        }
    }

    public static async Task<IPage> CreatePageAsync()
    {
        return await _testFixture.CreatePageAsync();
    }

    public static void AssertNoLoggedIssues() => _testFixture?.AssertNoLoggedIssues();

    public static async Task ReleaseAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            _refCount--;
            if (_refCount <= 0 && _testFixture is not null)
            {
                await _testFixture.DisposeAsync();
                _testFixture = null;
                _tenantPrefixes = null;
            }
        }
        finally
        {
            _initLock.Release();
        }
    }
}
