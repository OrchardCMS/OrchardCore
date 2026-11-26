using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Deployment;
using OrchardCore.Documents;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;
using OrchardCore.Themes.Models;
using OrchardCore.Themes.TheAdmin.Schemes;
using OrchardCore.Themes.TheAdmin.Controllers;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Recipes;
using OrchardCore.Themes.TheAdmin.Services;
using OrchardCore.Themes.TheAdmin.ViewModels;

namespace OrchardCore.Tests.Themes.TheAdmin;

/// <summary>
/// The schemes of TheAdmin: their validation, the store, the stylesheet and its cache, the runtime service, the
/// controller, the recipe and deployment steps, and the Schemes button of the theme's card.
/// </summary>
public sealed class AdminSchemeTests
{
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
    public void IsValidId_SchemeId_AcceptsOnlySafeCssValues(string id, bool valid)
        => Assert.Equal(valid, AdminSchemeStore.IsValidId(id));

    [Fact]
    public void NewId_Generated_IsValid()
        => Assert.True(AdminSchemeStore.IsValidId(AdminSchemeStore.NewId()));

    [Theory]
    [InlineData(null, null, null, null, null, true)]
    [InlineData("lato", "system", 0.875, 0.0, 0.75, true)]
    [InlineData("Georgia", null, 1.125, 1.5, 1.25, true)]
    [InlineData("Lato", "Georgia, serif", null, null, null, true)]
    [InlineData("'Open Sans', \"Segoe UI\", sans-serif", "comic-sans", null, null, null, true)]
    [InlineData("Comic Sans; color: red", null, null, null, null, false)]
    [InlineData("Arial\"}body{color:red", null, null, null, null, false)]
    [InlineData("Arial,,serif", null, null, null, null, false)]
    [InlineData(null, "papyrus{", null, null, null, false)]
    [InlineData(null, null, 0.8, null, null, false)]
    [InlineData(null, null, 1.2, null, null, false)]
    [InlineData(null, null, null, 1.6, null, false)]
    [InlineData(null, null, null, -0.5, null, false)]
    [InlineData(null, null, null, null, 0.5, false)]
    [InlineData(null, null, null, null, 1.5, false)]
    public void HasValidTypography_FontsAndScales_AcceptsFontNamesWithinTheRanges(string font, string heading, double? fontScale, double? radiusScale, double? density, bool valid)
        => Assert.Equal(valid, AdminSchemeStore.HasValidTypography(new AdminScheme
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
    public void HasValidColors_SchemeColors_AcceptsOnlyHexColors(string primary, string page, bool valid)
        => Assert.Equal(valid, AdminSchemeStore.HasValidColors(new AdminScheme { Primary = primary, Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", Page = page }));

    [Theory]
    [InlineData(1, 1, 1, true)]
    [InlineData(0.875, 0, 0.75, true)]
    [InlineData(1.125, 1.5, 1.25, true)]
    [InlineData(0.5, 1, 1, false)]
    [InlineData(1, 2, 1, false)]
    [InlineData(1, 1, 2, false)]
    public void Validate_EditorScales_RejectsValuesOutOfRange(double fontScale, double radiusScale, double density, bool valid)
    {
        var model = new AdminSchemeViewModel { Name = "A", FontScale = fontScale, RadiusScale = radiusScale, Density = density };

        Assert.Equal(valid, Validator.TryValidateObject(model, new ValidationContext(model), [], validateAllProperties: true));
    }

    [Fact]
    public void Steps_OfferedByTheEditor_AreAllValid()
    {
        Assert.All(AdminSchemeTypography.FontScales, value => Assert.True(AdminSchemeTypography.IsValidFontScale(value)));
        Assert.All(AdminSchemeTypography.RadiusScales, value => Assert.True(AdminSchemeTypography.IsValidRadiusScale(value)));
        Assert.All(AdminSchemeTypography.Densities, value => Assert.True(AdminSchemeTypography.IsValidDensity(value)));
    }

    [Theory]
    [InlineData("lato", "\"Lato\", system-ui")]
    [InlineData("LATO", "\"Lato\", system-ui")]
    [InlineData("Lato", "\"Lato\", system-ui")]
    [InlineData("System font", "system-ui, -apple-system")]
    [InlineData("system", "system-ui, -apple-system")]
    [InlineData("Georgia", "\"Georgia\", system-ui, -apple-system")]
    [InlineData(" Georgia , serif ", "\"Georgia\", serif")]
    [InlineData("'Open Sans', \"Segoe UI\", sans-serif", "\"Open Sans\", \"Segoe UI\", sans-serif")]
    [InlineData("ui-monospace, Consolas", "ui-monospace, \"Consolas\", system-ui")]
    public void Fonts_AreTheThemesOwn_OrTheFamiliesTyped(string value, string stack)
    {
        var fonts = new[] { new AdminSchemeFont("lato", "Lato", "\"Lato\", system-ui") };

        Assert.StartsWith(stack, AdminSchemeTypography.GetFontStack(value, fonts), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    [InlineData("Comic Sans; color: red")]
    [InlineData("a}b")]
    public void Fonts_ThatAreEmptyOrNotFontNames_HaveNoStack_AndTheThemesDefaultIsUsed(string value)
    {
        Assert.Null(AdminSchemeTypography.GetFontStack(value));
        Assert.StartsWith("\"Lato\", system-ui", TheAdminSchemes.GetFontStack(value), StringComparison.Ordinal);
    }

    [Fact]
    public void Fonts_ShowByTheNameOfTheThemesFont_OrAsTyped()
    {
        var fonts = new[] { new AdminSchemeFont("source-sans-3", "Source Sans 3", "x") };

        Assert.Equal("Source Sans 3", AdminSchemeTypography.GetDisplayName("source-sans-3", fonts));
        Assert.Equal("System font", AdminSchemeTypography.GetDisplayName("system", fonts));
        Assert.Equal("Georgia, serif", AdminSchemeTypography.GetDisplayName(" Georgia, serif ", fonts));
        Assert.Null(AdminSchemeTypography.GetDisplayName(null, fonts));
    }

    [Fact]
    public async Task Stylesheet_IsBuiltOnce_AcrossRequests_AndAgainAfterASave()
    {
        var (store, _) = CreateStore();
        var service = CreateService(store, new DefaultHttpContext());

        var (css, version) = await store.GetStylesheetAsync();
        var link = await service.GetStylesheetUrlAsync(CreateUrlHelper());

        // Read from memory: the same text, not built again.
        Assert.Same(css, (await store.GetStylesheetAsync()).Css);
        Assert.Equal($"/TheAdmin/schemes.css?v={version}", link);

        await store.SaveAsync(Scheme("custom-new", "New"));

        var (rebuilt, _) = await store.GetStylesheetAsync();

        Assert.NotSame(css, rebuilt);
        Assert.Contains("[data-ta-scheme=\"custom-new\"]", rebuilt, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StylesheetVersion_IsAHashOfTheCss_StableForTheSameData()
    {
        var (store, documents) = CreateStore();

        var (css, version) = await store.GetStylesheetAsync();

        Assert.Equal(AdminSchemeStylesheet.ComputeVersion(css), version);
        Assert.Matches("^[0-9a-f]{16}$", version);

        // Another store, over the same schemes, with a cache of its own: the same version.
        var (other, _) = CreateStore(documents);

        Assert.Equal(version, (await other.GetStylesheetAsync()).Version);
    }

    [Fact]
    public async Task GetStylesheet_AfterASave_ChangesTheVersion()
    {
        var (store, _) = CreateStore();
        var (_, before) = await store.GetStylesheetAsync();

        await store.SaveAsync(Scheme("custom-new", "New"));

        Assert.NotEqual(before, (await store.GetStylesheetAsync()).Version);
    }

    [Fact]
    public void Build_InvalidIdOrColor_SkipsTheScheme()
    {
        var badColor = Scheme("custom-bad-color", "Bad color");
        badColor.Primary = "blue";

        var css = AdminSchemeStylesheet.Build([Scheme("custom-ok", "Ok"), Scheme("Bad Id", "Bad"), badColor]);

        Assert.Contains("[data-ta-scheme=\"custom-ok\"]", css, StringComparison.Ordinal);
        Assert.DoesNotContain("Bad Id", css, StringComparison.Ordinal);
        Assert.DoesNotContain("custom-bad-color", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_AnyValidScheme_StartsWithTheCreditOfBootstrapsIcons()
    {
        var css = AdminSchemeStylesheet.Build([Scheme("custom-ok", "Ok")]);

        // The rules draw Bootstrap's check box, radio button and switch icons: the stylesheet keeps its MIT notice, in a
        // comment minifiers keep.
        Assert.StartsWith(AdminSchemeStylesheet.Banner, css, StringComparison.Ordinal);
        Assert.StartsWith("/*!", AdminSchemeStylesheet.Banner, StringComparison.Ordinal);
        Assert.Contains("The Bootstrap Authors, Licensed under MIT", css, StringComparison.Ordinal);
        Assert.Equal(1, css.Split("The Bootstrap Authors").Length - 1);

        // No scheme, no icon, and no credit.
        Assert.Empty(AdminSchemeStylesheet.Build([]));
    }

    [Fact]
    public async Task Store_SeedsNothingWhenRead_AndFallsBackToGray_WhenThereIsNoScheme()
    {
        // No migration ran: nothing is stored, and reading never writes.
        var documents = new InMemoryAdminSchemesDocumentManager();
        var (store, _) = CreateStore(documents);

        Assert.Empty(await store.ListAsync());
        Assert.Equal(0, documents.Updates);

        // The admin is not broken: it falls back to Gray, written in the stylesheet but never stored, and the page still
        // names that scheme.
        var (css, _) = await store.GetStylesheetAsync();

        Assert.Equal(TheAdminSchemes.DefaultId, await store.GetDefaultIdAsync());
        Assert.Contains($"[data-ta-scheme=\"{TheAdminSchemes.DefaultId}\"]", css, StringComparison.Ordinal);
        Assert.Equal(TheAdminSchemes.DefaultId, await CreateService(store, new DefaultHttpContext()).GetSchemeAttributeAsync());
        Assert.Empty(documents.Document.Schemes);
        Assert.Equal(0, documents.Updates);
    }

    [Fact]
    public async Task MigrationRecipe_ImportsTheThemesSchemes_AndMakesGrayTheDefault()
    {
        var documents = new InMemoryAdminSchemesDocumentManager();
        var (store, _) = CreateStore(documents);
        var context = new RecipeExecutionContext { Name = "AdminSchemes", Step = AdminSchemesRecipe.ReadStep() };

        await new AdminSchemesStep(store, CreateLocalizer()).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        Assert.Equal(s_defaultIds, (await store.ListAsync()).Select(scheme => scheme.Id));
        Assert.Equal("gray", documents.Document.DefaultSchemeId);
        Assert.Equal("gray", await store.GetDefaultIdAsync());
        Assert.Equivalent(TheAdminSchemes.CreateFallback(), await store.FindAsync("gray"), strict: true);
    }

    [Fact]
    public async Task Store_DefaultIs_ThePickedScheme_ThenTheThemesDefault_ThenTheFirst()
    {
        var (store, documents) = CreateStore();

        Assert.Equal("gray", await store.GetDefaultIdAsync());

        Assert.True(await store.SetDefaultAsync("blue"));
        Assert.Equal("blue", await store.GetDefaultIdAsync());
        Assert.Equal("blue", documents.Document.DefaultSchemeId);
        Assert.False(await store.SetDefaultAsync("missing"));

        // A picked default that is gone (e.g. removed by a recipe) falls back on the theme's default.
        documents.Document.Schemes.Remove("blue");
        Assert.Equal("gray", await store.GetDefaultIdAsync());

        // And when that is gone too, on the first scheme.
        documents.Document.Schemes.Remove("gray");
        Assert.Equal("ocean", await store.GetDefaultIdAsync());
    }

    [Fact]
    public async Task Store_RefusesToDeleteTheDefault_AndSchemesWithAnInvalidId()
    {
        var (store, _) = CreateStore();

        Assert.False(await store.DeleteAsync("gray"));
        Assert.NotNull(await store.FindAsync("gray"));

        Assert.True(await store.SetDefaultAsync("blue"));
        Assert.False(await store.DeleteAsync("blue"));
        Assert.True(await store.DeleteAsync("gray"));
        Assert.Null(await store.FindAsync("gray"));

        await Assert.ThrowsAsync<ArgumentException>(() => store.SaveAsync(Scheme("Bad Id", "Bad")));
    }

    [Fact]
    public async Task Service_ResolvesTheUsersCookie_ThenTheSiteDefault_ThenTheThemesDefault()
    {
        var (store, _) = CreateStore();
        var httpContext = new DefaultHttpContext();
        var service = CreateService(store, httpContext);

        Assert.Equal("Default-TheAdmin-scheme", service.CookieName);

        // No cookie, no site default: the theme's default.
        Assert.Equal("gray", await service.GetSchemeAttributeAsync());
        Assert.Null(await service.GetUserSchemeAsync());

        // The site's default.
        await store.SetDefaultAsync("blue");
        Assert.Equal("blue", await service.GetSchemeAttributeAsync());

        // The user's pick.
        httpContext.Request.Headers.Cookie = "Default-TheAdmin-scheme=plum";
        Assert.Equal("plum", await service.GetUserSchemeAsync());
        Assert.Equal("plum", await service.GetSchemeAttributeAsync());

        // A pick that was deleted since, or that is not a scheme id, is ignored.
        httpContext.Request.Headers.Cookie = "Default-TheAdmin-scheme=missing";
        Assert.Null(await service.GetUserSchemeAsync());
        httpContext.Request.Headers.Cookie = "Default-TheAdmin-scheme=Bad%20Id";
        Assert.Equal("blue", await service.GetSchemeAttributeAsync());
    }

    [Fact]
    public async Task Actions_MissingScheme_ReturnNotFound()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);
        var model = new AdminSchemeViewModel { Name = "X", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" };

        Assert.IsType<NotFoundResult>(await controller.Edit("missing"));
        Assert.IsType<NotFoundResult>(await controller.EditPost("missing", model));
        Assert.IsType<NotFoundResult>(await controller.Delete("missing"));
        Assert.IsType<NotFoundResult>(await controller.SetDefault("missing"));
    }

    [Fact]
    public async Task Controller_ListsTheSchemes_WithTheDefault()
    {
        var (store, _) = CreateStore();

        var view = Assert.IsType<ViewResult>(await CreateController(store).Index());
        var model = Assert.IsType<AdminSchemesIndexViewModel>(view.Model);

        Assert.Equal(s_defaultIds, model.List.Source.Select(entry => entry.Scheme.Id));
        Assert.Equal("gray", Assert.Single(model.List.Source, entry => entry.IsDefault).Scheme.Id);
    }

    [Fact]
    public async Task Controller_RefusesToDeleteTheDefault_AndSetsTheDefault()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);

        Assert.IsType<RedirectToActionResult>(await controller.Delete("gray"));
        Assert.NotNull(await store.FindAsync("gray"));

        Assert.IsType<RedirectToActionResult>(await controller.SetDefault("blue"));
        Assert.Equal("blue", await store.GetDefaultIdAsync());

        Assert.IsType<RedirectToActionResult>(await controller.Delete("gray"));
        Assert.Null(await store.FindAsync("gray"));
    }

    [Fact]
    public async Task Controller_SavesEverySetting_AndStoresTheThemesFontAsNone()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);

        Assert.IsType<RedirectToActionResult>(await controller.CreatePost(new AdminSchemeViewModel
        {
            Name = " Harbor ", Primary = "#1F6F8B", Deep = "#123c4c", Ink = "#081a21", Accent = "#5bc0de", LightMenu = true,
            Toolbar = " ", FontFamily = "lato", HeadingFontFamily = "Georgia, serif", FontScale = 1.0625, RadiusScale = 0.5, Density = 0.75,
            Position = 8,
        }));

        var saved = Assert.Single(await store.ListAsync(), scheme => scheme.Name == "Harbor");

        Assert.StartsWith(AdminSchemeStore.IdPrefix, saved.Id, StringComparison.Ordinal);
        Assert.Equal("#1f6f8b", saved.Primary);
        Assert.True(saved.LightMenu);
        Assert.Null(saved.Toolbar);
        // Lato is the theme's font, the default: the scheme names none, and its field is left empty.
        Assert.Null(saved.FontFamily);
        Assert.Equal("Georgia, serif", saved.HeadingFontFamily);
        Assert.Equal(1.0625, saved.FontScale);
        Assert.Equal(0.5, saved.RadiusScale);
        Assert.Equal(0.75, saved.Density);

        var edit = Assert.IsType<ViewResult>(await controller.Edit(saved.Id));

        Assert.Null(Assert.IsType<AdminSchemeViewModel>(edit.Model).FontFamily);

        // Another font is kept.
        Assert.IsType<RedirectToActionResult>(await controller.EditPost(saved.Id, new AdminSchemeViewModel
        {
            Name = "Harbor", Primary = "#1f6f8b", Deep = "#123c4c", Ink = "#081a21", Accent = "#5bc0de", FontFamily = " Georgia ",
        }));
        Assert.Equal("Georgia", (await store.FindAsync(saved.Id)).FontFamily);

        // A font that is not a font name is refused.
        var invalid = new AdminSchemeViewModel { Name = "X", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", FontFamily = "Arial; color: red" };

        Assert.IsType<ViewResult>(await controller.EditPost(saved.Id, invalid));
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal("Georgia", (await store.FindAsync(saved.Id)).FontFamily);
    }

    [Fact]
    public async Task Preview_EditedScheme_ReturnsItsCss()
    {
        var (store, _) = CreateStore();
        var controller = CreateController(store);

        var result = Assert.IsType<ContentResult>(await controller.Preview(new AdminSchemeViewModel { Name = "P", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" }));

        Assert.Contains($"[data-ta-scheme=\"{SchemesController.PreviewSchemeId}\"]", result.Content, StringComparison.Ordinal);
        Assert.IsType<BadRequestResult>(await controller.Preview(new AdminSchemeViewModel { Name = "P", Primary = "blue" }));
    }

    [Fact]
    public async Task DeploymentAndRecipe_RoundTripEverySetting_AndTheDefault()
    {
        var (sourceStore, _) = CreateStore();
        var scheme = new AdminScheme
        {
            Id = "custom-round-trip", Name = "Round trip", Position = 3,
            Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", LightMenu = true,
            Hue = 200, Saturation = 0.5, OnPrimary = "#ffffff", Page = "#fafafa", Toolbar = "#123456", Tint = "#abcdef", TintDark = "#fedcba",
            FontFamily = "Lato", HeadingFontFamily = "Georgia, serif",
            FontScale = 1.0625, RadiusScale = 0.5, Density = 0.75,
        };
        await sourceStore.SaveAsync(scheme);
        await sourceStore.SetDefaultAsync(scheme.Id);

        var result = new DeploymentPlanResult(new MemoryFileBuilder(), new RecipeDescriptor());
        await new AdminSchemesDeploymentSource(sourceStore).ProcessDeploymentStepAsync(new AdminSchemesDeploymentStep(), result);

        // Through text, as a recipe file is read.
        var step = Assert.IsType<JsonObject>(JsonNode.Parse(Assert.Single(result.Steps).ToJsonString()));
        Assert.Equal("AdminSchemes", step["name"]?.GetValue<string>());
        Assert.Equal(s_defaultIds.Length + 1, step["Schemes"].AsArray().Count);
        Assert.Contains(step["Schemes"].AsArray(), entry => entry["ItemId"]?.GetValue<string>() == scheme.Id);
        Assert.Equal(scheme.Id, step["DefaultSchemeId"]?.GetValue<string>());

        var (targetStore, _) = CreateStore();
        var context = new RecipeExecutionContext { Name = "AdminSchemes", Step = step };
        await new AdminSchemesStep(targetStore, CreateLocalizer()).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        Assert.Equivalent(scheme, await targetStore.FindAsync(scheme.Id), strict: true);
        Assert.Equal(scheme.Id, await targetStore.GetDefaultIdAsync());
    }

    [Fact]
    public async Task Recipe_AddsSchemesAfterTheOthers_AndSkipsInvalidOnes()
    {
        var (store, _) = CreateStore();
        var step = JsonNode.Parse("""
            {
              "name": "AdminSchemes",
              "Schemes": [
                { "ItemId": "custom-font", "Name": "Font", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontFamily": "Comic Sans; color: red" },
                { "ItemId": "custom-scale", "Name": "Scale", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontScale": 2 },
                { "ItemId": "Bad Id", "Name": "Bad id", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef" },
                { "ItemId": "custom-no-name", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef" },
                { "ItemId": "custom-ok", "Name": "Ok", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontFamily": "system", "Density": 1.25 },
                { "Name": "No id", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef" }
              ],
              "DefaultSchemeId": "missing"
            }
            """).AsObject();
        var context = new RecipeExecutionContext { Name = "AdminSchemes", Step = step };

        await new AdminSchemesStep(store, CreateLocalizer()).ExecuteAsync(context);

        Assert.Equal(5, context.Errors.Count);
        Assert.Null(await store.FindAsync("custom-font"));
        Assert.Null(await store.FindAsync("custom-scale"));
        Assert.Null(await store.FindAsync("custom-no-name"));

        var ok = await store.FindAsync("custom-ok");
        Assert.NotNull(ok);
        Assert.Equal(AdminSchemeTypography.SystemFont, ok.FontFamily);
        Assert.True(ok.Position >= s_defaultIds.Length);

        var noId = Assert.Single(await store.ListAsync(), scheme => scheme.Name == "No id");
        Assert.StartsWith(AdminSchemeStore.IdPrefix, noId.Id, StringComparison.Ordinal);
        Assert.Equal("gray", await store.GetDefaultIdAsync());
    }

    [Theory]
    [InlineData("\"Position\": \"2\"", "Position")]
    [InlineData("\"Position\": 1.5", "Position")]
    [InlineData("\"Hue\": \"210\"", "Hue")]
    [InlineData("\"LightMenu\": \"true\"", "LightMenu")]
    [InlineData("\"OnPrimary\": 42", "OnPrimary")]
    [InlineData("\"FontFamily\": [\"Georgia\"]", "FontFamily")]
    public async Task Recipe_ValueOfTheWrongType_ReportsTheSchemeAndImportsTheOthers(string property, string name)
    {
        var (store, _) = CreateStore();
        var step = JsonNode.Parse($$"""
            {
              "name": "AdminSchemes",
              "Schemes": [
                { "ItemId": "custom-typed", "Name": "Typed", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "Primary": "#2a81bb", {{property}} },
                { "ItemId": "custom-ok", "Name": "Ok", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "Position": 20, "Hue": 210.5, "LightMenu": true }
              ]
            }
            """).AsObject();
        var context = new RecipeExecutionContext { Name = "AdminSchemes", Step = step };

        await new AdminSchemesStep(store, CreateLocalizer()).ExecuteAsync(context);

        var error = Assert.Single(context.Errors);
        Assert.Contains(name, error, StringComparison.Ordinal);
        Assert.Null(await store.FindAsync("custom-typed"));

        var ok = await store.FindAsync("custom-ok");
        Assert.NotNull(ok);
        Assert.Equal(20, ok.Position);
        Assert.Equal(210.5, ok.Hue);
        Assert.True(ok.LightMenu);
    }

    [Theory]
    [InlineData("TheAdmin", "TheAdmin", true, true)]
    [InlineData("theadmin", "TheAdmin", true, true)]
    [InlineData("TheTheme", "TheAdmin", true, false)]
    [InlineData("TheAdmin", "TheAdmin", false, false)]
    // Another admin theme is in use: TheAdmin's shapes, the button's included, are not there.
    [InlineData("TheAdmin", "AdminThemeSample", true, false)]
    public async Task SchemesButton_RendersOnTheCardOfTheAdmin_WhileItIsTheTheme_ForUsersWhoCanApplyThemes(string themeId, string currentThemeId, bool authorized, bool rendered)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "TestAuth")) },
        };

        // The driver is internal to the theme.
        var driverType = typeof(AdminSchemeStore).Assembly.GetType("OrchardCore.Themes.TheAdmin.Drivers.ThemeEntrySchemesDisplayDriver", throwOnError: true);
        var currentTheme = new Mock<IExtensionInfo>();
        currentTheme.SetupGet(x => x.Id).Returns(currentThemeId);
        var themeManager = new Mock<IThemeManager>();
        themeManager.Setup(x => x.GetThemeAsync()).ReturnsAsync(currentTheme.Object);

        var driver = Assert.IsAssignableFrom<DisplayDriver<ThemeEntry>>(Activator.CreateInstance(driverType, authorizationService.Object, httpContextAccessor, themeManager.Object));

        var extension = new Mock<IExtensionInfo>();
        extension.SetupGet(x => x.Id).Returns(themeId);

        var row = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        var context = new BuildDisplayContext(row, OrchardCoreConstants.DisplayType.SummaryAdmin, "", CreateShapeFactory(), null, null);

        await driver.Display(new ThemeEntry(extension.Object), context).ApplyAsync(context);

        Assert.Equal(rendered, row.Zones.IsNotEmpty("FooterEnd"));
    }

    private static readonly string[] s_defaultIds = AdminSchemesRecipe.SchemeIds;

    // A store over the given document, or over one holding what the theme's migration imports, with a cache of its own.
    private static (AdminSchemeStore Store, InMemoryAdminSchemesDocumentManager Documents) CreateStore(InMemoryAdminSchemesDocumentManager documents = null)
    {
        documents ??= new InMemoryAdminSchemesDocumentManager(AdminSchemesRecipe.CreateDocument());

        return (new AdminSchemeStore(documents, new MemoryCache(new MemoryCacheOptions())), documents);
    }

    private static AdminSchemeService CreateService(AdminSchemeStore store, HttpContext httpContext)
        => new(new HttpContextAccessor { HttpContext = httpContext }, new ShellSettings { Name = "Default" }, store);

    private static IUrlHelper CreateUrlHelper()
    {
        var url = new Mock<IUrlHelper>();
        url.Setup(u => u.Content(It.IsAny<string>())).Returns((string path) => path.Replace("~/", "/", StringComparison.Ordinal));

        return url.Object;
    }

    private static SchemesController CreateController(AdminSchemeStore store)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

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

    private static IStringLocalizer<AdminSchemesStep> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<AdminSchemesStep>>();
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

    private static AdminScheme Scheme(string id, string name, int position = 10)
        => new() { Id = id, Name = name, Position = position, Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" };
}
