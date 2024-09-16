using Cysharp.Text;

namespace OrchardCore.DisplayManagement.Title;

public class PageTitleBuilderTests
{
    private const string Separator = " - ";
    private const string SimpleTitle = "I'm a title";
    private const string FirstPartTitle = "first part";
    private const string SecondPartTitle = "second part";

    private readonly IServiceProvider _serviceProvider;

    public PageTitleBuilderTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IPageTitleBuilder, PageTitleBuilder>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void GenerateTitleEmpty()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();

        // Act & Assert
        Assert.Equal(string.Empty, ToString(pageTitleBuilder.GenerateTitle()));
    }

    [Fact]
    public void FixedTitleSet()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.SetFixedTitle(new HtmlString(SimpleTitle));

        // Act & Assert
        Assert.Equal(SimpleTitle, ToString(pageTitleBuilder.GenerateTitle()));
    }

    [Fact]
    public void FixedTitleClearAndCheck()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.SetFixedTitle(new HtmlString(SimpleTitle));
        pageTitleBuilder.Clear();

        // Act & Assert
        Assert.Equal(string.Empty, ToString(pageTitleBuilder.GenerateTitle()));
    }

    [Fact]
    public void FixedTitleAddSegment()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.SetFixedTitle(new HtmlString(SimpleTitle));

        // Act & Assert
        pageTitleBuilder.AddSegment(new HtmlString("you don't see me"));
        Assert.Equal(SimpleTitle, ToString(pageTitleBuilder.GenerateTitle()));
    }

    [Fact]
    public void TitleAddSegment()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.Clear();

        // Act & Assert
        pageTitleBuilder.AddSegment(new HtmlString(SimpleTitle), "after");
        Assert.Equal(SimpleTitle, ToString(pageTitleBuilder.GenerateTitle()));
    }

    [Fact]
    public void TitleMultiAddSegment()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.Clear();

        // Act & Assert
        pageTitleBuilder.AddSegment(new HtmlString(FirstPartTitle), "after");
        Assert.Equal(FirstPartTitle, ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
        pageTitleBuilder.AddSegment(new HtmlString(SecondPartTitle), "after");
        Assert.Equal($"{FirstPartTitle}{Separator}{SecondPartTitle}", ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
    }

    [Fact]
    public void TitleAddAndClear()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.Clear();

        // Act & Assert
        pageTitleBuilder.AddSegment(new HtmlString(FirstPartTitle), "after");
        Assert.Equal(FirstPartTitle, ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
        pageTitleBuilder.Clear();
        Assert.Equal(string.Empty, ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
    }

    [Fact]
    public void TitleMultiGenerateTitle()
    {
        // Arrange
        var pageTitleBuilder = _serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.Clear();

        // Act
        pageTitleBuilder.AddSegment(new HtmlString(FirstPartTitle), "after");

        // Assert
        Assert.Equal(FirstPartTitle, ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
        Assert.Equal(FirstPartTitle, ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
    }

    [Fact]
    public void TitleAddSegments()
    {
        // Arrange
        var pageTitleBuilder = (PageTitleBuilder)_serviceProvider.GetService<IPageTitleBuilder>();
        pageTitleBuilder.Clear();

        var elements = new IHtmlContent[]
        {
            new HtmlString(FirstPartTitle),
            new HtmlString(SecondPartTitle)
        };

        pageTitleBuilder.AddSegments(elements, "after");

        // Act & Assert
        Assert.Equal($"{FirstPartTitle}{Separator}{SecondPartTitle}", ToString(pageTitleBuilder.GenerateTitle(new HtmlString(Separator))));
    }

    private static string ToString(IHtmlContent content)
    {
        using var writer = new ZStringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
