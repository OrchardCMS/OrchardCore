using System.Text.Unicode;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.ViewModels;
using OrchardCore.Localization.Data;

namespace OrchardCore.Tests.DisplayManagement;

public class GroupShapesTests
{
    private static readonly HtmlEncoder s_encoder = HtmlEncoder.Create(UnicodeRanges.All);

    [Fact]
    public void AdminTabs_TranslatedTabName_ShowsTheTranslationAndKeepsTheTabId()
    {
        // Arrange
        var groupShapes = new GroupShapes(new TestDataLocalizer(new() { ["Settings"] = "Ρυθμίσεις" }));
        var shape = CreateTabsShape("Settings");

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
        var groupShapes = new GroupShapes(NullDataLocalizer.Instance);
        var shape = CreateTabsShape("Settings");

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
        var groupShapes = new GroupShapes(new TestDataLocalizer(new() { ["Details"] = "Λεπτομέρειες" }));
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

    private static Shape CreateTabsShape(string tab)
    {
        var shape = new Shape();
        shape.Properties["Identifier"] = "test-id";
        shape.Properties["Tabs"] = new List<IGrouping<string, object>> { CreateGrouping(tab) };

        return shape;
    }

    private static IGrouping<string, object> CreateGrouping(string key)
        => new object[] { new Shape() }.ToLookup(_ => key).First();

    private static string GetHtmlString(IHtmlContent htmlContent)
    {
        using var writer = new StringWriter();
        htmlContent.WriteTo(writer, s_encoder);

        return writer.ToString();
    }

    /// <summary>
    /// Translates the names of the tabs and cards context only.
    /// </summary>
    private sealed class TestDataLocalizer : IDataLocalizer
    {
        private readonly Dictionary<string, string> _translations;

        public TestDataLocalizer(Dictionary<string, string> translations)
        {
            _translations = translations;
        }

        public DataLocalizedString this[string name, string context]
            => context == TabAndCardNames.DataLocalizationContext && _translations.TryGetValue(name, out var translation)
                ? new DataLocalizedString(context, name, translation)
                : new DataLocalizedString(context, name, name, resourceNotFound: true);

        public DataLocalizedString this[string name, string context, params object[] arguments]
            => this[name, context];

        public IEnumerable<DataLocalizedString> GetAllStrings(string context, bool includeParentCultures)
            => [];
    }
}
