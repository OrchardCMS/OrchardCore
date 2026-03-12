using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public abstract class CmsTestBase : IAsyncLifetime
{
    protected CmsSetupFixture Fixture { get; }

    protected TenantInfo Tenant { get; private set; }

    protected abstract string RecipeName { get; }

    protected CmsTestBase(CmsSetupFixture fixture)
    {
        Fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        Tenant = TestUtils.GenerateTenantInfo(RecipeName);
        var page = await Fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, Tenant);
        await page.CloseAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
