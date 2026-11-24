using System.Text.RegularExpressions;
using OrchardCore.Documents;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Schemes;
using OrchardCore.Themes.TheAdmin.Services;
using Match = System.Text.RegularExpressions.Match;

namespace OrchardCore.Tests.Themes.TheAdmin;

/// <summary>
/// TheAdmin derives the color tokens of its fallback scheme (Gray) in Sass and those of every scheme record in C#
/// (the generated tokens of <see cref="AdminSchemeStylesheet"/>, a few renamed, and <see cref="TheAdminSchemeTokens"/>).
/// These tests keep both derivations in step, check that whatever colors an admin picks the
/// derived tokens meet WCAG 2.2 AA contrast, and cover the typography and shape tokens.
/// </summary>
public sealed class TheAdminSchemeTests
{
    // The definitions the theme's schemes were tuned with; the seeded records must reproduce them exactly.
    public static TheoryData<string> DefaultSchemes => ["gray", "blue", "ocean", "forest", "plum", "sunset", "graphite", "gold"];

    private static readonly Dictionary<string, AdminScheme> s_builtIn = new(StringComparer.Ordinal)
    {
        ["blue"] = new() { Id = "blue", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", Hue = 209, Saturation = 1 },
        ["gray"] = new()
        {
            Id = "gray", Primary = "#0d6efd", Deep = "#212529", Ink = "#212529", Accent = "#6ea8fe",
            Hue = 210, Saturation = 0.12, LightMenu = true, Page = "#fff",
            Tint = "#495057", TintDark = "#adb5bd", Toolbar = "#495057", FontFamily = AdminSchemeTypography.SystemFont,
        },
        ["ocean"] = new() { Id = "ocean", Primary = "#12868b", Deep = "#0d4a52", Ink = "#062227", Accent = "#4fd1c5", Hue = 187, Saturation = 0.9 },
        ["forest"] = new() { Id = "forest", Primary = "#2f855a", Deep = "#1c4a36", Ink = "#0b2018", Accent = "#68d391", Hue = 150, Saturation = 0.7 },
        ["plum"] = new() { Id = "plum", Primary = "#7b4fc0", Deep = "#3d2a66", Ink = "#170f2b", Accent = "#b794f4", Hue = 262, Saturation = 0.7 },
        ["sunset"] = new() { Id = "sunset", Primary = "#c2551f", Deep = "#5c2a1c", Ink = "#24100b", Accent = "#f6ad55", Hue = 18, Saturation = 0.6 },
        ["graphite"] = new() { Id = "graphite", Primary = "#4b6584", Deep = "#2d3540", Ink = "#121519", Accent = "#94a8bf", Hue = 213, Saturation = 0.2 },
        ["gold"] = new()
        {
            Id = "gold", Primary = "#eaa52c", Deep = "#1e1e1e", Ink = "#09080b", Accent = "#eaa52c",
            Hue = 240, Saturation = 0.08, OnPrimary = "#1e1e1e",
        },
    };

    // The typography and shape tokens: the Sass writes them in a block of their own, and they are not colors.
    private static readonly HashSet<string> s_typographyTokens = new(StringComparer.Ordinal)
    {
        "--ta-font-family", "--ta-heading-font-family", "--ta-font-scale", "--ta-radius-scale", "--ta-density",
    };

    private static readonly Regex s_colorRegex = new(@"#[0-9a-fA-F]{3,8}\b|rgba?\([^)]*\)", RegexOptions.Compiled);
    private static readonly Regex s_numberRegex = new(@"-?\d+(\.\d+)?", RegexOptions.Compiled);

    // The stylesheet's fallback (Gray) is derived in Sass; every scheme record in C#. Both must agree.
    [Fact]
    public void CSharpDerivation_MatchesTheSassOne_InLightMode()
        => AssertSameTokens("gray", ReadBlock(":root", mustContain: "--ta-primary:"), Light(s_builtIn["gray"]));

    [Fact]
    public void CSharpDerivation_MatchesTheSassOne_InDarkMode()
        => AssertSameTokens("gray", ReadBlock(":root[data-bs-theme=dark]", mustContain: "--ta-link:"), Dark(s_builtIn["gray"]));

    [Theory]
    [MemberData(nameof(DefaultSchemes))]
    public void SeededDefaults_ReproduceTheSchemesTheyWereTunedAs(string id)
    {
        var seed = TheAdminSchemes.CreateDefaults().Single(scheme => scheme.Id == id);

        Assert.NotNull(AdminSchemePalette.FromScheme(seed));
        Assert.Equal(Light(s_builtIn[id]), Light(seed));
        Assert.Equal(Dark(s_builtIn[id]), Dark(seed));
    }

    [Fact]
    public void SeededDefaults_HaveValidUniqueIds_AndAnOrder_StartingWithTheDefault()
    {
        var defaults = TheAdminSchemes.CreateDefaults();

        Assert.All(defaults, scheme => Assert.True(AdminSchemeStore.IsValidId(scheme.Id), scheme.Id));
        Assert.All(defaults, scheme => Assert.True(AdminSchemeStore.HasValidTypography(scheme), scheme.Id));
        Assert.Equal(defaults.Count, defaults.Select(scheme => scheme.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(defaults.OrderBy(scheme => scheme.Position).Select(scheme => scheme.Id), defaults.Select(scheme => scheme.Id));
        Assert.Equal("gray", TheAdminSchemes.DefaultId);
        Assert.Equal(["gray", "blue"], defaults.Take(2).Select(scheme => scheme.Id));
    }

    [Fact]
    public void SeededGray_KeepsTheSystemFontOfTheFormerTheAdmin()
    {
        var gray = TheAdminSchemes.CreateDefaults().Single(scheme => scheme.Id == "gray");

        Assert.StartsWith("system-ui,", Light(gray)["--ta-font-family"], StringComparison.Ordinal);
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
            var scheme = new AdminScheme { Id = "custom-a", Primary = primary, Deep = deep, Ink = ink, Accent = accent, LightMenu = lightMenu };
            var light = Light(scheme);
            var dark = Dark(scheme);

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
        var gold = AdminSchemePalette.FromColors(Color("#eaa52c"), Color("#1e1e1e"), Color("#09080b"), Color("#eaa52c"));
        var blue = AdminSchemePalette.FromColors(Color("#2a81bb"), Color("#144c71"), Color("#081b26"), Color("#5aa4ef"));

        Assert.True(gold.OnPrimary.Luminance < 0.1);
        Assert.False(gold.HasLightTextOnPrimary);
        Assert.Equal(AdminColor.White, blue.OnPrimary);
        Assert.True(blue.HasLightTextOnPrimary);
    }

    [Fact]
    public void Theme_NamesItsAttributePrefixAndDefault()
    {
        Assert.Equal("data-ta-scheme", TheAdminSchemes.AttributeName);
        Assert.Equal("ta", TheAdminSchemes.TokenPrefix);
        Assert.Equal("gray", TheAdminSchemes.DefaultId);
        Assert.Equal("[data-bs-theme=\"dark\"]", TheAdminSchemes.DarkModeSelector);
        Assert.Contains(TheAdminSchemes.CreateDefaults(), scheme => scheme.Id == TheAdminSchemes.DefaultId);
        Assert.Equal(TheAdminSchemes.DefaultId, TheAdminSchemes.CreateFallback().Id);
    }

    [Fact]
    public void Stylesheet_WritesNothingForASchemeWithAnInvalidColor()
        => Assert.Empty(Build([new AdminScheme { Id = "custom-a", Name = "A", Primary = "blue", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" }]));

    [Fact]
    public async Task Store_SeedsTheDefaultsOfTheTheme_AndGrayIsTheSiteDefault()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDocumentManager<AdminSchemesDocument>>(new InMemoryAdminSchemesDocumentManager());
        services.AddMemoryCache();
        services.AddScoped<AdminSchemeStore>();

        using var provider = services.BuildServiceProvider();
        var store = provider.GetRequiredService<AdminSchemeStore>();

        var schemes = await store.ListAsync();

        Assert.Equal(["gray", "blue", "ocean", "forest", "plum", "sunset", "graphite", "gold"], schemes.Select(scheme => scheme.Id));
        Assert.Equal("gray", await store.GetDefaultIdAsync());

        var (css, _) = await store.GetStylesheetAsync();

        Assert.Contains("[data-ta-scheme=\"gold\"]{", css, StringComparison.Ordinal);
        Assert.Contains("[data-bs-theme=\"dark\"][data-ta-scheme=\"blue\"]{", css, StringComparison.Ordinal);
        Assert.Contains(".ta-scheme-swatch[data-scheme=\"blue\"]>span:nth-child(4){background-color:#5aa4ef}", css, StringComparison.Ordinal);
        Assert.DoesNotContain("--bs-", css, StringComparison.Ordinal);
    }

    [Fact]
    public void Stylesheet_HoldsEveryValidScheme_AndSkipsTheRest()
    {
        var css = Build(
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
        var tokens = Typography(new AdminScheme { Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef" });

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

            var tokens = Typography(new AdminScheme
            {
                Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef",
                FontFamily = "lato",
                HeadingFontFamily = "Segoe UI",
                FontScale = 0.9375,
                RadiusScale = 0,
                Density = 1.25,
            });

            Assert.StartsWith("\"Lato\", system-ui,", tokens["--ta-font-family"], StringComparison.Ordinal);
            Assert.StartsWith("\"Segoe UI\", system-ui,", tokens["--ta-heading-font-family"], StringComparison.Ordinal);
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
    public void Typography_ClampsScalesOutOfRange_AndFallsBackOnFontsThatAreNotFontNames()
    {
        var tokens = Typography(new AdminScheme
        {
            Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef",
            FontFamily = "Comic Sans; color: red",
            HeadingFontFamily = "papyrus{",
            FontScale = 3,
            RadiusScale = -1,
            Density = 0.1,
        });

        Assert.StartsWith("\"Lato\",", tokens["--ta-font-family"], StringComparison.Ordinal);
        Assert.Equal("var(--ta-font-family)", tokens["--ta-heading-font-family"]);
        Assert.Equal("1.125", tokens["--ta-font-scale"]);
        Assert.Equal("0", tokens["--ta-radius-scale"]);
        Assert.Equal("0.75", tokens["--ta-density"]);
    }

    [Fact]
    public void Typography_WritesAnyTypedFont_WithTheFallbackStack()
    {
        var tokens = Typography(new AdminScheme
        {
            Id = "custom-a", Name = "A", Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef",
            FontFamily = "Segoe UI",
            HeadingFontFamily = "Georgia, serif",
        });

        Assert.Equal("\"Segoe UI\", system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", Arial, sans-serif", tokens["--ta-font-family"]);
        Assert.Equal("\"Georgia\", serif", tokens["--ta-heading-font-family"]);
    }

    [Fact]
    public void Theme_ShipsLato_ItsDefaultFont()
    {
        Assert.Equal(["lato"], TheAdminSchemes.Fonts.Select(font => font.Key));
        Assert.Equal("lato", TheAdminSchemes.DefaultFontFamily);
        Assert.StartsWith("\"Lato\", system-ui", TheAdminSchemes.GetFontStack(null), StringComparison.Ordinal);
        Assert.StartsWith("\"Lato\", system-ui", TheAdminSchemes.GetFontStack("Comic Sans; color: red"), StringComparison.Ordinal);
        Assert.StartsWith("system-ui", TheAdminSchemes.GetFontStack("system"), StringComparison.Ordinal);
    }

    [Fact]
    public void Stylesheet_WritesTypographyOnce_InTheLightBlock()
    {
        var css = Build(
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

    private static string Build(IEnumerable<AdminScheme> schemes)
        => AdminSchemeStylesheet.Build(schemes);

    // Every token of the light rule of a scheme, typography included.
    private static Dictionary<string, string> Light(AdminScheme scheme)
        => Tokens(scheme, AdminSchemeMode.Light);

    private static Dictionary<string, string> Dark(AdminScheme scheme)
        => Tokens(scheme, AdminSchemeMode.Dark);

    private static Dictionary<string, string> Typography(AdminScheme scheme)
        => Light(scheme).Where(token => s_typographyTokens.Contains(token.Key)).ToDictionary(StringComparer.Ordinal);

    private static Dictionary<string, string> Tokens(AdminScheme scheme, AdminSchemeMode mode)
    {
        var palette = AdminSchemePalette.FromScheme(scheme);

        Assert.NotNull(palette);

        return ToDictionary(AdminSchemeStylesheet.BuildTokens(palette, mode));
    }

    private static void AssertSameTokens(string scheme, Dictionary<string, string> sass, Dictionary<string, string> csharp)
    {
        var generated = new Dictionary<string, string>(csharp, StringComparer.Ordinal);

        // The typography tokens are not part of the color derivation. The order of the tokens does not matter.
        foreach (var name in s_typographyTokens)
        {
            sass.Remove(name);
            generated.Remove(name);
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
}
