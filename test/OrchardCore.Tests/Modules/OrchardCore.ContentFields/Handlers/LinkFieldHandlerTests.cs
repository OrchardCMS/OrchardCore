using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Handlers;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Infrastructure.Html;
using System.Text.Encodings.Web;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields.Handlers;

public class LinkFieldHandlerTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IStringLocalizer<LinkFieldHandler>> _localizerMock;
    private readonly Mock<IHtmlSanitizerService> _htmlSanitizerServiceMock;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly LinkFieldHandler _handler;

    public LinkFieldHandlerTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _localizerMock = new Mock<IStringLocalizer<LinkFieldHandler>>();
        _htmlSanitizerServiceMock = new Mock<IHtmlSanitizerService>();
        _htmlEncoder = HtmlEncoder.Default;
        
        _handler = new LinkFieldHandler(
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
    public async Task ValidatingAsync_ShouldResolveVirtualPathWithPathBase(string inputUrl, string pathBase, string expectedUrl)
    {
        // Arrange
        var field = new LinkField { Url = inputUrl };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext(pathBase);
        SetupHtmlSanitizer();

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains(expectedUrl))), Times.Once);
        Assert.False(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldHandleNullHttpContext()
    {
        // Arrange
        var field = new LinkField { Url = "~/test" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext)null);
        SetupHtmlSanitizer();

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains("/test"))), Times.Once);
        Assert.False(context.ContentValidateResult.Errors.Any());
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    [InlineData("/absolute/path")]
    [InlineData("relative/path")]
    [InlineData("mailto:test@example.com")]
    public async Task ValidatingAsync_ShouldNotModifyNonVirtualPaths(string inputUrl)
    {
        // Arrange
        var field = new LinkField { Url = inputUrl };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("/app");
        SetupHtmlSanitizer();

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains(inputUrl))), Times.Once);
        Assert.False(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldHandleUrlWithAnchor()
    {
        // Arrange
        var field = new LinkField { Url = "~/test#section" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("/app");
        SetupHtmlSanitizer();

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        // Should validate only the path part without the anchor
        _htmlSanitizerServiceMock.Verify(h => h.Sanitize(It.Is<string>(s => s.Contains("/app/test") && !s.Contains("#section"))), Times.Once);
        Assert.False(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFailForInvalidUrl()
    {
        // Arrange
        var field = new LinkField { Url = "invalid://url with spaces" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("");
        SetupHtmlSanitizer();
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Invalid URL"));

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        Assert.True(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFailForRequiredFieldWhenEmpty()
    {
        // Arrange
        var field = new LinkField { Url = "" };
        var settings = new LinkFieldSettings { Required = true };
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("");
        SetupHtmlSanitizer();
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Required"));

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        Assert.True(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFailForSanitizedUrl()
    {
        // Arrange
        var field = new LinkField { Url = "~/test" };
        var settings = new LinkFieldSettings();
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("");
        _htmlSanitizerServiceMock.Setup(h => h.Sanitize(It.IsAny<string>())).Returns("different string");
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Invalid URL"));

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        Assert.True(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFailForRequiredLinkTextWhenEmpty()
    {
        // Arrange
        var field = new LinkField { Url = "http://example.com", Text = "" };
        var settings = new LinkFieldSettings { LinkTextMode = LinkTextMode.Required };
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("");
        SetupHtmlSanitizer();
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Required"));

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        Assert.True(context.ContentValidateResult.Errors.Any());
    }

    [Fact]
    public async Task ValidatingAsync_ShouldFailForStaticLinkTextModeWhenDefaultTextEmpty()
    {
        // Arrange
        var field = new LinkField { Url = "http://example.com", Text = "Some text" };
        var settings = new LinkFieldSettings { LinkTextMode = LinkTextMode.Static, DefaultText = "" };
        var partFieldDefinition = CreatePartFieldDefinition(settings);
        var context = CreateValidateContext(partFieldDefinition);

        SetupHttpContext("");
        SetupHtmlSanitizer();
        _localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()]).Returns(new LocalizedString("key", "Required"));

        // Act
        await _handler.ValidatingAsync(context, field);

        // Assert
        Assert.True(context.ContentValidateResult.Errors.Any());
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
        var settingsDict = new Dictionary<string, object> { { typeof(LinkFieldSettings).Name, settings } };
        return new ContentPartFieldDefinition("LinkField", "LinkField", settingsDict);
    }

    private static ValidateContentFieldContext CreateValidateContext(ContentPartFieldDefinition partFieldDefinition)
    {
        var contentItem = new ContentItem();
        var context = new ValidateContentFieldContext(contentItem)
        {
            ContentPartFieldDefinition = partFieldDefinition
        };
        return context;
    }
}