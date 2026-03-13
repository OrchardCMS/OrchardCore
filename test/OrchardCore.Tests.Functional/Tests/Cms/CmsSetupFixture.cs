using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional;

public sealed class CmsSetupFixture : IAsyncLifetime
{
    private readonly OrchardTestFixture _testFixture = new();

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
            await TenantHelper.SiteSetupAsync(page, new TenantInfo
            {
                Name = "Testing SaaS",
                Prefix = string.Empty,
                SetupRecipe = "SaaS",
            });
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
