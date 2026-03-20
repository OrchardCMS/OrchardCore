using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

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
        Fixture.AssertNoLoggedErrors();
        return ValueTask.CompletedTask;
    }
}
