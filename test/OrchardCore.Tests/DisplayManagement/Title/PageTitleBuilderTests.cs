using OrchardCore.DisplayManagement.Extensions;

namespace OrchardCore.DisplayManagement.Title
{
    public class PageTitleBuilderTests
    {
        private const string Separator = " - ";
        private const string SimpleTitle = "I'm a title";
        private const string FirstPartTitle = "first part";
        private const string SecondPartTitle = "second part";


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
            
            Assert.Equal(String.Empty, pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void FixedTitleSet()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.SetFixedTitle(new HtmlString(SimpleTitle));

            Assert.Equal(SimpleTitle, pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void FixedTitleClearAndCheck()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.SetFixedTitle(new HtmlString(SimpleTitle));
            pageTitleBuilder.Clear();

            Assert.Equal(String.Empty, pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void FixedTitleAddSegment()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.SetFixedTitle(new HtmlString(SimpleTitle));

            pageTitleBuilder.AddSegment(new HtmlString("you don't see me"));

            Assert.Equal(SimpleTitle, pageTitleBuilder.GenerateTitle().GetString());
        }


        [Fact]
        public void TitleAddSegment()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString(SimpleTitle), "after");

            Assert.Equal(SimpleTitle, pageTitleBuilder.GenerateTitle().GetString());
        }

        [Fact]
        public void TitleMultiAddSegment()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString(FirstPartTitle), "after");

            Assert.Equal(FirstPartTitle, pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());

            pageTitleBuilder.AddSegment(new HtmlString(SecondPartTitle), "after");

            Assert.Equal($"{FirstPartTitle}{Separator}{SecondPartTitle}", pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());
        }

        [Fact]
        public void TitleAddAndClear()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString(FirstPartTitle), "after");

            Assert.Equal(FirstPartTitle, pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());

            pageTitleBuilder.Clear();

            Assert.Equal(String.Empty, pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());
        }

        [Fact]
        public void TitleMultiGenerateTitle()
        {
            var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            pageTitleBuilder.AddSegment(new HtmlString(FirstPartTitle), "after");

            Assert.Equal(FirstPartTitle, pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());
            Assert.Equal(FirstPartTitle, pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());
        }

        [Fact]
        public void TitleAddSegments()
        {
            var pageTitleBuilder = (PageTitleBuilder)_serviceProvider.GetService<IPageTitleBuilder>();
            pageTitleBuilder.Clear();

            var elements = new IHtmlContent[]
            {
                new HtmlString(FirstPartTitle),
                new HtmlString(SecondPartTitle)
            };
            pageTitleBuilder.AddSegments(elements, "after");

            Assert.Equal($"{FirstPartTitle}{Separator}{SecondPartTitle}", pageTitleBuilder.GenerateTitle(new HtmlString(Separator)).GetString());
        }
    }
}
