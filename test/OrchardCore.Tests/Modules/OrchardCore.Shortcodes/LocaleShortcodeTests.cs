using OrchardCore.Shortcodes.Providers;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Tests.Modules.OrchardCore.Shortcodes
{
    public class LocaleShortcodeTests
    {
        [Theory]
        [InlineData("en", "foo bar baz", "foo bar baz")]
        [InlineData("en", "foo [locale 'en']bar[/locale][locale 'fr']far[/locale] baz", @"foo bar baz")]
        [InlineData("en", "foo [locale en]bar[/locale][locale fr]far[/locale] baz", @"foo bar baz")]
        [InlineData("en-CA", "foo [locale 'en']bar[/locale][locale 'fr']far[/locale] baz", @"foo bar baz")]
        [InlineData("en-CA", "foo [locale en false]bar[/locale][locale fr]far[/locale] baz", @"foo  baz")]
        [InlineData("fr", "foo [locale 'en']bar[/locale][locale 'fr']far[/locale] baz", @"foo far baz")]
        [InlineData("fr", "foo [locale en]bar[/locale][locale fr]far[/locale] baz", @"foo far baz")]
        public async Task ShouldProcess(string currentCulture, string text, string expected)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(currentCulture);

            var localeProvider = new LocaleShortcodeProvider();
            var processor = new ShortcodeService(new IShortcodeProvider[] { localeProvider }, Enumerable.Empty<IShortcodeContextProvider>());
            var processed = await processor.ProcessAsync(text);

            Assert.Equal(expected, processed);
        }
    }
}
