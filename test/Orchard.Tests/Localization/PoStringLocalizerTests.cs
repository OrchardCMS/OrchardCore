using Moq;
using Orchard.Localization.Abstractions;
using Orchard.Localization.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace Orchard.Tests.Localization
{
    public class PoStringLocalizerTests
    {
        private static Func<int, int> _csPluralRule = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);
        private Mock<ILocalizationManager> _localizationManager;

        public PoStringLocalizerTests()
        {
            _localizationManager = new Mock<ILocalizationManager>();
        }

        [Fact]
        public void LocalizerReturnsTranslationsFromProvidedDictionary()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), null, _localizationManager.Object);

            var translation = localizer["ball"];

            Assert.Equal("míč", translation);
        }

        [Fact]
        public void LocalizerReturnsOriginalTextIfTranslationsDoesntExistInProvidedDictionary()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), null, _localizationManager.Object);

            var translation = localizer["car"];

            Assert.Equal("car", translation);
        }

        [Fact]
        public void LocalizerReturnsOriginalTextIfDictionaryIsEmpty()
        {
            SetupDictionary("cs", new CultureDictionaryRecord[] { });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), null, _localizationManager.Object);

            var translation = localizer["car"];

            Assert.Equal("car", translation);
        }

        [Fact]
        public void LocalizerFallbacksToParentCultureIfTranslationDoesntExistInSpecificCulture()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" })
            });
            SetupDictionary("cs-CZ", new[] {
                new CultureDictionaryRecord("car", null, new[] { "auto", "auta", "aut" })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs-cz"), null, _localizationManager.Object);

            var translation = localizer["ball"];

            Assert.Equal("míč", translation);
        }

        [Fact]
        public void LocalizerReturnsTranslationFromSpecificCultureIfItExists()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" })
            });
            SetupDictionary("cs-CZ", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "balón", "balóny", "balónů" })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs-CZ"), null, _localizationManager.Object);

            var translation = localizer["ball"];

            Assert.Equal("balón", translation);
        }

        [Fact]
        public void LocalizerReturnsTranslationWithSpecificContext()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" }),
                new CultureDictionaryRecord("ball", "small", new[] { "míček", "míčky", "míčků" })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), "small", _localizationManager.Object);

            var translation = localizer["ball"];

            Assert.Equal("míček", translation);
        }

        [Fact]
        public void LocalizerReturnsTranslationWithoutContextIfTranslationWithContextDoesntExist()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" }),
                new CultureDictionaryRecord("ball", "big", new[] { "míček", "míčky", "míčků" })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), "small", _localizationManager.Object);

            var translation = localizer["ball"];

            Assert.Equal("míč", translation);
        }

        [Fact]
        public void LocalizerReturnsFormattedTranslation()
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("The page (ID:{0}) was deleted.", null, new[] { "Stránka (ID:{0}) byla smazána." })
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), "small", _localizationManager.Object);

            var translation = localizer["The page (ID:{0}) was deleted.", 1];

            Assert.Equal("Stránka (ID:1) byla smazána.", translation);
        }

        [Theory]
        [InlineData("car", 1)]
        [InlineData("cars", 2)]
        public void LocalizerReturnsOriginalTextForPluralIfTranslationDoesntExist(string expected, int count)
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "míče", "míčů" }),
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), null, _localizationManager.Object);

            var translation = localizer.Plural("car", "cars", count);

            Assert.Equal(expected, translation);
        }

        [Theory]
        [InlineData("míč", 1)]
        [InlineData("2 míče", 2)]
        [InlineData("5 míčů", 5)]
        public void LocalizerReturnsTranslationInCorrectPluralForm(string expected, int count)
        {
            SetupDictionary("cs", new[] {
                new CultureDictionaryRecord("ball", null, new[] { "míč", "{0} míče", "{0} míčů" }),
            });
            var localizer = new PoStringLocalizer(new CultureInfo("cs"), null, _localizationManager.Object);

            var translation = localizer.Plural("ball", "{0} balls", count, count);

            Assert.Equal(expected, translation);
        }

        private void SetupDictionary(string cultureName, IEnumerable<CultureDictionaryRecord> records)
        {
            var dictionary = new CultureDictionary(cultureName, _csPluralRule);
            dictionary.MergeTranslations(records);

            _localizationManager.Setup(o => o.GetDictionary(It.Is<CultureInfo>(c => c.Name == cultureName))).Returns(dictionary);

        }
    }
}
