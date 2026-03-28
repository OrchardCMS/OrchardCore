using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class SaasFixture : IAsyncLifetime
{
    public IBrowser Browser => CmsServer.Browser;
    public string BaseUrl => CmsServer.BaseUrl;
    public TenantInfo Tenant { get; private set; }

    public async ValueTask InitializeAsync()
    {
        await CmsServer.AcquireAsync();

        // Create an additional child tenant to verify multi-tenancy (beyond the recipe tenants).
        var page = await CmsServer.CreatePageAsync();
        try
        {
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

    public void AssertNoLoggedIssues() => CmsServer.AssertNoLoggedIssues();

    public async Task<IPage> CreatePageAsync()
    {
        return await CmsServer.CreatePageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await CmsServer.ReleaseAsync();
    }
}
