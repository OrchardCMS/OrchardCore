using OrchardCore.Liquid.Services;
using Xunit;

namespace OrchardCore.Modules.Liquid.Tests
{
    public class SlugServiceTests
    {
        [Theory]
        [InlineData("Sébastien Ros", "sebastien-ros")]
        [InlineData("Zoltán Lehóczky", "zoltan-lehoczky")]
        public void SlugServiceShouldNotRemoveDiacritics(string text, string expectedSlug)
        {
            // Arrange
            var slugService = new SlugService();

            // Act
            var slug = slugService.Slugify(text);

            // Assert
            Assert.Equal(expectedSlug, slug);
        }
    }
}
