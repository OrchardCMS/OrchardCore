using Orchard.Tokens.Content.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Orchard.Tests.Tokens.Content
{
    public class SlugServiceTests
    {
        private SlugService _slugService;

        public SlugServiceTests()
        {
            _slugService = new SlugService();
        }

        [Fact]
        public void ShouldStripContiguousDashes()
        {
            var slug = _slugService.Slugify("a - b");
            Assert.Equal("a-b", slug);
        }

        [Fact]
        public void ShouldStripContiguousDashes2()
        {
            var slug = _slugService.Slugify("a  -  -      -  -   -   -b");
            Assert.Equal("a-b", slug);
        }

        [Fact]
        public void ShouldStripContiguousDashesEverywhere()
        {
            var slug = _slugService.Slugify("a  -  b - c -- d");
            Assert.Equal("a-b-c-d", slug);
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

        [Fact]
        public void ShouldPreserveCyrilicCharacters()
        {
            var slug = _slugService.Slugify("джинсы_клеш");
            Assert.Equal("джинсы_клеш", slug);
        }

        [Fact]
        public void ShouldPreserveHebrewCharacters()
        {
            var slug = _slugService.Slugify("צוות_אורצ_רד");
            Assert.Equal("צוות_אורצ_רד", slug);
        }

        [Fact]
        public void ShouldPreserveChineseCharacters()
        {
            var slug = _slugService.Slugify("调度模块允许后台任务调度");
            Assert.Equal("调度模块允许后台任务调度", slug);
        }

        [Fact]
        public void ShouldPreserveArabicCharacters()
        {
            var slug = _slugService.Slugify("فريق_الاورشارد");
            Assert.Equal("فريق_الاورشارد", slug);
        }

        [Fact]
        public void ShouldPreserveJapaneseCharacters()
        {
            var slug = _slugService.Slugify("不正なコンテナ");
            Assert.Equal("不正なコンテナ", slug);
        }
    }
}
