using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional;

public sealed class CmsSetupFixture : IAsyncLifetime
{
    private static int _instanceCounter;

    // IClassFixture creates one instance per test class and classes run in parallel, so each
    // instance needs its own App_Data directory — InitializeAsync deletes it, which would
    // otherwise wipe the data of another class's already-running server.
    private readonly OrchardTestFixture _testFixture = new(
        instanceId: $"{nameof(CmsSetupFixture)}_{Interlocked.Increment(ref _instanceCounter)}");

    public IBrowser Browser => _testFixture.Browser;
    public string BaseUrl => _testFixture.BaseUrl;

    public async ValueTask InitializeAsync()
    {
        await _testFixture.InitializeAsync();

        // Perform the default SaaS tenant setup.
        var page = await CreatePageAsync();
        try
        {
            await page.GotoAsync("/");

            // Only run setup if the setup page is present (skipped when using an externally hosted site).
            if (await page.Locator("#SiteName").CountAsync() > 0)
            {
                await TenantHelper.SiteSetupAsync(page, new TenantInfo
                {
                    Name = "Testing SaaS",
                    Prefix = string.Empty,
                    SetupRecipe = "SaaS",
                });
            }

            await AuthHelper.LoginAsync(page);
            await ConfigurationHelper.SetPageSizeAsync(page, string.Empty, "100");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async Task<IPage> CreatePageAsync()
    {
        return await _testFixture.CreatePageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _testFixture.DisposeAsync();
    }
}
