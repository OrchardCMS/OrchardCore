using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class PluralizeStringLocalizerTests
    {
        private static PluralizationRule _csPluralRule = n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2);

        [Fact]
        public void PluralizeStringLocalizer_Pluralize()
        {
            // Arrange
            var localizer = new InMemoryPluralizeStringLocalizer("cs");
            LocalizedString resource;

            // Act
            resource = localizer.Pluralize("ball", 1);

            // Assert
            Assert.Equal("míč", resource.Value);
        }

        [Theory]
        [InlineData("{0} balls", 2, "2 míče")]
        [InlineData("{0} balls", 5, "5 míčů")]
        public void PluralizeStringLocalizer_PluralizeWithArgument(string name, int count, string expectedValue)
        {
            // Arrange
            var localizer = new InMemoryPluralizeStringLocalizer("cs");
            LocalizedString resource;

            // Act
            resource = localizer.Pluralize(name, count);

            // Assert
            Assert.Equal(expectedValue, resource.Value);
        }

        [Theory]
        [InlineData("person", 1, "person")]
        [InlineData("{0} person", 1, "1 person")]
        [InlineData("{0} person", 2, "2 person")]
        public void PluralizeStringLocalizer_Pluralize_MissingResourceReturnsDefaultName(string name, int count, string expectedValue)
        {
            // Arrange
            var localizer = new InMemoryPluralizeStringLocalizer("cs");
            LocalizedString resource;

            // Act
            resource = localizer.Pluralize(name, count);

            // Assert
            Assert.True(resource.ResourceNotFound);
            Assert.Equal(expectedValue, resource.Value);
        }

        [Fact]
        public void PluralizeStringLocalizer_GetPluralRule_InvalidCultureThrows()
        {
            // Arrange
            var localizer = new InMemoryPluralizeStringLocalizer("cs");

            // Act & Assert
            Assert.Throws(typeof(NotSupportedException), () =>
                localizer.GetPluralRule("??"));
        }

        public class InMemoryPluralizeStringLocalizer : IPluralizeStringLocalizer
        {
            private static CultureDictionary _dictionary;

            public InMemoryPluralizeStringLocalizer(string twoLetterISOLanguageName)
            {
                _dictionary = new CultureDictionary(twoLetterISOLanguageName, _csPluralRule);
                _dictionary.MergeTranslations(new[] {
                    new CultureDictionaryRecord("ball", null, new[] { "míč" }),
                    new CultureDictionaryRecord("{0} balls", null, new[] { "míč", "{0} míče", "{0} míčů" })
                });
                CultureInfo.CurrentUICulture = new CultureInfo(twoLetterISOLanguageName);
            }

            public LocalizedString this[string name]
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public LocalizedString this[string name, params object[] arguments]
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            {
                throw new NotImplementedException();
            }

            public PluralizationRule GetPluralRule(string twoLetterISOLanguageName)
            {
                if (twoLetterISOLanguageName == "cs")
                {
                    return _csPluralRule;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            public LocalizedString Pluralize(string name, int count)
            {
                var culture = CultureInfo.CurrentUICulture;
                var pluralRule = GetPluralRule(culture.TwoLetterISOLanguageName);
                var plurality = pluralRule(count);
                var format = _dictionary.Translations.ContainsKey(name) ? _dictionary.Translations[name][plurality] : null;
                var value = string.Format(format ?? name, count);

                return new LocalizedString(name, value, resourceNotFound: format == null);
            }

            public IStringLocalizer WithCulture(CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
