using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class SaasFixture : IAsyncLifetime
{
    private readonly OrchardTestFixture _testFixture = new(instanceId: nameof(SaasFixture));

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

    public void AssertNoLoggedErrors() => _testFixture.AssertNoLoggedErrors();

    public async Task<IPage> CreatePageAsync()
    {
        return await _testFixture.CreatePageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _testFixture.DisposeAsync();
    }
}

public sealed class SaasTests : IClassFixture<SaasFixture>, IAsyncLifetime
{
    private readonly SaasFixture _fixture;

    public SaasTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        _fixture.AssertNoLoggedErrors();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task DisplaysTheHomePageOfTheSaasTheme()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"/{_fixture.Tenant.Prefix}");
        await Assertions.Expect(page.Locator("h4")).ToContainTextAsync("Welcome to Orchard Core, your site has been successfully set up.");
        await page.CloseAsync();
    }

    [Fact]
    public async Task SaasAdminLoginShouldWork()
    {
        var page = await _fixture.CreatePageAsync();
        await page.LoginAsync($"/{_fixture.Tenant.Prefix}");
        await page.GotoAsync($"/{_fixture.Tenant.Prefix}/Admin");
        await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
        await page.CloseAsync();
    }
}
