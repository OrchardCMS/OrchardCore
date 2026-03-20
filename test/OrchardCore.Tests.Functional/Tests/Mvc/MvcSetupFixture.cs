using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Mvc;

public sealed class MvcSetupFixture : IAsyncLifetime
{
    private readonly OrchardTestFixture _testFixture = new(isMvc: true);

    public IBrowser Browser => _testFixture.Browser;
    public string BaseUrl => _testFixture.BaseUrl;

    public async ValueTask InitializeAsync()
    {
        await _testFixture.InitializeAsync();
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
