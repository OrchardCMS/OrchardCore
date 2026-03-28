using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public abstract class CmsRecipeFixture : IAsyncLifetime
{
    protected abstract string RecipeName { get; }

    /// <summary>
    /// The URL prefix for this recipe's tenant (e.g., "blog", "agency").
    /// </summary>
    public string Prefix { get; private set; }

    public IBrowser Browser => CmsServer.Browser;
    public string BaseUrl => CmsServer.BaseUrl;

    public async ValueTask InitializeAsync()
    {
        await CmsServer.AcquireAsync();
        Prefix = CmsServer.GetPrefix(RecipeName);
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

public sealed class AgencyFixture : CmsRecipeFixture
{
    protected override string RecipeName => "Agency";
}

public sealed class BlogFixture : CmsRecipeFixture
{
    protected override string RecipeName => "Blog";
}

public sealed class ComingSoonFixture : CmsRecipeFixture
{
    protected override string RecipeName => "ComingSoon";
}

public sealed class HeadlessFixture : CmsRecipeFixture
{
    protected override string RecipeName => "Headless";
}

public sealed class MigrationsFixture : CmsRecipeFixture
{
    protected override string RecipeName => "Migrations";
}
