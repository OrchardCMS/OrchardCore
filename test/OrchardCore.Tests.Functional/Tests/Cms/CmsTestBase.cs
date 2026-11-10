using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

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
        try
        {
            await page.NewTenantAsync(Tenant);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public abstract class CmsTestBase<TFixture> : IAsyncLifetime
    where TFixture : CmsRecipeFixture
{
    protected TFixture Fixture { get; }

    protected CmsTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual ValueTask DisposeAsync()
    {
        Fixture.AssertNoLoggedIssues();
        return ValueTask.CompletedTask;
    }
}
