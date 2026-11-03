using System.Text.RegularExpressions;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the Flatpickr-based DateField-Localized editor (src/OrchardCore.
// Modules/OrchardCore.ContentFields/Views/DateField-Localized.Edit.cshtml,
// src/OrchardCore.Modules/OrchardCore.ContentFields/Assets/ts/
// date-field-localized.ts) - no shipped recipe configures a DateField with
// the "Localized" editor, so this uses the custom WidgetDragTests recipe/
// fixture, which defines a "DateFieldTest" content type with a "PublishDate"
// field using it.
public sealed class DateFieldLocalizedTests : CmsTestBase<WidgetDragTestsFixture>, IClassFixture<WidgetDragTestsFixture>
{
    public DateFieldLocalizedTests(WidgetDragTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task DateFieldLocalized_PickDate_StoresIsoAndRoundTrips()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/Contents/ContentTypes/DateFieldTest/Create");
        await page.Locator("#TitlePart_Title").FillAsync("Date Field Test Item");

        // Flatpickr's altInput:true replaces the field with two inputs: the
        // original (now type="hidden", still carrying the real "name" so it
        // posts) holding the canonical ISO value, and a visible, readonly
        // companion the admin actually interacts with.
        var isoInput = page.Locator("input.datepicker[type='hidden']");
        var visibleInput = page.Locator("input.datepicker[type='text']");
        await visibleInput.WaitForAsync();

        // Opens the calendar popup - picking the 10th avoids any ambiguity
        // with greyed-out leading/trailing days from adjacent months, which
        // also render as ".flatpickr-day" elements, regardless of what
        // today's date happens to be.
        await visibleInput.ClickAsync();
        var calendar = page.Locator(".flatpickr-calendar.open");
        await calendar.WaitForAsync();
        await calendar.Locator(".flatpickr-day:not(.prevMonthDay):not(.nextMonthDay)")
            .Filter(new LocatorFilterOptions { HasTextRegex = new Regex("^10$") }).ClickAsync();

        await Assertions.Expect(visibleInput).Not.ToHaveValueAsync(string.Empty);
        var isoValue = await isoInput.InputValueAsync();
        Assert.Matches(@"^\d{4}-\d{2}-10$", isoValue);

        await page.Locator(".btn.draft").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Creating (unlike editing) redirects straight to the new item's own
        // Edit page.
        await page.GotoAndAssertOkAsync(page.Url);

        var reloadedIsoInput = page.Locator("input.datepicker[type='hidden']");
        await Assertions.Expect(reloadedIsoInput).ToHaveValueAsync(isoValue);

        await page.CloseAsync();
    }
}
