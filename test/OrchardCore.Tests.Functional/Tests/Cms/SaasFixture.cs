using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class SaasFixture : IAsyncLifetime
{
    // xunit creates one SaasFixture instance per consuming test class (SaasTests,
    // TenantsBulkSelectTests, ...). A shared instanceId would point them all at the
    // same App_Data directory and race on Directory.Delete/SQLite files when their
    // fixtures run concurrently, so each instance needs its own, like CmsRecipeFixture.
    private static int _instanceCounter;
    private readonly OrchardTestFixture _testFixture = new(instanceId: $"{nameof(SaasFixture)}_{Interlocked.Increment(ref _instanceCounter)}");

    public IBrowser Browser => _testFixture.Browser;
    public string BaseUrl => _testFixture.BaseUrl;
    public TenantInfo Tenant { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await _testFixture.InitializeAsync();

        var page = await CreatePageAsync();
        try
        {
            await page.GotoAsync("/");

            if (await page.Locator("#SiteName").CountAsync() > 0)
            {
                await page.SiteSetupAsync(new TenantInfo
                {
                    Name = "Testing SaaS",
                    Prefix = string.Empty,
                    SetupRecipe = "SaaS",
                });
            }

            // Create a test tenant to verify multi-tenancy.
            Tenant = TestUtils.GenerateTenantInfo("SaaS");
            await page.LoginAsync();
            await page.SetPageSizeAsync(string.Empty, "100");
            await page.CreateTenantAsync(Tenant);
            await page.VisitTenantSetupPageAsync(Tenant);
            await page.SiteSetupAsync(Tenant);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public void AssertNoLoggedIssues() => _testFixture.AssertNoLoggedIssues();

    public async Task<IPage> CreatePageAsync()
    {
        return await _testFixture.CreatePageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _testFixture.DisposeAsync();
    }
}
