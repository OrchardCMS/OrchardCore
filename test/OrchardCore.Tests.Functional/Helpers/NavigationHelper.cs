using Microsoft.Playwright;
using Xunit;

namespace OrchardCore.Tests.Functional.Helpers;

public static class NavigationHelper
{
    public static async Task GotoAndAssertOkAsync(this IPage page, string url)
    {
        var response = await page.GotoAsync(url);
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200 but got {response.Status} for {response.Url}");
    }
}
