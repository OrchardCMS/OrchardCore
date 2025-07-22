using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Drivers;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields.Drivers;

public class LinkFieldDisplayDriverTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IStringLocalizer<LinkFieldDisplayDriver>> _localizerMock;
    private readonly Mock<IHtmlSanitizerService> _htmlSanitizerServiceMock;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly LinkFieldDisplayDriver _driver;

    public LinkFieldDisplayDriverTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _localizerMock = new Mock<IStringLocalizer<LinkFieldDisplayDriver>>();
        _htmlSanitizerServiceMock = new Mock<IHtmlSanitizerService>();
        _htmlEncoder = HtmlEncoder.Default;

        _driver = new LinkFieldDisplayDriver(
            _httpContextAccessorMock.Object,
            _localizerMock.Object,
            _htmlSanitizerServiceMock.Object,
            _htmlEncoder);
    }

    [Theory]
    [InlineData("~/test", "", "/test")]
    [InlineData("~/test", "/app", "/app/test")]
    [InlineData("~/page", "/subdir", "/subdir/page")]
    [InlineData("~/", "", "/")]
    [InlineData("~/", "/app", "/app/")]
    public async Task UpdateAsync_ShouldResolveVirtualPathWithPathBase(string inputUrl, string pathBase, string expectedUrl)
    {
        // Arrange
        var field = new LinkField { Url = inputUrl };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateUpdateContext(partFieldDefinition);

        SetupHttpContext(pathBase);
        SetupHtmlSanitizer();

        // Act
        await _driver.UpdateAsync(field, context);

        // Assert
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains(expectedUrl))), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleNullHttpContext()
    {
        // Arrange
        var field = new LinkField { Url = "~/test" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateUpdateContext(partFieldDefinition);

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext)null);
        SetupHtmlSanitizer();

        // Act
        await _driver.UpdateAsync(field, context);

        // Assert
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains("/test"))), Times.Once);
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("/absolute/path")]
    [InlineData("relative/path")]
    [InlineData("mailto:test@example.com")]
    public async Task UpdateAsync_ShouldNotModifyNonVirtualPaths(string inputUrl)
    {
        // Arrange
        var field = new LinkField { Url = inputUrl };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateUpdateContext(partFieldDefinition);

        SetupHttpContext("/app");
        SetupHtmlSanitizer();

        // Act
        await _driver.UpdateAsync(field, context);

        // Assert
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains(inputUrl))), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHandleUrlWithAnchor()
    {
        // Arrange
        var field = new LinkField { Url = "~/test#section" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateUpdateContext(partFieldDefinition);

        SetupHttpContext("/app");
        SetupHtmlSanitizer();

        // Act
        await _driver.UpdateAsync(field, context);

        // Assert
        // Should validate only the path part without the anchor
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains("/app/test") && !s.Contains("#section"))), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldAddValidationErrorForInvalidUrl()
    {
        // Arrange
        var field = new LinkField { Url = "invalid://url with spaces" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateUpdateContext(partFieldDefinition);

        SetupHttpContext("");
        SetupHtmlSanitizer();
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Invalid URL"));

        // Act
        await _driver.UpdateAsync(field, context);

        // Assert
        Assert.True(context.Updater.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task UpdateAsync_ShouldAddValidationErrorForRequiredFieldWhenEmpty()
    {
        // Arrange
        var field = new LinkField { Url = "" };
        var settings = new LinkFieldSettings { Required = true };
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateUpdateContext(partFieldDefinition);

        SetupHttpContext("");
        SetupHtmlSanitizer();
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Required"));

        // Act
        await _driver.UpdateAsync(field, context);

        // Assert
        Assert.True(context.Updater.ModelState.ErrorCount > 0);
    }

    private void SetupHttpContext(string pathBase)
    {
        var httpContextMock = new Mock<HttpContext>();
        var requestMock = new Mock<HttpRequest>();

        requestMock.Setup(r => r.PathBase).Returns(new PathString(pathBase));
        httpContextMock.Setup(h => h.Request).Returns(requestMock.Object);
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContextMock.Object);
    }

    private void SetupHtmlSanitizer()
    {
        _htmlSanitizerServiceMock.Setup(h => h.Sanitize(It.IsAny<string>())).Returns<string>(s => s);
    }

    private static ContentPartFieldDefinition CreatePartFieldDefinition(LinkFieldSettings settings)
    {
        var settingsDict = new JsonObject
        {
            [typeof(LinkFieldSettings).Name] = JObject.FromObject(settings),
        };

        return new ContentPartFieldDefinition(new ContentFieldDefinition("LinkField"), "LinkField", settingsDict);
    }

    private static UpdateFieldEditorContext CreateUpdateContext(ContentPartFieldDefinition partFieldDefinition)
    {
        var updaterMock = new Mock<IUpdateModel>();
        updaterMock.Setup(u => u.TryUpdateModelAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<Expression<Func<object, object>>[]>()))
                  .ReturnsAsync(true);
        updaterMock.Setup(u => u.ModelState).Returns(new ModelStateDictionary());

        var shapeFactoryMock = new Mock<IShapeFactory>();
        var model = new Mock<IShape>();

        var updateEditorContext = new UpdateEditorContext(model.Object, "", false, "", shapeFactoryMock.Object, null, updaterMock.Object);
        return new UpdateFieldEditorContext(null, null, partFieldDefinition, updateEditorContext);
    }
}
