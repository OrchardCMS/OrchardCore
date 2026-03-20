using Microsoft.Playwright;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Mvc;

public sealed class MvcTests : IClassFixture<MvcSetupFixture>, IAsyncLifetime
{
    private readonly MvcSetupFixture _fixture;

    public MvcTests(MvcSetupFixture fixture)
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
    public async Task ShouldDisplayHelloWorld()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync("/");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Hello World");
        await page.CloseAsync();
    }
}
