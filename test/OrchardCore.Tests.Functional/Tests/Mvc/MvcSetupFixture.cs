using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional;

public sealed class MvcSetupFixture : IAsyncLifetime
{
    private readonly OrchardTestFixture _testFixture = new();

    public IBrowser Browser => _testFixture.Browser;
    public string BaseUrl => _testFixture.BaseUrl;

    public async ValueTask InitializeAsync()
    {
        await _testFixture.InitializeAsync();
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
