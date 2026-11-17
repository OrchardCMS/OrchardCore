using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Match = System.Text.RegularExpressions.Match;
using OrchardCore.Data.Documents;
using OrchardCore.Deployment;
using OrchardCore.Documents;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;
using OrchardCore.Themes.TheAdmin;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Recipes;
using OrchardCore.Themes.TheAdmin.Schemes;
using OrchardCore.Themes.TheAdmin.Services;
using OrchardCore.Themes.TheAdmin.ViewModels;

namespace OrchardCore.Tests.Themes.TheAdmin;

/// <summary>
/// TheAdmin derives the color tokens of its fallback scheme (Blue) in Sass and those of every scheme record in C#
/// (<see cref="AdminSchemeCssBuilder"/>). These tests keep both derivations in step, check that whatever colors an
/// admin picks the derived tokens meet WCAG 2.2 AA contrast, and cover the typography and shape tokens.
/// </summary>
public sealed class AdminSchemeTests
{
    // The definitions the theme's schemes were tuned with; the seeded records must reproduce them exactly.
    public static TheoryData<string> DefaultSchemes => ["blue", "gray", "ocean", "forest", "plum", "sunset", "graphite", "gold"];

    private static readonly Dictionary<string, AdminSchemeDefinition> s_builtIn = new(StringComparer.Ordinal)
    {
        ["blue"] = new() { Primary = Color("#2a81bb"), Deep = Color("#144c71"), Ink = Color("#081b26"), Accent = Color("#5aa4ef"), Hue = 209, Saturation = 1 },
        ["gray"] = new()
        {
            Primary = Color("#0d6efd"), Deep = Color("#212529"), Ink = Color("#212529"), Accent = Color("#6ea8fe"),
            Hue = 210, Saturation = 0.12, LightMenu = true, Page = Color("#fff"),
            Tint = Color("#495057"), TintDark = Color("#adb5bd"), Toolbar = Color("#495057"),
        },
        ["ocean"] = new() { Primary = Color("#12868b"), Deep = Color("#0d4a52"), Ink = Color("#062227"), Accent = Color("#4fd1c5"), Hue = 187, Saturation = 0.9 },
        ["forest"] = new() { Primary = Color("#2f855a"), Deep = Color("#1c4a36"), Ink = Color("#0b2018"), Accent = Color("#68d391"), Hue = 150, Saturation = 0.7 },
        ["plum"] = new() { Primary = Color("#7b4fc0"), Deep = Color("#3d2a66"), Ink = Color("#170f2b"), Accent = Color("#b794f4"), Hue = 262, Saturation = 0.7 },
        ["sunset"] = new() { Primary = Color("#c2551f"), Deep = Color("#5c2a1c"), Ink = Color("#24100b"), Accent = Color("#f6ad55"), Hue = 18, Saturation = 0.6 },
        ["graphite"] = new() { Primary = Color("#4b6584"), Deep = Color("#2d3540"), Ink = Color("#121519"), Accent = Color("#94a8bf"), Hue = 213, Saturation = 0.2 },
        ["gold"] = new()
        {
            Primary = Color("#eaa52c"), Deep = Color("#1e1e1e"), Ink = Color("#09080b"), Accent = Color("#eaa52c"),
            Hue = 240, Saturation = 0.08, OnPrimary = Color("#1e1e1e"),
        },
    };

    // The typography and shape tokens: the Sass writes them in a block of their own, and they are not colors.
    private static readonly HashSet<string> s_typographyTokens = new(StringComparer.Ordinal)
    {
        "--ta-font-family", "--ta-heading-font-family", "--ta-font-scale", "--ta-radius-scale", "--ta-density",
    };

    private static readonly Regex s_colorRegex = new(@"#[0-9a-fA-F]{3,8}\b|rgba?\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex s_numberRegex = new(@"-?\d+(\.\d+)?", RegexOptions.Compiled);

    // The stylesheet's fallback (Blue) is derived in Sass; every scheme record in C#. Both must agree.
    [Fact]
    public void CSharpDerivation_MatchesTheSassOne_InLightMode()
        => AssertSameTokens("blue", ReadBlock(":root", mustContain: "--ta-primary:"), AdminSchemeCssBuilder.BuildLight(s_builtIn["blue"]));

    [Fact]
    public void CSharpDerivation_MatchesTheSassOne_InDarkMode()
        => AssertSameTokens("blue", ReadBlock(":root[data-bs-theme=dark]", mustContain: "--ta-link:"), AdminSchemeCssBuilder.BuildDark(s_builtIn["blue"]));

    [Theory]
    [MemberData(nameof(DefaultSchemes))]
    public void SeededDefaults_ReproduceTheSchemesTheyWereTunedAs(string id)
    {
        var seed = AdminSchemes.CreateDefaults().Single(scheme => scheme.Id == id);
        var definition = AdminSchemeStore.ToDefinition(seed);

        Assert.NotNull(definition);
        Assert.Equal(AdminSchemeCssBuilder.BuildLight(s_builtIn[id]), AdminSchemeCssBuilder.BuildLight(definition));
        Assert.Equal(AdminSchemeCssBuilder.BuildDark(s_builtIn[id]), AdminSchemeCssBuilder.BuildDark(definition));
    }

    [Fact]
    public void SeededDefaults_HaveValidUniqueIds_AndAnOrder_StartingWithTheDefault()
    {
        var defaults = AdminSchemes.CreateDefaults();

        Assert.All(defaults, scheme => Assert.True(AdminSchemeStore.IsValidId(scheme.Id), scheme.Id));
        Assert.All(defaults, scheme => Assert.True(AdminSchemeStore.HasValidTypography(scheme), scheme.Id));
        Assert.Equal(defaults.Count, defaults.Select(scheme => scheme.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(defaults.OrderBy(scheme => scheme.Position).Select(scheme => scheme.Id), defaults.Select(scheme => scheme.Id));
        Assert.Equal("blue", AdminSchemes.DefaultId);
        Assert.Equal(["blue", "gray"], defaults.Take(2).Select(scheme => scheme.Id));
    }

    [Fact]
    public void SeededGray_KeepsTheSystemFontOfTheFormerTheAdmin()
    {
        var gray = AdminSchemes.CreateDefaults().Single(scheme => scheme.Id == "gray");
        var tokens = ToDictionary(AdminSchemeCssBuilder.BuildTypography(AdminSchemeStore.ToDefinition(gray)));

        Assert.StartsWith("system-ui,", tokens["--ta-font-family"], StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("#ffd400", "#3a2f00", "#120f00", "#ffe680")] // A bright yellow primary, which white text cannot sit on.
    [InlineData("#f0f4ff", "#1f2a44", "#0b1020", "#9fb4ff")] // A primary almost as pale as the page.
    [InlineData("#5b0f2a", "#2b0614", "#12020a", "#ff8fb8")] // A very dark primary.
    [InlineData("#00ff66", "#00331a", "#001208", "#66ffaa")] // A neon one.
    public void AnySchemeAnAdminPicks_MeetsWcagContrast(string primary, string deep, string ink, string accent)
    {
        foreach (var lightMenu in new[] { false, true })
        {
            var scheme = AdminSchemeDefinition.FromColors(Color(primary), Color(deep), Color(ink), Color(accent), lightMenu);
            var light = ToDictionary(AdminSchemeCssBuilder.BuildLight(scheme));
            var dark = ToDictionary(AdminSchemeCssBuilder.BuildDark(scheme));

            foreach (var (mode, tokens) in new[] { ("light", light), ("dark", Merge(light, dark)) })
            {
                var surfaces = new[] { "--ta-page-bg", "--ta-surface", "--ta-surface-muted", "--ta-list-header-bg" }
                    .Select(name => Color(tokens[name]))
                    .ToArray();

                AssertContrast($"{mode} link", Color(tokens["--ta-link"]), surfaces, 4.5);
                AssertContrast($"{mode} muted text", Color(tokens["--ta-muted"]), surfaces, 4.5);
                AssertContrast($"{mode} control border", Color(tokens["--ta-control-border"]), surfaces, 3);
                AssertContrast($"{mode} focus ring", Color(tokens["--ta-focus-color"]), surfaces, 3);
                AssertContrast($"{mode} text on primary", Color(tokens["--ta-on-primary"]), [Color(tokens["--ta-primary"])], 4.5);
                AssertContrast($"{mode} text on hovered primary", Color(tokens["--ta-on-primary"]), [Color(tokens["--ta-primary-hover"])], 4.5);
                AssertContrast($"{mode} text on pressed primary", Color(tokens["--ta-on-primary"]), [Color(tokens["--ta-primary-active"])], 4.5);
                AssertContrast($"{mode} text on the toolbar", Color(tokens["--ta-toolbar-fg"]), [Color(tokens["--ta-toolbar-bg"])], 4.5);
            }
        }
    }

    [Fact]
    public void FromColors_PutsDarkTextOnALightPrimary()
    {
        var gold = AdminSchemeDefinition.FromColors(Color("#eaa52c"), Color("#1e1e1e"), Color("#09080b"), Color("#eaa52c"), lightMenu: false);
        var blue = AdminSchemeDefinition.FromColors(Color("#2a81bb"), Color("#144c71"), Color("#081b26"), Color("#5aa4ef"), lightMenu: false);

        Assert.NotNull(gold.OnPrimary);
        Assert.True(gold.OnPrimary.Value.Luminance < 0.1);
        Assert.Null(blue.OnPrimary);
    }

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
        => Assert.Equal(valid, AdminSchemeStore.IsValidId(id));

    [Fact]
    public void NewIds_AreValid()
        => Assert.True(AdminSchemeStore.IsValidId(AdminSchemeStore.NewId()));

    [Fact]
    public void Stylesheet_HoldsEveryValidScheme_AndSkipsTheRest()
    {
        var css = AdminSchemeStore.BuildStylesheet(
        [
            new AdminScheme { Id = "custom-ok", Name = "Ok", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" },
            new AdminScheme { Id = "custom-bad-color", Name = "Bad", Primary = "blue", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" },
            new AdminScheme { Id = "Bad Id", Name = "Bad id", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" },
        ]);

        Assert.Contains("[data-ta-scheme=\"custom-ok\"]{", css, StringComparison.Ordinal);
        Assert.Contains("[data-bs-theme=\"dark\"][data-ta-scheme=\"custom-ok\"]{", css, StringComparison.Ordinal);
        Assert.Contains(".ta-scheme-swatch[data-scheme=\"custom-ok\"]", css, StringComparison.Ordinal);
        Assert.DoesNotContain("custom-bad-color", css, StringComparison.Ordinal);
        Assert.DoesNotContain("Bad Id", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Typography_DefaultsToLato_TheBodyFontForHeadings_AndUnitScales()
    {
        var definition = AdminSchemeStore.ToDefinition(new AdminScheme { Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" });
        var tokens = ToDictionary(AdminSchemeCssBuilder.BuildTypography(definition));

        Assert.Equal(s_typographyTokens.Order(StringComparer.Ordinal), tokens.Keys.Order(StringComparer.Ordinal));
        Assert.Equal("\"Lato\", system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", Arial, sans-serif", tokens["--ta-font-family"]);
        Assert.Equal("var(--ta-font-family)", tokens["--ta-heading-font-family"]);
        Assert.Equal("1", tokens["--ta-font-scale"]);
        Assert.Equal("1", tokens["--ta-radius-scale"]);
        Assert.Equal("1", tokens["--ta-density"]);
    }

    [Fact]
    public void Typography_WritesTheChosenFontsAndScales_InTheInvariantCulture()
    {
        var culture = CultureInfo.CurrentCulture;

        try
        {
            // A culture with a decimal comma: CSS needs a dot.
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            var definition = AdminSchemeStore.ToDefinition(new AdminScheme
            {
                Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef",
                FontFamily = AdminSchemeTypography.Inter,
                HeadingFontFamily = AdminSchemeTypography.SourceSans3,
                FontScale = 0.9375,
                RadiusScale = 0,
                Density = 1.25,
            });
            var tokens = ToDictionary(AdminSchemeCssBuilder.BuildTypography(definition));

            Assert.StartsWith("\"Inter\", system-ui,", tokens["--ta-font-family"], StringComparison.Ordinal);
            Assert.StartsWith("\"Source Sans 3\", system-ui,", tokens["--ta-heading-font-family"], StringComparison.Ordinal);
            Assert.Equal("0.9375", tokens["--ta-font-scale"]);
            Assert.Equal("0", tokens["--ta-radius-scale"]);
            Assert.Equal("1.25", tokens["--ta-density"]);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void Typography_ClampsScalesOutOfRange_AndFallsBackOnUnknownFonts()
    {
        var definition = AdminSchemeStore.ToDefinition(new AdminScheme
        {
            Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef",
            FontFamily = "comic-sans",
            HeadingFontFamily = "papyrus",
            FontScale = 3,
            RadiusScale = -1,
            Density = 0.1,
        });
        var tokens = ToDictionary(AdminSchemeCssBuilder.BuildTypography(definition));

        Assert.StartsWith("\"Lato\",", tokens["--ta-font-family"], StringComparison.Ordinal);
        Assert.Equal("var(--ta-font-family)", tokens["--ta-heading-font-family"]);
        Assert.Equal("1.125", tokens["--ta-font-scale"]);
        Assert.Equal("0", tokens["--ta-radius-scale"]);
        Assert.Equal("0.75", tokens["--ta-density"]);
    }

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
        => Assert.Equal(valid, AdminSchemeStore.HasValidTypography(new AdminScheme
        {
            FontFamily = font,
            HeadingFontFamily = heading,
            FontScale = fontScale,
            RadiusScale = radiusScale,
            Density = density,
        }));

    [Theory]
    [InlineData(1, 1, 1, true)]
    [InlineData(0.875, 0, 0.75, true)]
    [InlineData(1.125, 1.5, 1.25, true)]
    [InlineData(0.5, 1, 1, false)]
    [InlineData(1, 2, 1, false)]
    [InlineData(1, 1, 2, false)]
    public void Editor_RejectsScalesOutOfRange(double fontScale, double radiusScale, double density, bool valid)
    {
        var model = new AdminSchemeViewModel { Name = "A", FontScale = fontScale, RadiusScale = radiusScale, Density = density };

        Assert.Equal(valid, Validator.TryValidateObject(model, new ValidationContext(model), [], validateAllProperties: true));
    }

    [Fact]
    public void Editor_OffersOnlyValidSteps()
    {
        Assert.All(AdminSchemeTypography.FontScales, value => Assert.True(AdminSchemeTypography.IsValidFontScale(value)));
        Assert.All(AdminSchemeTypography.RadiusScales, value => Assert.True(AdminSchemeTypography.IsValidRadiusScale(value)));
        Assert.All(AdminSchemeTypography.Densities, value => Assert.True(AdminSchemeTypography.IsValidDensity(value)));
        Assert.Equal(["lato", "inter", "source-sans-3", "system"], AdminSchemeTypography.Fonts.Select(font => font.Key));
    }

    [Fact]
    public void Stylesheet_WritesTypographyOnce_InTheLightBlock()
    {
        var css = AdminSchemeStore.BuildStylesheet(
        [
            new AdminScheme { Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", FontScale = 1.0625 },
        ]);

        var lines = css.Split('\n');
        var light = Assert.Single(lines, line => line.StartsWith("[data-ta-scheme=\"custom-a\"]{", StringComparison.Ordinal));
        var dark = Assert.Single(lines, line => line.StartsWith("[data-bs-theme=\"dark\"][data-ta-scheme=\"custom-a\"]{", StringComparison.Ordinal));

        Assert.Contains("--ta-font-scale:1.0625;", light, StringComparison.Ordinal);
        Assert.Contains("--ta-font-family:\"Lato\"", light, StringComparison.Ordinal);
        Assert.Contains("--ta-density:1;", light, StringComparison.Ordinal);
        Assert.DoesNotContain("--ta-font", dark, StringComparison.Ordinal);
        Assert.DoesNotContain("--ta-density", dark, StringComparison.Ordinal);
        Assert.DoesNotContain("--ta-radius-scale", dark, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DeploymentAndRecipe_RoundTripEverySetting()
    {
        var source = new InMemoryDocumentManager();
        var sourceStore = new AdminSchemeStore(source, new MemoryCache(new MemoryCacheOptions()));
        var scheme = new AdminScheme
        {
            Id = "custom-round-trip", Name = "Round trip", Position = 3,
            Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", LightMenu = true,
            Hue = 200, Saturation = 0.5, OnPrimary = "#ffffff", Page = "#fafafa", Toolbar = "#123456", Tint = "#abcdef", TintDark = "#fedcba",
            FontFamily = AdminSchemeTypography.SourceSans3, HeadingFontFamily = AdminSchemeTypography.Inter,
            FontScale = 1.0625, RadiusScale = 0.5, Density = 0.75,
        };
        await sourceStore.SaveAsync(scheme);

        var result = new DeploymentPlanResult(new MemoryFileBuilder(), new RecipeDescriptor());
        await new AdminSchemesDeploymentSource(sourceStore).ProcessDeploymentStepAsync(new AdminSchemesDeploymentStep(), result);

        // Through text, as a recipe file is read.
        var step = Assert.IsType<JsonObject>(JsonNode.Parse(Assert.Single(result.Steps).ToJsonString()));
        Assert.Equal("AdminSchemes", step["name"]?.GetValue<string>());

        var target = new InMemoryDocumentManager();
        var targetStore = new AdminSchemeStore(target, new MemoryCache(new MemoryCacheOptions()));
        var context = new RecipeExecutionContext { Name = "AdminSchemes", Step = step };
        await new AdminSchemesStep(targetStore, Mock.Of<IStringLocalizer<AdminSchemesStep>>()).ExecuteAsync(context);

        Assert.Empty(context.Errors);
        var imported = await targetStore.FindAsync(scheme.Id);
        Assert.NotNull(imported);
        Assert.Equivalent(scheme, imported, strict: true);
    }

    [Fact]
    public async Task Recipe_RejectsUnknownFontsAndScalesOutOfRange()
    {
        var store = new AdminSchemeStore(new InMemoryDocumentManager(), new MemoryCache(new MemoryCacheOptions()));
        var step = JsonNode.Parse("""
            {
              "name": "AdminSchemes",
              "Schemes": [
                { "ItemId": "custom-font", "Name": "Font", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontFamily": "comic-sans" },
                { "ItemId": "custom-scale", "Name": "Scale", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontScale": 2 },
                { "ItemId": "custom-ok", "Name": "Ok", "Primary": "#2a81bb", "Deep": "#144c71", "Ink": "#081b26", "Accent": "#5aa4ef", "FontFamily": "system", "Density": 1.25 }
              ]
            }
            """).AsObject();
        var localizer = new Mock<IStringLocalizer<AdminSchemesStep>>();
        localizer
            .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(CultureInfo.InvariantCulture, name, arguments)));
        var context = new RecipeExecutionContext { Name = "AdminSchemes", Step = step };

        await new AdminSchemesStep(store, localizer.Object).ExecuteAsync(context);

        Assert.Equal(2, context.Errors.Count);
        Assert.Null(await store.FindAsync("custom-font"));
        Assert.Null(await store.FindAsync("custom-scale"));
        Assert.Equal(AdminSchemeTypography.SystemFont, (await store.FindAsync("custom-ok"))?.FontFamily);
    }

    private static void AssertSameTokens(string scheme, Dictionary<string, string> sass, IReadOnlyList<KeyValuePair<string, string>> csharp)
    {
        var generated = ToDictionary(csharp);

        // The typography tokens are not part of the color derivation.
        foreach (var name in s_typographyTokens)
        {
            sass.Remove(name);
        }

        Assert.Equal(sass.Keys.Order(StringComparer.Ordinal), generated.Keys.Order(StringComparer.Ordinal));

        foreach (var (name, expected) in sass)
        {
            var expectedNumbers = Numbers(expected);
            var actualNumbers = Numbers(generated[name]);

            Assert.True(expectedNumbers.Count == actualNumbers.Count, $"{scheme} {name}: Sass '{expected}', C# '{generated[name]}'.");

            for (var i = 0; i < expectedNumbers.Count; i++)
            {
                // A channel may round one step apart; Sass keeps fractions, C# writes rounded hex.
                Assert.True(Math.Abs(expectedNumbers[i] - actualNumbers[i]) <= 1.01, $"{scheme} {name}: Sass '{expected}', C# '{generated[name]}'.");
            }
        }
    }

    private static void AssertContrast(string what, AdminColor color, IEnumerable<AdminColor> backgrounds, double minimum)
    {
        var lowest = color.MinContrast(backgrounds);

        Assert.True(lowest >= minimum, $"{what}: {lowest:0.00} is under {minimum}.");
    }

    // The numbers of a token, colors as their 0-255 channels, so '#2677ae', 'rgb(14.9%, 46.7%, 68.2%)' and
    // '%232677ae' (a color in an SVG data URI) compare alike.
    private static List<double> Numbers(string value)
    {
        value = value.Replace("%23", "#", StringComparison.Ordinal);
        var numbers = new List<double>();
        var position = 0;

        foreach (Match match in s_colorRegex.Matches(value))
        {
            AddPlainNumbers(numbers, value[position..match.Index]);
            var color = ParseCssColor(match.Value, out var alpha);
            numbers.AddRange([color.R, color.G, color.B]);

            if (alpha is not null)
            {
                numbers.Add(alpha.Value * 100);
            }

            position = match.Index + match.Length;
        }

        AddPlainNumbers(numbers, value[position..]);

        return numbers;
    }

    private static void AddPlainNumbers(List<double> numbers, string text)
    {
        // Keep the triplets of the '-rgb' tokens and the stops of gradients; skip the numbers of the SVG markup.
        if (text.Contains("svg", StringComparison.Ordinal))
        {
            return;
        }

        foreach (Match match in s_numberRegex.Matches(text))
        {
            numbers.Add(double.Parse(match.Value, CultureInfo.InvariantCulture));
        }
    }

    private static AdminColor ParseCssColor(string value, out double? alpha)
    {
        alpha = null;

        if (value.StartsWith('#'))
        {
            return Color(value.Length == 9 ? value[..7] : value);
        }

        var parts = value[(value.IndexOf('(') + 1)..^1].Split(',', StringSplitOptions.TrimEntries);
        var channels = parts.Take(3)
            .Select(part => part.EndsWith('%')
                ? double.Parse(part[..^1], CultureInfo.InvariantCulture) * 2.55
                : double.Parse(part, CultureInfo.InvariantCulture))
            .ToArray();

        if (parts.Length > 3)
        {
            alpha = double.Parse(parts[3], CultureInfo.InvariantCulture);
        }

        return new AdminColor(channels[0], channels[1], channels[2]);
    }

    private static Dictionary<string, string> ReadBlock(string selector, string mustContain)
    {
        var path = Path.Combine(FindRepositoryRoot(), "src", "OrchardCore.Themes", "TheAdmin", "wwwroot", "css", "TheAdmin.css");
        var css = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
        var start = 0;

        while ((start = css.IndexOf(selector + " {", start, StringComparison.Ordinal)) >= 0)
        {
            var open = css.IndexOf('{', start);
            var close = css.IndexOf('}', open);
            var body = css[(open + 1)..close];

            if (body.Contains(mustContain, StringComparison.Ordinal) && (start == 0 || css[start - 1] == '\n'))
            {
                return body.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(declaration => declaration.StartsWith("--", StringComparison.Ordinal))
                    .Select(declaration => declaration.Split(':', 2))
                    .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim(), StringComparer.Ordinal);
            }

            start = close;
        }

        throw new InvalidOperationException($"No '{selector}' block with '{mustContain}' in {path}.");
    }

    private static Dictionary<string, string> ToDictionary(IReadOnlyList<KeyValuePair<string, string>> tokens)
        => tokens.ToDictionary(token => token.Key, token => token.Value, StringComparer.Ordinal);

    private static Dictionary<string, string> Merge(Dictionary<string, string> light, Dictionary<string, string> dark)
    {
        var merged = new Dictionary<string, string>(light, StringComparer.Ordinal);

        foreach (var (name, value) in dark)
        {
            merged[name] = value;
        }

        return merged;
    }

    private static AdminColor Color(string value) => AdminColor.Parse(value);

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OrchardCore.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Unable to locate the repository root from the test assembly location.");
    }

    // The schemes document, kept in memory; each update gives it a new identifier, as the document store does.
    private sealed class InMemoryDocumentManager : IDocumentManager<AdminSchemesDocument>
    {
        private AdminSchemesDocument _document = new();

        public Task<AdminSchemesDocument> GetOrCreateMutableAsync(Func<Task<AdminSchemesDocument>> factoryAsync = null)
            => Task.FromResult(_document);

        public Task<AdminSchemesDocument> GetOrCreateImmutableAsync(Func<Task<AdminSchemesDocument>> factoryAsync = null)
            => Task.FromResult(_document);

        public Task UpdateAsync(AdminSchemesDocument document, Func<AdminSchemesDocument, Task> afterUpdateAsync = null)
        {
            document.Identifier = IdGenerator.GenerateId();
            _document = document;

            return Task.CompletedTask;
        }
    }
}
