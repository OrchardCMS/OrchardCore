using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public abstract class CmsTestBase : IAsyncDisposable
{
    protected CmsSetupFixture Fixture { get; }

    protected CmsTestBase(CmsSetupFixture fixture)
    {
        Fixture = fixture;
    }

    protected async Task<(IPage Page, TenantInfo Tenant)> SetupTenantAsync(string recipeName)
    {
        var tenant = TestUtils.GenerateTenantInfo(recipeName);
        var page = await Fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, tenant);
        await page.CloseAsync();

        return (await Fixture.CreatePageAsync(), tenant);
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
