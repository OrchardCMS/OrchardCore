using Microsoft.Playwright;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Mvc;

[Collection(MvcTestCollection.Name)]
public sealed class MvcTests
{
    private readonly MvcSetupFixture _fixture;

    public MvcTests(MvcSetupFixture fixture)
    {
        _fixture = fixture;
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
