using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class ContentTypeDefinitionDataLocalizerTests
    {
        private readonly Mock<ILogger> _logger;

        public ContentTypeDefinitionDataLocalizerTests()
        {
            _logger = new Mock<ILogger>();
        }

        [Fact]
        public void LocalizerReturnsTranslationsFromProvidedDictionary()
        {
            var frDictionary = SetupDictionary("fr", new[] {
                new CultureDictionaryRecord("Menu", new[] { "Menu" }),
                new CultureDictionaryRecord("Blog", new[] { "Blog" }),
                new CultureDictionaryRecord("Shirt", new[] { "Chemise" })
            });

            var culture = "fr";
            var localizer = new ContentTypeDefinitionDataLocalizer(
                new DataResourceManager(new[] { frDictionary }),
                _logger.Object);

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var translation = localizer["Shirt"];

            Assert.Equal("Chemise", translation);
        }

        [Fact]
        public void LocalizerReturnsOriginalTextIfTranslationsDoesntExistInProvidedDictionary()
        {
            var frDictionary = SetupDictionary("fr", new[] {
                new CultureDictionaryRecord("Menu", new[] { "Menu" }),
                new CultureDictionaryRecord("Blog", new[] { "Blog" }),
                new CultureDictionaryRecord("Shirt", new[] { "Chemise" })
            });

            var culture = "fr";
            var localizer = new ContentTypeDefinitionDataLocalizer(
                new DataResourceManager(new[] { frDictionary }),
                _logger.Object);

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var translation = localizer["Page"];

            Assert.Equal("Page", translation);
        }

        [Fact]
        public void LocalizerReturnsOriginalTextIfDictionaryIsEmpty()
        {
            var csDictionary = SetupDictionary("cs", new CultureDictionaryRecord[] { });

            var culture = "cs";
            var localizer = new ContentTypeDefinitionDataLocalizer(
                new DataResourceManager(new[] { csDictionary }),
                _logger.Object);

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var translation = localizer["Page"];

            Assert.Equal("Page", translation);
        }

        [Fact]
        public void LocalizerReturnsTranslationFromSpecificCultureIfItExists()
        {
            var arDictionary = SetupDictionary("ar", new[] {
                new CultureDictionaryRecord("Menu", new[] { "قائمة" }),
                new CultureDictionaryRecord("Blog", new[] { "مدونة" }),
                new CultureDictionaryRecord("Shirt", new[] { "قميص" })
            });
            var ar_yeDictionary = SetupDictionary("ar-YE", new[] {
                new CultureDictionaryRecord("Menu", new[] { "قائمة" }),
                new CultureDictionaryRecord("Blog", new[] { "مدونة" }),
                new CultureDictionaryRecord("Shirt", new[] { "شميز" })
            });

            var culture = "ar-YE";
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizer = new ContentTypeDefinitionDataLocalizer(
                new DataResourceManager(new[] { arDictionary, ar_yeDictionary }),
                _logger.Object);
            var translation = localizer["Shirt"];

            Assert.Equal("شميز", translation);
        }

        [Theory]
        [InlineData(false, "Blog", "Blog")]
        [InlineData(true, "Blog", "مدونة")]
        public void LocalizerFallBackToParentCultureIfFallBackToParentUICulturesIsTrue(bool fallBackToParentUICulture, string resourceKey, string expected)
        {
            var arDictionary = SetupDictionary("ar", new[] {
                new CultureDictionaryRecord("Menu", new[] { "قائمة" }),
                new CultureDictionaryRecord("Blog", new[] { "مدونة" }),
                new CultureDictionaryRecord("Shirt", new[] { "قميص" })
            });
            var ar_yeDictionary = SetupDictionary("ar-YE", new[] {
                new CultureDictionaryRecord("Menu", new[] { "قائمة" }),
                new CultureDictionaryRecord("Shirt", new[] { "شميز" })
            });

            var culture = "ar-YE";
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizer = new ContentTypeDefinitionDataLocalizer(
                new DataResourceManager(new[] { arDictionary, ar_yeDictionary }, fallBackToParentUICulture),
                _logger.Object);
            var translation = localizer[resourceKey];

            Assert.Equal(expected, translation);
        }

        [Theory]
        [InlineData(false, new[] { "شميز" })]
        [InlineData(true, new[] { "شميز", "قائمة", "مدونة" })]
        public void LocalizerReturnsGetAllStrings(bool includeParentCultures, string[] values)
        {
            var arDictionary = SetupDictionary("ar", new[] {
                new CultureDictionaryRecord("Menu", new[] { "قائمة" }),
                new CultureDictionaryRecord("Blog", new[] { "مدونة" }),
                new CultureDictionaryRecord("Shirt", new[] { "قميص" })
            });
            var ar_yeDictionary = SetupDictionary("ar-YE", new[] {
                new CultureDictionaryRecord("Shirt", new[] { "شميز" })
            });

            var culture = "ar-YE";
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizer = new ContentTypeDefinitionDataLocalizer(
                new DataResourceManager(new[] { arDictionary, ar_yeDictionary }),
                _logger.Object);
            var localizedStrings = localizer.GetAllStrings(includeParentCultures);

            Assert.Equal(values.Length, localizedStrings.Count());
            Assert.Equal(values, localizedStrings.Select(l => l.Value));
        }

        private CultureDictionary SetupDictionary(string cultureName, IEnumerable<CultureDictionaryRecord> records)
        {
            var dictionary = new CultureDictionary(cultureName, null);
            dictionary.MergeTranslations(records);

            return dictionary;
        }
    }
}