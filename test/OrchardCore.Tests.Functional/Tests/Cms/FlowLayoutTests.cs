using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

public sealed class FlowLayoutFixture : CmsRecipeFixture
{
    protected override string RecipeName => "FlowLayout";
}

public sealed class FlowLayoutTests(FlowLayoutFixture fixture)
    : CmsTestBase<FlowLayoutFixture>(fixture), IClassFixture<FlowLayoutFixture>
{
    [Theory]
    [InlineData("flowstandard", true)]
    [InlineData("flowblocks", true)]
    [InlineData("bagstandard", false)]
    [InlineData("bagblocks", false)]
    public async Task ImportedWidgets_UseFullWidthByDefault_AndPreserveExplicitFlowWidths(string id, bool flow)
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.SetViewportSizeAsync(1600, 1100);
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync($"/Admin/Contents/ContentItems/{id}/Edit");

            var cards = page.Locator(".widget-template-placeholder > .widget-template");
            await Assertions.Expect(cards).ToHaveCountAsync(4);
            await AssertWidthsAsync(cards, flow);

            // Saving must not be necessary to obtain the default layout, nor change explicit widths.
            await page.Locator("button[name='submit.Publish']").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.Load);
            await page.GotoAndAssertOkAsync($"/Admin/Contents/ContentItems/{id}/Edit");
            await AssertWidthsAsync(cards, flow);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Theory]
    [InlineData("containerstandard")]
    [InlineData("containerblocks")]
    public async Task FlowInsideNamedBag_StacksWidgetsWithoutMetadata(string id)
    {
        var page = await Fixture.CreatePageAsync();
        try
        {
            await page.SetViewportSizeAsync(1600, 1100);
            await page.LoginAsync();
            await page.GotoAndAssertOkAsync($"/Admin/Contents/ContentItems/{id}/Edit");
            var bagCards = page.Locator(".bagpart-cards > .widget-template");
            await Assertions.Expect(bagCards).ToHaveCountAsync(1);
            var flowCards = bagCards.Locator(".flowpart-sections > .widget-template");
            await Assertions.Expect(flowCards).ToHaveCountAsync(2);
            await AssertStackedAsync(flowCards.Nth(0), flowCards.Nth(1));
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static async Task AssertWidthsAsync(ILocator cards, bool flow)
    {
        await AssertStackedAsync(cards.Nth(0), cards.Nth(1));
        await Assertions.Expect(cards.Nth(2)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(flow ? @"\bcol-md-6\b" : @"\bcol-md-12\b"));
        await Assertions.Expect(cards.Nth(3)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex(@"\bcol-md-12\b"));
        var full = await cards.Nth(0).BoundingBoxAsync();
        var explicitWidth = await cards.Nth(2).BoundingBoxAsync();
        Assert.NotNull(full);
        Assert.NotNull(explicitWidth);
        Assert.InRange(Math.Abs(explicitWidth.Width - full.Width * (flow ? 0.5f : 1)), 0, 1);
    }

    private static async Task AssertStackedAsync(ILocator first, ILocator second)
    {
        var fullWidth = new System.Text.RegularExpressions.Regex(@"\bcol-md-12\b");
        await Assertions.Expect(first).ToHaveClassAsync(fullWidth);
        await Assertions.Expect(second).ToHaveClassAsync(fullWidth);
        var firstBox = await first.BoundingBoxAsync();
        var secondBox = await second.BoundingBoxAsync();
        Assert.NotNull(firstBox);
        Assert.NotNull(secondBox);
        Assert.True(secondBox.Y >= firstBox.Y + firstBox.Height - 1, "Widgets should occupy separate rows.");
    }
}
