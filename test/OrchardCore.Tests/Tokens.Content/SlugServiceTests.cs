using OrchardCore.Liquid.Services;
using Xunit;

namespace OrchardCore.Tests.Tokens.Content
{
    public class SlugServiceTests
    {
        private SlugService _slugService;

        public SlugServiceTests()
        {
            _slugService = new SlugService();
        }

        [Theory]
        [InlineData("a - b", "a-b")]
        [InlineData("a  -  -      -  -   -   -b", "a-b")]
        [InlineData("a - b - c-- d", "a-b-c-d")]
        public void ShouldStripContiguousDashes(string input, string expected)
        {
            var slug = _slugService.Slugify(input);
            Assert.Equal(expected, slug);
        }

        [Fact]
        public void ShouldChangePercentSymbolsToHyphans()
        {
            var slug = _slugService.Slugify("a%d");
            Assert.Equal("a-d", slug);
        }

        [Fact]
        public void ShouldChangeDotSymbolsToHyphans()
        {
            var slug = _slugService.Slugify("a,d");
            Assert.Equal("a-d", slug);
        }

        [Theory]
        [InlineData("Smith, John B.")]
        [InlineData("Smith, John B...")]
        public void ShouldRemoveHyphansFromEnd(string input)
        {
            // Act
            var slug = _slugService.Slugify(input);
            Assert.Equal("smith-john-b", slug);
        }

        [Fact]
        public void ShouldMakeSureFunkycharactersAndHyphansOnlyReturnSingleHyphan()
        {
            var slug = _slugService.Slugify("«a»\"'-%-.d");
            Assert.Equal("a-d", slug);
        }

        [Fact]
        public void ShouldConvertToLowercase()
        {
            var slug = _slugService.Slugify("ABCDE");
            Assert.Equal("abcde", slug);
        }

        [Fact]
        public void ShouldRemoveDiacritics()
        {
            var slug = _slugService.Slugify("àçéïôù");
            Assert.Equal("aceiou", slug);
        }

        [Theory]
        [InlineData("джинсы_клеш", "джинсы_клеш")]
        [InlineData("צוות_אורצ_רד", "צוות_אורצ_רד")]
        [InlineData("调度模块允许后台任务调度", "调度模块允许后台任务调度")]
        [InlineData("فريق_الاورشارد", "فريق_الاورشارد")]
        [InlineData("不正なコンテナ", "不正なコンテナ")]
        public void ShouldPreserveNonLatinCharacters(string input, string expected)
        {
            var slug = _slugService.Slugify(input);
            Assert.Equal(expected, slug);
        }
    }
}
