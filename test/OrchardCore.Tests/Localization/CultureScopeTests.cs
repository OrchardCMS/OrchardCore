using System;
using System.Globalization;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class CultureScopeTests
    {
        [Fact]
        public void CultureScopeSetUICultureAutomaticallyIfNotSet()
        {
            // Arrange
            var culture = "ar-YE";

            // Act
            using (var cultureScope = CultureScope.Create(culture))
            {
                // Assert
                Assert.Equal(culture, cultureScope.Culture.Name);
                Assert.Equal(culture, cultureScope.UICulture.Name);
            }
        }

        [Fact]
        public void CultureScopeRetrievesBothCultureAndUICulture()
        {
            // Arrange
            var culture = "ar";
            var uiCulture = "ar-YE";

            // Act
            using (var cultureScope = CultureScope.Create(culture, uiCulture))
            {
                // Assert
                Assert.Equal(culture, cultureScope.Culture.Name);
                Assert.Equal(uiCulture, cultureScope.UICulture.Name);
            }
        }

        [Fact]
        public void CultureScopeRetrievesTheOrginalCulturesAfterScopeEnded()
        {
            // Arrange
            var culture = CultureInfo.CurrentCulture;
            var uiCulture = CultureInfo.CurrentUICulture;

            // Act
            using (var cultureScope = CultureScope.Create("FR"))
            {

            }

            // Assert
            Assert.Equal(culture, CultureInfo.CurrentCulture);
            Assert.Equal(uiCulture, CultureInfo.CurrentUICulture);
        }

        [Fact]
        public void CultureScopeRetrievesTheOrginalCulturesIfExceptionOccurs()
        {
            // Arrange
            var culture = CultureInfo.CurrentCulture;
            var uiCulture = CultureInfo.CurrentUICulture;

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() =>
            {
                using (var cultureScope = CultureScope.Create("FR"))
                {
                    throw new Exception("Something goes wrong!!");
                }
            });
            Assert.Equal(culture, CultureInfo.CurrentCulture);
            Assert.Equal(uiCulture, CultureInfo.CurrentUICulture);
        }
    }
}
