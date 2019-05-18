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
        private readonly IList<CultureDictionary> _cultureDictionaries = new List<CultureDictionary>();
        private readonly Mock<ILogger> _logger;

        public ContentTypeDefinitionDataLocalizerTests()
        {
            SetupDictionary("en", new[] {
                new CultureDictionaryRecord("Menu", new[] { "Menu" }),
                new CultureDictionaryRecord("Blog", new[] { "Blog" }),
                new CultureDictionaryRecord("Shirt", new[] { "Shirt" }),
            });
            SetupDictionary("fr", new[] {
                new CultureDictionaryRecord("Menu", new[] { "Menu" }),
                new CultureDictionaryRecord("Blog", new[] { "Blog" }),
                new CultureDictionaryRecord("Shirt", new[] { "Chemise" }),
            });
            SetupDictionary("ar", new[] {
                new CultureDictionaryRecord("Menu", new[] { "قائمة" }),
                new CultureDictionaryRecord("Blog", new[] { "مدونة" }),
                new CultureDictionaryRecord("Shirt", new[] { "قميص" }),
            });
            SetupDictionary("cs", new CultureDictionaryRecord[] { });

            _logger = new Mock<ILogger>();
        }

        [Fact]
        public void LocalizerReturnsTranslationsFromProvidedDictionary()
        {
            var culture = "fr";
            var localizer = new ContentTypeDefinitionDataLocalizer(GetDictionary(culture), _logger.Object);

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var translation = localizer["Shirt"];

            Assert.Equal("Chemise", translation);
        }

        [Fact]
        public void LocalizerReturnsOriginalTextIfTranslationsDoesntExistInProvidedDictionary()
        {
            var culture = "fr";
            var localizer = new ContentTypeDefinitionDataLocalizer(GetDictionary(culture), _logger.Object);

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var translation = localizer["Page"];

            Assert.Equal("Page", translation);
        }

        [Fact]
        public void LocalizerReturnsOriginalTextIfDictionaryIsEmpty()
        {
            var culture = "cs";
            var localizer = new ContentTypeDefinitionDataLocalizer(GetDictionary(culture), _logger.Object);

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var translation = localizer["Page"];

            Assert.Equal("Page", translation);
        }

        [Fact]
        public void LocalizerReturnsTranslationFromSpecificCultureIfItExists()
        {
            var culture = "ar-YE";
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizerFactory = ContentTypeDefinitionDataLocalizerFactoryTests.CreateLocalizerFactory();
            var localizer = localizerFactory.Create();
            var translation = localizer["Shirt"];

            Assert.Equal("شميز", translation);
        }


        [Fact]
        public void LocalizerFallbacksToParentCultureIfTranslationDoesntExistInSpecificCulture()
        {
            var culture = "fr-FR";
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizerFactory = ContentTypeDefinitionDataLocalizerFactoryTests.CreateLocalizerFactory();
            var localizer = localizerFactory.Create();
            var translation = localizer["Shirt"];

            Assert.Equal("Chemise", translation);
        }

        [Theory]
        [InlineData(false, "Blog", "Blog")]
        [InlineData(true, "Blog", "مدونة")]
        public void LocalizerFallBackToParentCultureIfFallBackToParentUICulturesIsTrue(bool fallBackToParentUICulture, string resourceKey, string expected)
        {
            var culture = "ar-YE";
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizerFactory = ContentTypeDefinitionDataLocalizerFactoryTests.CreateLocalizerFactory(fallBackToParentUICulture);
            var localizer = localizerFactory.Create();
            var translation = localizer[resourceKey];

            Assert.Equal(expected, translation);
        }

        private void SetupDictionary(string cultureName, IEnumerable<CultureDictionaryRecord> records)
        {
            var dictionary = new CultureDictionary(cultureName, null);
            dictionary.MergeTranslations(records);
            _cultureDictionaries.Add(dictionary);
        }

        private CultureDictionary GetDictionary(string cultureName)
            => _cultureDictionaries.SingleOrDefault(d => d.CultureName == cultureName);
    }
}