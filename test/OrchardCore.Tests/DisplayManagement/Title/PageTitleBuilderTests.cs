using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Environment.Extensions;
using OrchardCore.Localization;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.DisplayManagement.Title
{
    public class PageTitleBuilderTests
    {
        private readonly IServiceProvider _serviceProvider;

        public PageTitleBuilderTests()
        {

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IPageTitleBuilder, PageTitleBuilder>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void GenerateTitleEmpty()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();

            
            Assert.Equal("", pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void FixedTitleSet()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.SetFixedTitle(new HtmlString("I'm a title"));

            Assert.Equal("I'm a title", pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void FixedTitleClearAndCheck()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.SetFixedTitle(new HtmlString("I'm a title"));
            pageTitleBuilder.Clear();

            Assert.Equal("", pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void FixedTitleAddSegment()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.SetFixedTitle(new HtmlString("I'm a title"));

            pageTitleBuilder.AddSegment(new HtmlString("you don't see me"));

            Assert.Equal("I'm a title", pageTitleBuilder.GenerateTitle().GetString());
        }


        [Fact]
        public void TitleAddSegment()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString("you have to see me"), "after");

            Assert.Equal("you have to see me", pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void TitleMultiAddSegment()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString("first part"), "after");

            Assert.Equal("first part", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());

            pageTitleBuilder.AddSegment(new HtmlString("second part"), "after");

            Assert.Equal("first part - second part", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());
        }

        [Fact]
        public void TitleAddAndClear()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString("first part"), "after");

            Assert.Equal("first part", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());

            pageTitleBuilder.Clear();

            Assert.Equal("", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());
        }

        [Fact]
        public void TitleMultiGenerateTitle()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString("first part"), "after");

            Assert.Equal("first part", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());

            Assert.Equal("first part", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());
        }

        [Fact]
        public void TitleAddSegments()
        {
            var pageTitleBuilder = (PageTitleBuilder)_serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            var elements = new IHtmlContent[]
            {
                new HtmlString("the first part"),
                new HtmlString("the second one")
            };
            pageTitleBuilder.AddSegments(elements, "after");

            Assert.Equal("the first part - the second one", pageTitleBuilder.GenerateTitle(new HtmlString(" - ")).GetString());
        }
    }
}
