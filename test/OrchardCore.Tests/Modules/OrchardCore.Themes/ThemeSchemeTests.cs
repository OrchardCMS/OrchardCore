using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;
using OrchardCore.Themes.Models;
using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.Schemes.Controllers;
using OrchardCore.Themes.Schemes.Deployment;
using OrchardCore.Themes.Schemes.Models;
using OrchardCore.Themes.Schemes.Recipes;
using OrchardCore.Themes.Schemes.Services;
using OrchardCore.Themes.Schemes.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Themes;

/// <summary>
/// The generic side of theme schemes: the store, the runtime service, the controller, the recipe and deployment
/// steps, and the Schemes button of the themes list, with two themes that support schemes.
/// </summary>
public sealed class ThemeSchemeTests
{
    private const string ThemeA = "ThemeA";
    private const string ThemeB = "ThemeB";
    private const string Unregistered = "NoSchemes";

    [Theory]
    [InlineData("custom-4c3hkg22jkk551r5mg8wevt6cc", true)]
    [InlineData("custom-a-b-1", true)]
    [InlineData("blue", true)]
    [InlineData("1blue", false)]
    [InlineData("-blue", false)]
    [InlineData("custom-ABC", false)]
    [InlineData("custom-x\"]{color:red}", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void SchemeIds_AreSafeCssValues(string id, bool valid)
        => Assert.Equal(valid, ThemeSchemeStore.IsValidId(id));

    [Fact]
    public void NewIds_AreValid()
        => Assert.True(ThemeSchemeStore.IsValidId(ThemeSchemeStore.NewId()));

    [Theory]
    [InlineData(null, null, null, null, null, true)]
    [InlineData("lato", "system", 0.875, 0.0, 0.75, true)]
    [InlineData("source-sans-3", null, 1.125, 1.5, 1.25, true)]
    [InlineData("Lato", null, null, null, null, false)]
    [InlineData("comic-sans", null, null, null, null, false)]
    [InlineData(null, "papyrus", null, null, null, false)]
    [InlineData(null, null, 0.8, null, null, false)]
    [InlineData(null, null, 1.2, null, null, false)]
    [InlineData(null, null, null, 1.6, null, false)]
    [InlineData(null, null, null, -0.5, null, false)]
    [InlineData(null, null, null, null, 0.5, false)]
    [InlineData(null, null, null, null, 1.5, false)]
    public void Typography_IsValidatedAgainstTheKnownFontsAndTheRanges(string font, string heading, double? fontScale, double? radiusScale, double? density, bool valid)
        => Assert.Equal(valid, ThemeSchemeStore.HasValidTypography(new ThemeScheme
        {
            FontFamily = font,
            HeadingFontFamily = heading,
            FontScale = fontScale,
            RadiusScale = radiusScale,
            Density = density,
        }));

    [Theory]
    [InlineData("#2a81bb", null, true)]
    [InlineData("#fff", "#123456", true)]
    [InlineData("blue", null, false)]
    [InlineData("#2a81bb", "red", false)]
    public void Colors_AreValidatedAsHexColors(string primary, string page, bool valid)
        => Assert.Equal(valid, ThemeSchemeStore.HasValidColors(new ThemeScheme { Primary = primary, Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", Page = page }));

    [Theory]
    [InlineData(1, 1, 1, true)]
    [InlineData(0.875, 0, 0.75, true)]
    [InlineData(1.125, 1.5, 1.25, true)]
    [InlineData(0.5, 1, 1, false)]
    [InlineData(1, 2, 1, false)]
    [InlineData(1, 1, 2, false)]
    public void Editor_RejectsScalesOutOfRange(double fontScale, double radiusScale, double density, bool valid)
    {
        var model = new ThemeSchemeViewModel { Name = "A", FontScale = fontScale, RadiusScale = radiusScale, Density = density };

        Assert.Equal(valid, Validator.TryValidateObject(model, new ValidationContext(model), [], validateAllProperties: true));
    }

    [Fact]
    public void Editor_OffersOnlyValidSteps()
    {
        Assert.All(ThemeSchemeTypography.FontScales, value => Assert.True(ThemeSchemeTypography.IsValidFontScale(value)));
        Assert.All(ThemeSchemeTypography.RadiusScales, value => Assert.True(ThemeSchemeTypography.IsValidRadiusScale(value)));
        Assert.All(ThemeSchemeTypography.Densities, value => Assert.True(ThemeSchemeTypography.IsValidDensity(value)));
        Assert.Equal(["lato", "inter", "source-sans-3", "system"], ThemeSchemeTypography.Fonts.Select(font => font.Key));
    }

    [Fact]
    public void AddThemeSchemes_RegistersTheProvider_AndItsTheme()
    {
        var services = new ServiceCollection();
        services.AddThemeSchemes<FakeProviderA>();
        services.AddThemeSchemes<FakeProviderB>();
        services.AddThemeSchemes<FakeProviderA>();

        using var provider = services.BuildServiceProvider();

        Assert.Equal([ThemeA, ThemeB], provider.GetServices<IThemeSchemeProvider>().Select(p => p.ThemeId));
        Assert.Equal([ThemeA, ThemeB], provider.GetRequiredService<IOptions<ThemeSchemesOptions>>().Value.Themes.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Store_KeepsTheSchemesOfEachThemeApart()
    {
        var (store, _) = CreateStore();

        await store.SaveAsync(ThemeA, Scheme("custom-only-a", "Only A"));

        Assert.Equal(["a-one", "a-two", "custom-only-a"], (await store.ListAsync(ThemeA)).Select(scheme => scheme.Id));
        Assert.Equal(["b-one"], (await store.ListAsync(ThemeB)).Select(scheme => scheme.Id));
        Assert.Null(await store.FindAsync(ThemeB, "custom-only-a"));
        Assert.Null(await store.FindAsync(ThemeB, "a-one"));
        Assert.Null(await store.FindAsync(ThemeA, "b-one"));

        // Each theme's stylesheet holds its own schemes, written by its own provider.
        var (cssA, _) = await store.GetStylesheetAsync(ThemeA);
        var (cssB, _) = await store.GetStylesheetAsync(ThemeB);

        Assert.Contains("[data-a-scheme=\"custom-only-a\"]", cssA, StringComparison.Ordinal);
        Assert.DoesNotContain("b-one", cssA, StringComparison.Ordinal);
        Assert.Contains("[data-b-scheme=\"b-one\"]", cssB, StringComparison.Ordinal);
        Assert.DoesNotContain("a-one", cssB, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Store_SeedsATheme_OnlyOnce_AndNeverAgainOnceEmptied()
    {
        var providerA = new FakeProviderA();
        var documents = new InMemoryThemeSchemesDocumentManager();
        var store = new ThemeSchemeStore(documents, [providerA, new FakeProviderB()], new MemoryCache(new MemoryCacheOptions()));

        Assert.Equal(2, (await store.ListAsync(ThemeA)).Count);
        Assert.Equal(2, (await store.ListAsync(ThemeA)).Count);
        Assert.Equal(1, providerA.Seeds);
        Assert.Contains(ThemeA, documents.Document.Seeded);

        // A theme is seeded when it is first read, not before.
        Assert.DoesNotContain(ThemeB, documents.Document.Seeded);

        // Every scheme deleted (the default can't be deleted through the store, so straight from the document).
        documents.Document.Themes[ThemeA].Clear();
        await documents.UpdateAsync(documents.Document);

        var later = new ThemeSchemeStore(documents, [providerA, new FakeProviderB()], new MemoryCache(new MemoryCacheOptions()));

        Assert.Empty(await later.ListAsync(ThemeA));
        Assert.Null(await later.GetDefaultIdAsync(ThemeA));
        Assert.Equal(1, providerA.Seeds);
    }

    [Fact]
    public async Task Store_SeedsBeforeTheFirstSave_SoTheDefaultsAreNotLost()
    {
        var (store, documents) = CreateStore();

        await store.SaveAsync(ThemeA, Scheme("custom-first", "First"));

        Assert.Equal(["a-one", "a-two", "custom-first"], documents.Document.Themes[ThemeA].Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task Store_DefaultIs_ThePickedScheme_ThenTheProvidersDefault_ThenTheFirst()
    {
        var (store, documents) = CreateStore();

        Assert.Equal("a-two", await store.GetDefaultIdAsync(ThemeA));

        Assert.True(await store.SetDefaultAsync(ThemeA, "a-one"));
        Assert.Equal("a-one", await store.GetDefaultIdAsync(ThemeA));
        Assert.Equal("a-one", documents.Document.Defaults[ThemeA]);

        // A theme's default is its own: the other theme keeps its provider's.
        Assert.Equal("b-one", await store.GetDefaultIdAsync(ThemeB));
        Assert.False(await store.SetDefaultAsync(ThemeB, "a-one"));

        // A picked default that is gone (e.g. removed by a recipe) falls back on the provider's default.
        documents.Document.Themes[ThemeA].Remove("a-one");
        Assert.Equal("a-two", await store.GetDefaultIdAsync(ThemeA));

        // And when that is gone too, on the first scheme.
        documents.Document.Themes[ThemeA].Remove("a-two");
        documents.Document.Themes[ThemeA]["custom-z"] = Scheme("custom-z", "Z");
        Assert.Equal("custom-z", await store.GetDefaultIdAsync(ThemeA));
    }

    [Fact]
    public async Task Store_RefusesToDeleteTheDefault()
    {
        var (store, _) = CreateStore();

        Assert.False(await store.DeleteAsync(ThemeA, "a-two"));
        Assert.NotNull(await store.FindAsync(ThemeA, "a-two"));

        Assert.True(await store.SetDefaultAsync(ThemeA, "a-one"));
        Assert.False(await store.DeleteAsync(ThemeA, "a-one"));
        Assert.True(await store.DeleteAsync(ThemeA, "a-two"));
        Assert.Null(await store.FindAsync(ThemeA, "a-two"));
    }

    [Fact]
    public async Task Store_RejectsAThemeThatIsNotRegistered()
    {
        var (store, documents) = CreateStore();

        Assert.False(store.IsRegistered(Unregistered));
        Assert.Empty(await store.ListAsync(Unregistered));
        Assert.Null(await store.FindAsync(Unregistered, "a-one"));
        Assert.Null(await store.GetDefaultIdAsync(Unregistered));
        await Assert.ThrowsAsync<ArgumentException>(() => store.SaveAsync(Unregistered, Scheme("custom-x", "X")));
        await Assert.ThrowsAsync<ArgumentException>(() => store.DeleteAsync(Unregistered, "a-one"));
        await Assert.ThrowsAsync<ArgumentException>(() => store.SetDefaultAsync(Unregistered, "a-one"));
        await Assert.ThrowsAsync<ArgumentException>(() => store.GetStylesheetAsync(Unregistered));
        Assert.False(documents.Document.Themes.ContainsKey(Unregistered));
    }

    [Fact]
    public void Stylesheet_SkipsSchemesWithAnInvalidId()
    {
        var css = ThemeSchemeStore.BuildStylesheet(new FakeProviderA(), [Scheme("custom-ok", "Ok"), Scheme("Bad Id", "Bad")]);

        Assert.Contains("[data-a-scheme=\"custom-ok\"]", css, StringComparison.Ordinal);
        Assert.DoesNotContain("Bad Id", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Service_ResolvesTheUsersCookie_ThenTheSiteDefault_ThenTheProvidersDefault()
    {
        var (store, _) = CreateStore();
        var httpContext = new DefaultHttpContext();
        var service = new ThemeSchemeService(new HttpContextAccessor { HttpContext = httpContext }, new ShellSettings { Name = "Default" }, store);

        Assert.Equal("Default-ThemeA-scheme", service.CookieName(ThemeA));

        // No cookie, no site default: the provider's default.
        Assert.Equal("a-two", await service.GetSchemeAttributeAsync(ThemeA));
        Assert.Null(await service.GetUserSchemeAsync(ThemeA));

        // The site's default.
        await store.SetDefaultAsync(ThemeA, "a-one");
        Assert.Equal("a-one", await service.GetSchemeAttributeAsync(ThemeA));

        // The user's pick, per theme: a cookie of another theme, or naming a scheme of another theme, is ignored.
        httpContext.Request.Headers.Cookie = "Default-ThemeA-scheme=a-two; Default-ThemeB-scheme=a-one";
        Assert.Equal("a-two", await service.GetUserSchemeAsync(ThemeA));
        Assert.Equal("a-two", await service.GetSchemeAttributeAsync(ThemeA));
        Assert.Null(await service.GetUserSchemeAsync(ThemeB));
        Assert.Equal("b-one", await service.GetSchemeAttributeAsync(ThemeB));

        // A theme that does not support schemes has none.
        Assert.Null(await service.GetSchemeAttributeAsync(Unregistered));
        Assert.Empty(await service.ListAsync(Unregistered));
    }

    [Fact]
    public async Task Service_LinksToTheStylesheetOfTheTheme_VersionedByTheDocument()
    {
        var (store, documents) = CreateStore();
        var service = new ThemeSchemeService(new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, new ShellSettings { Name = "Default" }, store);
        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Content(It.IsAny<string>())).Returns((string path) => path.Replace("~/", "/", StringComparison.Ordinal));

        var link = await service.GetStylesheetUrlAsync(ThemeA, url.Object);

        Assert.Equal($"/Themes/schemes/ThemeA.css?v={documents.Document.Identifier}", link);
        Assert.Null(await service.GetStylesheetUrlAsync(Unregistered, url.Object));
    }

    [Fact]
    public async Task Controller_ServesOnlyRegisteredThemes()
    {
        var (store, documents) = CreateStore();
        var controller = CreateController(store);
        var model = new ThemeSchemeViewModel { Name = "X", FontFamily = ThemeSchemeTypography.Lato };

        Assert.IsType<NotFoundResult>(await controller.Index(Unregistered));
        Assert.IsType<NotFoundResult>(await controller.Create(Unregistered));
        Assert.IsType<NotFoundResult>(await controller.CreatePost(Unregistered, model));
        Assert.IsType<NotFoundResult>(await controller.Edit(Unregistered, "a-one"));
        Assert.IsType<NotFoundResult>(await controller.EditPost(Unregistered, "a-one", model));
        Assert.IsType<NotFoundResult>(await controller.Delete(Unregistered, "a-one"));
        Assert.IsType<NotFoundResult>(await controller.SetDefault(Unregistered, "a-one"));
        Assert.IsType<NotFoundResult>(await controller.Preview(Unregistered, model));
        Assert.False(documents.Document.Themes.ContainsKey(Unregistered));

        // A scheme of another theme is not found through this one.
        Assert.IsType<NotFoundResult>(await controller.Edit(ThemeB, "a-one"));
    }

    [Fact]
    public async Task ThemeIds_MatchWithoutCase_AndAreStoredAsTheThemeSpellsThem()
    {
        var (store, documents) = CreateStore();
        var upper = ThemeA.ToUpperInvariant();
        var lower = ThemeA.ToLowerInvariant();

        Assert.True(store.IsRegistered(lower));
        Assert.Equal(ThemeA, store.GetThemeId(upper));

        await store.SaveAsync(upper, Scheme("custom", "Custom"));

        Assert.Contains("custom", (await store.ListAsync(lower)).Select(scheme => scheme.Id));
        Assert.True(documents.Document.Themes.ContainsKey(ThemeA));
        Assert.False(documents.Document.Themes.ContainsKey(upper));

        // The page of a theme opened with another case uses the theme's own spelling.
        var view = Assert.IsType<ViewResult>(await CreateController(store).Index(lower));
        var model = Assert.IsType<ThemeSchemesIndexViewModel>(view.Model);

        Assert.Equal(ThemeA, model.ThemeId);
        Assert.Equal("Theme A", model.ThemeName);

        var options = new ThemeSchemesOptions();
        options.Themes.Add(ThemeA);
        Assert.Contains(lower, options.Themes);
    }

    [Fact]
    public async Task Controller_ListsTheSchemesOfTheTheme_WithItsDefault()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);

        var view = Assert.IsType<ViewResult>(await controller.Index(ThemeA));
        var model = Assert.IsType<ThemeSchemesIndexViewModel>(view.Model);

        Assert.Equal("Theme A", model.ThemeName);
        Assert.Equal(["a-one", "a-two"], model.List.Source.Select(entry => entry.Scheme.Id));
        Assert.All(model.List.Source, entry => Assert.Equal(ThemeA, entry.ThemeId));
        Assert.Equal("a-two", Assert.Single(model.List.Source, entry => entry.IsDefault).Scheme.Id);
    }

    [Fact]
    public async Task Controller_RefusesToDeleteTheDefault_AndSetsTheDefault()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);

        Assert.IsType<RedirectToActionResult>(await controller.Delete(ThemeA, "a-two"));
        Assert.NotNull(await store.FindAsync(ThemeA, "a-two"));

        Assert.IsType<RedirectToActionResult>(await controller.SetDefault(ThemeA, "a-one"));
        Assert.Equal("a-one", await store.GetDefaultIdAsync(ThemeA));

        Assert.IsType<RedirectToActionResult>(await controller.Delete(ThemeA, "a-two"));
        Assert.Null(await store.FindAsync(ThemeA, "a-two"));
    }

    [Fact]
    public async Task Controller_PreviewsWithTheProviderOfTheTheme()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);

        var result = Assert.IsType<ContentResult>(await controller.Preview(ThemeB, new ThemeSchemeViewModel { Name = "P" }));

        Assert.Contains($"[data-b-scheme=\"{SchemesController.PreviewSchemeId}\"]", result.Content, StringComparison.Ordinal);
        Assert.IsType<BadRequestResult>(await controller.Preview(ThemeB, new ThemeSchemeViewModel { Name = "P", Primary = "blue" }));
    }

    [Fact]
    public async Task DeploymentAndRecipe_RoundTripEverySetting_AndTheDefaults()
    {
        var (sourceStore, _) = CreateStore();
        var scheme = new ThemeScheme
        {
            Id = "custom-round-trip", Name = "Round trip", Position = 3,
            Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", LightMenu = true,
            Hue = 200, Saturation = 0.5, OnPrimary = "#ffffff", Page = "#fafafa", Toolbar = "#123456", Tint = "#abcdef", TintDark = "#fedcba",
            FontFamily = ThemeSchemeTypography.SourceSans3, HeadingFontFamily = ThemeSchemeTypography.Inter,
            FontScale = 1.0625, RadiusScale = 0.5, Density = 0.75,
        };
        await sourceStore.SaveAsync(ThemeB, scheme);
        await sourceStore.SetDefaultAsync(ThemeB, scheme.Id);

        var result = new DeploymentPlanResult(new MemoryFileBuilder(), new RecipeDescriptor());
        await new ThemeSchemesDeploymentSource(sourceStore).ProcessDeploymentStepAsync(new ThemeSchemesDeploymentStep(), result);

        // Through text, as a recipe file is read.
        var step = Assert.IsType<JsonObject>(JsonNode.Parse(Assert.Single(result.Steps).ToJsonString()));
        Assert.Equal("ThemeSchemes", step["name"]?.GetValue<string>());
        Assert.Equal(ThemeB, step["Schemes"].AsArray().Single(entry => entry["ItemId"]?.GetValue<string>() == scheme.Id)["ThemeId"]?.GetValue<string>());
        Assert.Equal("a-two", step["Defaults"]?[ThemeA]?.GetValue<string>());

        var (targetStore, _) = CreateStore();
        var context = new RecipeExecutionContext { Name = "ThemeSchemes", Step = step };
        await new ThemeSchemesStep(targetStore, CreateLocalizer()).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        var imported = await targetStore.FindAsync(ThemeB, scheme.Id);
        Assert.NotNull(imported);
        Assert.Equivalent(scheme, imported, strict: true);
        Assert.Null(await targetStore.FindAsync(ThemeA, scheme.Id));
        Assert.Equal(scheme.Id, await targetStore.GetDefaultIdAsync(ThemeB));
    }

    [Fact]
    public async Task Recipe_SkipsThemesThatAreNotRegistered_AndInvalidSchemes()
    {
        var (store, documents) = CreateStore();
        var step = JsonNode.Parse("""
            {
              "name": "ThemeSchemes",
              "Schemes": [
                { "ThemeId": "NoSchemes", "ItemId": "custom-elsewhere", "Name": "Elsewhere", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef" },
                { "ItemId": "custom-no-theme", "Name": "No theme", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef" },
                { "ThemeId": "ThemeA", "ItemId": "custom-font", "Name": "Font", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontFamily": "comic-sans" },
                { "ThemeId": "ThemeA", "ItemId": "custom-scale", "Name": "Scale", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontScale": 2 },
                { "ThemeId": "ThemeA", "ItemId": "custom-ok", "Name": "Ok", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontFamily": "system", "Density": 1.25 }
              ],
              "Defaults": { "NoSchemes": "custom-elsewhere", "ThemeA": "custom-ok", "ThemeB": "missing" }
            }
            """).AsObject();
        var context = new RecipeExecutionContext { Name = "ThemeSchemes", Step = step };

        await new ThemeSchemesStep(store, CreateLocalizer()).ExecuteAsync(context);

        Assert.Equal(6, context.Errors.Count);
        Assert.False(documents.Document.Themes.ContainsKey(Unregistered));
        Assert.Null(await store.FindAsync(ThemeA, "custom-no-theme"));
        Assert.Null(await store.FindAsync(ThemeA, "custom-font"));
        Assert.Null(await store.FindAsync(ThemeA, "custom-scale"));
        Assert.Equal(ThemeSchemeTypography.SystemFont, (await store.FindAsync(ThemeA, "custom-ok"))?.FontFamily);
        Assert.Equal("custom-ok", await store.GetDefaultIdAsync(ThemeA));
        Assert.Equal("b-one", await store.GetDefaultIdAsync(ThemeB));
    }

    [Theory]
    [InlineData(ThemeA, true, true)]
    [InlineData(ThemeB, true, true)]
    [InlineData(Unregistered, true, false)]
    [InlineData(ThemeA, false, false)]
    public async Task SchemesButton_RendersOnlyForRegisteredThemes_ForUsersWhoCanApplyThemes(string themeId, bool authorized, bool rendered)
    {
        var options = new ThemeSchemesOptions();
        options.Themes.Add(ThemeA);
        options.Themes.Add(ThemeB);

        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "TestAuth")) },
        };

        // The driver is internal to the module.
        var driverType = typeof(ThemeSchemeStore).Assembly.GetType("OrchardCore.Themes.Schemes.Drivers.ThemeEntrySchemesDisplayDriver", throwOnError: true);
        var driver = Assert.IsAssignableFrom<DisplayDriver<ThemeEntry>>(Activator.CreateInstance(driverType, authorizationService.Object, httpContextAccessor, Options.Create(options)));

        var extension = new Mock<IExtensionInfo>();
        extension.SetupGet(x => x.Id).Returns(themeId);

        var row = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        var context = new BuildDisplayContext(row, OrchardCoreConstants.DisplayType.SummaryAdmin, "", CreateShapeFactory(), null, null);

        await driver.Display(new ThemeEntry(extension.Object), context).ApplyAsync(context);

        Assert.Equal(rendered, row.Zones.IsNotEmpty("FooterEnd"));
    }

    private static (ThemeSchemeStore Store, InMemoryThemeSchemesDocumentManager Documents) CreateStore()
    {
        var documents = new InMemoryThemeSchemesDocumentManager();

        return (new ThemeSchemeStore(documents, [new FakeProviderA(), new FakeProviderB()], new MemoryCache(new MemoryCacheOptions())), documents);
    }

    private static SchemesController CreateController(ThemeSchemeStore store)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var manifest = new Mock<IManifestInfo>();
        manifest.SetupGet(x => x.Name).Returns("Theme A");
        var extension = new Mock<IExtensionInfo>();
        extension.SetupGet(x => x.Exists).Returns(true);
        extension.SetupGet(x => x.Manifest).Returns(manifest.Object);
        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(x => x.GetExtension(ThemeA)).Returns(extension.Object);

        var htmlLocalizer = new Mock<IHtmlLocalizer<SchemesController>>();
        htmlLocalizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedHtmlString(name, string.Format(CultureInfo.InvariantCulture, name, arguments), false, arguments));

        var stringLocalizer = new Mock<IStringLocalizer<SchemesController>>();
        stringLocalizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        return new SchemesController(
            authorizationService.Object,
            store,
            extensionManager.Object,
            Mock.Of<INotifier>(),
            htmlLocalizer.Object,
            stringLocalizer.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "TestAuth")),
                },
            },
        };
    }

    private static IStringLocalizer<ThemeSchemesStep> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<ThemeSchemesStep>>();
        localizer
            .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(CultureInfo.InvariantCulture, name, arguments)));

        return localizer.Object;
    }

    private static IShapeFactory CreateShapeFactory()
    {
        // Calls the default implementation of the generic overloads, which call the setup below.
        var shapeFactory = new Mock<IShapeFactory> { CallBase = true };
        shapeFactory
            .Setup(x => x.CreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<ValueTask<IShape>>>(),
                It.IsAny<Action<ShapeCreatingContext>>(),
                It.IsAny<Action<ShapeCreatedContext>>()))
            .Returns((string shapeType, Func<ValueTask<IShape>> factory, Action<ShapeCreatingContext> creating, Action<ShapeCreatedContext> created) => factory());

        return shapeFactory.Object;
    }

    private static ThemeScheme Scheme(string id, string name, int position = 10)
        => new() { Id = id, Name = name, Position = position, Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" };

    private abstract class FakeProvider : IThemeSchemeProvider
    {
        public int Seeds { get; private set; }

        public abstract string ThemeId { get; }

        public abstract string DefaultSchemeId { get; }

        public abstract string SchemeAttributeName { get; }

        protected abstract IEnumerable<string> DefaultIds { get; }

        public IReadOnlyList<ThemeScheme> CreateDefaults()
        {
            Seeds++;

            return [.. DefaultIds.Select((id, position) => Scheme(id, id.ToUpperInvariant(), position))];
        }

        public void AppendCss(StringBuilder css, string schemeId, ThemeScheme scheme)
        {
            if (ThemeSchemeStore.HasValidColors(scheme))
            {
                css.Append('[').Append(SchemeAttributeName).Append("=\"").Append(schemeId).Append("\"]{--primary:").Append(scheme.Primary).Append("}\n");
            }
        }
    }

    private sealed class FakeProviderA : FakeProvider
    {
        public override string ThemeId => ThemeA;

        public override string DefaultSchemeId => "a-two";

        public override string SchemeAttributeName => "data-a-scheme";

        protected override IEnumerable<string> DefaultIds => ["a-one", "a-two"];
    }

    private sealed class FakeProviderB : FakeProvider
    {
        public override string ThemeId => ThemeB;

        public override string DefaultSchemeId => "b-one";

        public override string SchemeAttributeName => "data-b-scheme";

        protected override IEnumerable<string> DefaultIds => ["b-one"];
    }
}
