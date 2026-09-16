using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;
using static OrchardCore.Localization.Endpoints.LocalizationManagementEndpoints;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class LocalizationMessageTests
{
    [Fact]
    public async Task List_FiltersExactContextKeyAndPrefixBeforePaging()
    {
        var fixture = new Fixture();
        fixture.French.MergeTranslations([
            new CultureDictionaryRecord("Cart.One", "Shop", ["Un"]),
            new CultureDictionaryRecord("Cart.Two", "Shop", ["Deux", "Plusieurs"]),
            new CultureDictionaryRecord("Cart.One", "Other", ["Autre"]),
            new CultureDictionaryRecord("cart.Three", "Shop", ["Trois"]),
        ]);
        var result = await fixture.List(new MessageListRequest { Culture = "FR", Context = "Shop", Prefix = "Cart.", Skip = 1, Take = 1 });
        var response = Assert.IsType<MessageListResponse>(((IValueHttpResult)result).Value);
        Assert.Equal("fr", response.Culture);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal("Cart.Two", Assert.Single(response.Items).Key);
        Assert.Equal(["Deux", "Plusieurs"], response.Items[0].Translations);
        result = await fixture.List(new MessageListRequest { Culture = "fr", Key = "Cart.One" });
        Assert.Equal(2, Assert.IsType<MessageListResponse>(((IValueHttpResult)result).Value).TotalCount);
        result = await fixture.List(new MessageListRequest { Culture = "fr-CA", Key = "Cart.One" });
        Assert.Empty(Assert.IsType<MessageListResponse>(((IValueHttpResult)result).Value).Items);
    }

    [Theory]
    [InlineData(0, "0 livre pour Alice")]
    [InlineData(1, "1 livre pour Alice")]
    [InlineData(2, "2 livres pour Alice")]
    public async Task Resolve_UsesCatalogPluralRulesAndAdditionalArguments(int count, string expected)
    {
        var fixture = new Fixture();
        fixture.French.MergeTranslations([new CultureDictionaryRecord("{0} book for {1}", "Shop", ["{0} livre pour {1}", "{0} livres pour {1}"])]);
        var response = await fixture.Resolve(new MessageResolveRequest
        {
            Culture = "fr", Context = "Shop", Key = "{0} book for {1}", Plural = "{0} books for {1}", Count = count, Arguments = Args("[\"Alice\"]"),
        });
        var message = Assert.IsType<MessageResolveResponse>(((IValueHttpResult)response).Value);
        Assert.Equal(expected, message.Value);
        Assert.Contains("{0}", message.Template);
    }

    [Fact]
    public async Task Resolve_UsesThreePluralFormsFromCatalog()
    {
        var fixture = new Fixture();
        fixture.Czech.MergeTranslations([new CultureDictionaryRecord("{0} book", "Shop", ["{0} kniha", "{0} knihy", "{0} knih"])]);
        foreach (var (count, expected) in new[] { (1, "1 kniha"), (3, "3 knihy"), (5, "5 knih") })
        {
            var result = await fixture.Resolve(new MessageResolveRequest { Culture = "cs", Context = "Shop", Key = "{0} book", Plural = "{0} books", Count = count });
            Assert.Equal(expected, Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value).Value);
        }
    }

    [Fact]
    public async Task Resolve_UsesParentAndContextlessFallbackAndRestoresCulture()
    {
        var fixture = new Fixture();
        fixture.French.MergeTranslations([new CultureDictionaryRecord("Price {0:N2} in 2026", ["Prix {0:N2} en 2026"])]);
        var original = CultureInfo.CurrentCulture;
        var result = await fixture.Resolve(new MessageResolveRequest { Culture = "fr-CA", Context = "Shop", Key = "Price {0:N2} in 2026", Arguments = Args("[12.5]") });
        Assert.Contains("12,50", Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value).Value);
        Assert.Equal(original, CultureInfo.CurrentCulture);
        fixture.Fallback = false;
        result = await fixture.Resolve(new MessageResolveRequest { Culture = "fr-CA", Context = "Shop", Key = "Price {0:N2} in 2026" });
        Assert.Equal("Price {0:N2} in 2026", Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value).Value);
    }

    [Fact]
    public async Task Resolve_OmittedCultureUsesRequestUiAndFormattingCultures()
    {
        var fixture = new Fixture();
        fixture.French.MergeTranslations([new CultureDictionaryRecord("Price {0:N2}", ["Prix {0:N2}"])]);
        using var scope = CultureScope.Create("en-US", "fr", ignoreSystemSettings: true);
        var result = await fixture.Resolve(new MessageResolveRequest { Key = "Price {0:N2}", Arguments = Args("[12.5]") });
        var response = Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value);
        Assert.Equal("fr", response.Culture);
        Assert.Equal("en-US", response.FormattingCulture);
        Assert.Equal("Prix 12.50", response.Value);
        var list = await fixture.List(new MessageListRequest());
        Assert.Equal("fr", Assert.IsType<MessageListResponse>(((IValueHttpResult)list).Value).Culture);
    }

    [Fact]
    public async Task ExplicitUnsupportedChildSelectsParentOnlyWhenEnabled()
    {
        var fixture = new Fixture();
        fixture.French.MergeTranslations([new CultureDictionaryRecord("Hello", ["Bonjour"])]);
        var result = await fixture.Resolve(new MessageResolveRequest { Culture = "fr-FR", Key = "Hello" });
        var response = Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value);
        Assert.Equal("fr", response.Culture);
        Assert.Equal("Bonjour", response.Value);
        fixture.Fallback = false;
        result = await fixture.Resolve(new MessageResolveRequest { Culture = "fr-FR", Key = "Hello" });
        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
    }

    [Fact]
    public async Task Resolve_MissingEntryUsesSourceAndPluralFallback()
    {
        var fixture = new Fixture();
        var result = await fixture.Resolve(new MessageResolveRequest { Key = "Missing {0}" });
        Assert.Equal("Missing {0}", Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value).Value);
        result = await fixture.Resolve(new MessageResolveRequest { Key = "{0} book", Plural = "{0} books", Count = 2 });
        Assert.Equal("2 books", Assert.IsType<MessageResolveResponse>(((IValueHttpResult)result).Value).Value);
    }

    [Theory]
    [InlineData("{0,1000000000}", "[\"x\"]")]
    [InlineData("{0:D999999999}", "[2]")]
    [InlineData("{9}", "[\"x\"]")]
    [InlineData("{0", "[\"x\"]")]
    [InlineData("{0}", "[{}]")]
    [InlineData("{0}", "[1e1000]")]
    public async Task Resolve_InvalidFormattingReturns400AndRestoresCulture(string key, string arguments)
    {
        var fixture = new Fixture();
        var original = CultureInfo.CurrentUICulture;
        var result = await fixture.Resolve(new MessageResolveRequest { Culture = "fr", Key = key, Arguments = Args(arguments) });
        Assert.Equal(400, ((IStatusCodeHttpResult)result).StatusCode);
        Assert.Equal(original, CultureInfo.CurrentUICulture);
    }

    [Fact]
    public async Task Endpoints_RejectUnsupportedCultureInvalidPagingAndIncompletePlural()
    {
        var fixture = new Fixture();
        Assert.Equal(400, ((IStatusCodeHttpResult)await fixture.List(new MessageListRequest { Take = 201 })).StatusCode);
        Assert.Equal(400, ((IStatusCodeHttpResult)await fixture.List(new MessageListRequest { Culture = "de" })).StatusCode);
        Assert.Equal(400, ((IStatusCodeHttpResult)await fixture.Resolve(new MessageResolveRequest { Key = "book", Count = 2 })).StatusCode);
        Assert.Equal(400, ((IStatusCodeHttpResult)await fixture.Resolve(new MessageResolveRequest { Key = "book", Plural = "books" })).StatusCode);
    }

    [Fact]
    public async Task Endpoints_DeniedPermissionDoesNotReadCatalog()
    {
        var fixture = new Fixture { Allowed = false };
        Assert.Equal(403, ((IStatusCodeHttpResult)await fixture.List(new MessageListRequest())).StatusCode);
        Assert.Equal(403, ((IStatusCodeHttpResult)await fixture.Resolve(new MessageResolveRequest { Key = "x" })).StatusCode);
        fixture.Manager.Verify(manager => manager.GetDictionary(It.IsAny<CultureInfo>()), Times.Never);
    }

    private static JsonElement[] Args(string json) => JsonSerializer.Deserialize<JsonElement[]>(json);

    private sealed class Fixture
    {
        private readonly DefaultHttpContext _context = new() { RequestServices = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider() };

        public bool Allowed { get; set; } = true;
        public bool Fallback { get; set; } = true;
        public CultureDictionary French { get; } = new("fr", count => count > 1 ? 1 : 0);
        public CultureDictionary Czech { get; } = new("cs", count => count == 1 ? 0 : count is >= 2 and <= 4 ? 1 : 2);
        public Mock<ILocalizationManager> Manager { get; } = new();
        private readonly Mock<ILocalizationService> _localization = new();
        private readonly Mock<IAuthorizationService> _authorization = new();
        private readonly Mock<IStringLocalizerFactory> _factory = new();

        public Fixture()
        {
            Manager.Setup(manager => manager.GetDictionary(It.IsAny<CultureInfo>())).Returns<CultureInfo>(culture => culture.Name switch
            {
                "fr" => French, "cs" => Czech, _ => new CultureDictionary(culture.Name, count => count == 1 ? 0 : 1),
            });
            _localization.Setup(service => service.GetDefaultCultureAsync()).ReturnsAsync("en");
            _localization.SetupGet(service => service.FallBackToParentCultures).Returns(() => Fallback);
            _localization.Setup(service => service.GetSupportedCulturesAsync()).ReturnsAsync(["en", "fr", "fr-CA", "cs"]);
            _authorization.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .Returns(() => Task.FromResult(Allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed()));
            _factory.Setup(factory => factory.Create(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((context, _) => new PortableObjectStringLocalizer(context, Manager.Object, Fallback, NullLogger.Instance));
        }

        public Task<IResult> List(MessageListRequest request) => MessagesAsync(_context, _authorization.Object, _localization.Object, Manager.Object, request);
        public Task<IResult> Resolve(MessageResolveRequest request) => ResolveMessageAsync(_context, _authorization.Object, _localization.Object, _factory.Object, request);
    }
}
