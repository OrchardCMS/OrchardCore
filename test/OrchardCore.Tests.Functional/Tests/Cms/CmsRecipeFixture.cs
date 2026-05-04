using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public abstract class CmsRecipeFixture : IAsyncLifetime
{
    private static int _instanceCounter;
    private readonly OrchardTestFixture _testFixture;

    protected abstract string RecipeName { get; }

    public IBrowser Browser => _testFixture.Browser;
    public string BaseUrl => _testFixture.BaseUrl;

    protected CmsRecipeFixture()
    {
        _testFixture = new OrchardTestFixture(instanceId: $"{GetType().Name}_{_instanceCounter++}");
    }

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
                    Name = $"Testing {RecipeName}",
                    Prefix = string.Empty,
                    SetupRecipe = RecipeName,
                });
            }
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

