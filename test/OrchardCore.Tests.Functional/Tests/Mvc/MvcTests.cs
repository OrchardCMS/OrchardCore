using Microsoft.Playwright;

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
        _fixture.AssertNoLoggedIssues();
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task ShouldDisplayHelloWorld()
    {
        var page = await _fixture.CreatePageAsync();
        var response = await page.GotoAsync("/");
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
        await Assertions.Expect(page.Locator("body")).ToContainTextAsync("Hello World");
        await page.CloseAsync();
    }
}
