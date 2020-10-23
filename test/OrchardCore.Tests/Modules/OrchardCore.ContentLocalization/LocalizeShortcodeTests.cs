using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentLocalization.Shortcodes;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.Shortcodes;
using OrchardCore.ResourceManagement;
using OrchardCore.Shortcodes.Services;
using Shortcodes;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentLocalization
{
    public class LocalizeShortcodeTests
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
            CultureInfo.CurrentCulture = new CultureInfo(currentCulture);

            var localizationProvider = new LocalizationShortcodeProvider();

            var processor = new ShortcodeService(new IShortcodeProvider[] { localizationProvider }, Enumerable.Empty<IShortcodeContextProvider>());

            var processed = await processor.ProcessAsync(text);
            Assert.Equal(expected, processed);

        }
    }
}
