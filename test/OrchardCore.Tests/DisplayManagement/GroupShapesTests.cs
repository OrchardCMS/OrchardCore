using System.Text.Unicode;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.ViewModels;

namespace OrchardCore.Tests.DisplayManagement;

public class GroupShapesTests
{
    private static readonly HtmlEncoder _encoder = HtmlEncoder.Create(UnicodeRanges.All);

    [Fact]
    public void AdminTabs_TranslatedTabName_ShowsTheTranslationAndKeepsTheTabId()
    {
        // Arrange
        var groupShapes = new GroupShapes(new TestStringLocalizer(new() { ["Settings"] = "Ρυθμίσεις" }));
        var shape = new Shape();
        shape.Properties["Identifier"] = "test-id";
        shape.Properties["Tabs"] = new List<IGrouping<string, object>> { CreateGrouping("Settings") };

        // Act
        var html = GetHtmlString(groupShapes.AdminTabs(shape));

        // Assert
        Assert.Contains(">Ρυθμίσεις</a>", html);
        Assert.Contains("href=\"#tab-settings-test-id\"", html);
        Assert.Contains("aria-controls=\"tab-settings-test-id\"", html);
    }

    [Fact]
    public void AdminTabs_UntranslatedTabName_ShowsTheName()
    {
        // Arrange
        var groupShapes = new GroupShapes(new TestStringLocalizer(new()));
        var shape = new Shape();
        shape.Properties["Identifier"] = "test-id";
        shape.Properties["Tabs"] = new List<IGrouping<string, object>> { CreateGrouping("Settings") };

        // Act
        var html = GetHtmlString(groupShapes.AdminTabs(shape));

        // Assert
        Assert.Contains(">Settings</a>", html);
        Assert.Contains("href=\"#tab-settings-test-id\"", html);
    }

    [Fact]
    public async Task Card_TranslatedCardName_ShowsTheTranslationAndKeepsTheCardIds()
    {
        // Arrange
        var groupShapes = new GroupShapes(new TestStringLocalizer(new() { ["Details"] = "Λεπτομέρειες" }));
        var displayHelper = new Mock<IDisplayHelper>();
        displayHelper
            .Setup(helper => helper.ShapeExecuteAsync(It.IsAny<IShape>()))
            .ReturnsAsync(HtmlString.Empty);

        var shape = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = CreateGrouping("Details"),
        };

        // Act
        var html = GetHtmlString(await groupShapes.Card(displayHelper.Object, shape));

        // Assert
        Assert.Contains(">Λεπτομέρειες</h5>", html);
        Assert.Contains("id=\"heading-card-test-id-details\"", html);
        Assert.Contains("id=\"collapse-card-test-id-details\"", html);
    }

    private static IGrouping<string, object> CreateGrouping(string key)
        => new object[] { new Shape() }.ToLookup(_ => key).First();

    private static string GetHtmlString(IHtmlContent htmlContent)
    {
        using var writer = new StringWriter();
        htmlContent.WriteTo(writer, _encoder);

        return writer.ToString();
    }

    private sealed class TestStringLocalizer : IStringLocalizer<GroupShapes>
    {
        private readonly Dictionary<string, string> _translations;

        public TestStringLocalizer(Dictionary<string, string> translations)
        {
            _translations = translations;
        }

        public LocalizedString this[string name]
            => _translations.TryGetValue(name, out var translation)
                ? new LocalizedString(name, translation)
                : new LocalizedString(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments]
            => this[name];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => [];
    }
}
