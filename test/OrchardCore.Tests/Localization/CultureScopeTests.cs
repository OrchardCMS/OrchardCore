using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization
{
    public class CultureScopeTests
    {
        [Fact]
        public void CultureScopeSetsUICultureIfNotProvided()
        {
            // Arrange
            var culture = "ar-YE";

            // Act
            using var cultureScope = CultureScope.Create(culture);

            // Assert
            Assert.Equal(culture, CultureInfo.CurrentCulture.Name);
            Assert.Equal(culture, CultureInfo.CurrentUICulture.Name);
        }

        [Fact]
        public void CultureScopeSetsBothCultureAndUICulture()
        {
            // Arrange
            var culture = "ar";
            var uiCulture = "ar-YE";

            // Act
            using var cultureScope = CultureScope.Create(culture, uiCulture);

            // Assert
            Assert.Equal(culture, CultureInfo.CurrentCulture.Name);
            Assert.Equal(uiCulture, CultureInfo.CurrentUICulture.Name);
        }

        [Fact]
        public void CultureScopeSetsOrginalCulturesAfterEndOfScope()
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
        public async Task CultureScopeSetsOrginalCulturesOnException()
        {
            // Arrange
            var culture = CultureInfo.CurrentCulture;
            var uiCulture = CultureInfo.CurrentUICulture;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
            {
                using var cultureScope = CultureScope.Create("FR");
                throw new Exception("Something goes wrong!!");
            });

            Assert.Equal(culture, CultureInfo.CurrentCulture);
            Assert.Equal(uiCulture, CultureInfo.CurrentUICulture);
        }
    }
}
