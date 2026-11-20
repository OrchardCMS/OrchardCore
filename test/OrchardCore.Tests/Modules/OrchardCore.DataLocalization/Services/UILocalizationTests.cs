using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Documents;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.DataLocalization.Services.Tests;

public sealed class UILocalizationTests
{
    [Fact]
    public void ApplyTranslations_OverridesAndRemoval_RestoresUnmodifiedPoFallback()
    {
        var fallback = new CultureDictionary("fr", count => count == 1 ? 0 : 1);
        fallback.MergeTranslations([
            new CultureDictionaryRecord("Hello", "Context", ["PO"]),
            new CultureDictionaryRecord("Other", "OtherContext", ["Unchanged"]),
        ]);
        var provider = new UITranslationProvider();
        var first = new UITranslationsDocument();
        first.Translations["fr"] = [new UITranslation { Context = "Context", Key = "Hello", Values = ["Database"] }];
        var key = CultureDictionaryRecord.GetKey("Hello", "Context");

        var overlaid = provider.ApplyTranslations(fallback, first);

        Assert.Equal("Database", overlaid[key]);
        Assert.Equal("Unchanged", overlaid[CultureDictionaryRecord.GetKey("Other", "OtherContext")]);
        Assert.Equal("PO", fallback[key]);
        Assert.Same(overlaid, provider.ApplyTranslations(fallback, first));
        var updated = new UITranslationsDocument();
        updated.Translations["fr"] = [new UITranslation { Context = "Context", Key = "Hello", Values = ["Updated"] }];
        Assert.Equal("Updated", provider.ApplyTranslations(fallback, updated)[key]);
        Assert.Same(fallback, provider.ApplyTranslations(fallback, new UITranslationsDocument()));
    }

    [Fact]
    public void ApplyTranslations_PoPluralFormsRule_KeptForPoEntriesAndCultureRuleForOverrides()
    {
        PluralizationRuleDelegate poRule = count => count == 0 ? 0 : count == 1 ? 1 : 2;
        var fallback = new CultureDictionary("fr", count => count > 1 ? 1 : 0);
        fallback.MergeTranslations([
            new CultureDictionaryRecord("Item", "Context", ["None", "One", "Many"], poRule),
            new CultureDictionaryRecord("Other", "Context", ["None", "One", "Many"], poRule),
        ]);
        var document = new UITranslationsDocument();
        document.Translations["fr"] = [new UITranslation { Context = "Context", Key = "Item", Values = ["Single", "Plural"] }];

        var overlaid = new UITranslationProvider().ApplyTranslations(fallback, document);

        Assert.Equal("None", overlaid[CultureDictionaryRecord.GetKey("Other", "Context"), 0]);
        Assert.Equal("Single", overlaid[CultureDictionaryRecord.GetKey("Item", "Context"), 0]);
        Assert.Equal("Plural", overlaid[CultureDictionaryRecord.GetKey("Item", "Context"), 2]);
    }

    [Fact]
    public void LocalizationManager_OverrideProvider_AppliesAfterStandardProviders()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var po = new Mock<ITranslationProvider>();
        po.Setup(provider => provider.LoadTranslations("fr", It.IsAny<CultureDictionary>()))
            .Callback<string, CultureDictionary>((_, dictionary) =>
                dictionary.MergeTranslations([new CultureDictionaryRecord("Hello", "Context", ["PO"])]));
        var documents = new UITranslationsDocument();
        documents.Translations["fr"] = [new UITranslation { Key = "Hello", Context = "Context", Values = ["Override"] }];
        var provider = new UITranslationProvider();
        var overrides = new Mock<ITranslationOverrideProvider>();
        overrides.Setup(value => value.ApplyTranslations(It.IsAny<CultureDictionary>()))
            .Returns<CultureDictionary>(dictionary => provider.ApplyTranslations(dictionary, documents));
        var manager = new LocalizationManager([new DefaultPluralRuleProvider()], [po.Object], cache, [overrides.Object]);
        var key = CultureDictionaryRecord.GetKey("Hello", "Context");

        Assert.Equal("Override", manager.GetDictionary(CultureInfo.GetCultureInfo("fr"))[key]);
        documents = new UITranslationsDocument();
        Assert.Equal("PO", manager.GetDictionary(CultureInfo.GetCultureInfo("fr"))[key]);
        po.Verify(value => value.LoadTranslations("fr", It.IsAny<CultureDictionary>()), Times.Once);
    }

    [Fact]
    public void Localizers_ContextsParentFallbackAndHtml_PreservePipelineAndEncodeOverrides()
    {
        var dictionaries = new Dictionary<string, CultureDictionary>
        {
            ["fr-FR"] = new("fr-FR", count => count > 1 ? 1 : 0),
            ["fr"] = new("fr", count => count > 1 ? 1 : 0),
        };
        dictionaries["fr"].MergeTranslations([
            new CultureDictionaryRecord("Hello", "Context", ["<script>{0}</script>"]),
            new CultureDictionaryRecord("Hello", "Other", ["Other context"]),
            new CultureDictionaryRecord("PO HTML", "Context", ["<strong>Trusted PO</strong>"]),
            new CultureDictionaryRecord("{0} item", "Context", ["<b>{0} article</b>", "<b>{0} articles</b>"]),
        ]);
        dictionaries["fr"].PlainTextTranslations.UnionWith([
            CultureDictionaryRecord.GetKey("Hello", "Context"),
            CultureDictionaryRecord.GetKey("{0} item", "Context"),
        ]);
        var manager = Mock.Of<ILocalizationManager>(value => value.GetDictionary(It.IsAny<CultureInfo>()) == null);
        Mock.Get(manager).Setup(value => value.GetDictionary(It.IsAny<CultureInfo>())).Returns<CultureInfo>(culture => dictionaries[culture.Name]);
        var localizer = new PortableObjectStringLocalizer("Context", manager, true, NullLogger.Instance);
        var other = new PortableObjectStringLocalizer("Other", manager, true, NullLogger.Instance);
        var html = new PortableObjectHtmlLocalizer(localizer);
        using var scope = CultureScope.Create("fr-FR");

        Assert.Equal("<script>x</script>", localizer["Hello", "x"].Value);
        Assert.Equal("Other context", other["Hello"].Value);
        Assert.Equal("Missing", localizer["Missing"].Value);
        using var writer = new StringWriter();
        html["Hello", "<img>"].WriteTo(writer, HtmlEncoder.Default);
        Assert.Equal("&lt;script&gt;&lt;img&gt;&lt;/script&gt;", writer.ToString());
        writer.GetStringBuilder().Clear();
        html["PO HTML"].WriteTo(writer, HtmlEncoder.Default);
        Assert.Equal("<strong>Trusted PO</strong>", writer.ToString());
        writer.GetStringBuilder().Clear();
        html.Plural(2, "{0} item", "{0} items").WriteTo(writer, HtmlEncoder.Default);
        Assert.Equal("&lt;b&gt;2 articles&lt;/b&gt;", writer.ToString());
        dictionaries["fr-FR"].MergeTranslations([new CultureDictionaryRecord("Hello", "Context", ["Exact PO"])]);
        Assert.Equal("Exact PO", localizer["Hello"].Value);
    }

    [Fact]
    public void Read_UntranslatedCatalog_PreservesContextsPluralsAndMetadata()
    {
        using var reader = new StringReader("""
            msgid ""
            msgstr ""
            "Content-Type: text/plain; charset=UTF-8\n"

            #. Translator comment
            #. Format arguments: {0}: count
            #: Views/Example.cshtml:12
            #, csharp-format
            msgctxt "Views.Example"
            msgid "{0} item"
            msgid_plural "{0} items"
            msgstr[0] ""
            msgstr[1] ""

            msgctxt "Other"
            msgid "Hello "
            "world"
            msgstr ""
            """);

        var resources = PortableObjectFormat.ReadResources(reader, "Module");

        Assert.Equal(2, resources.Count);
        Assert.Equal("Views.Example", resources[0].Context);
        Assert.Equal("{0} items", resources[0].Plural);
        Assert.Equal(["", ""], resources[0].Values);
        Assert.Contains("#. Format arguments: {0}: count", resources[0].Metadata);
        Assert.Equal(4, resources[0].Metadata.Count);
        Assert.Equal("Module", resources[0].AssemblyName);
        Assert.Equal("Hello world", resources[1].Key);
    }

    [Fact]
    public void Discover_ModuleAssembly_FindsGeneratedCatalogIncludingRazor()
    {
        var resources = UILocalizationCatalog.Discover([typeof(UITranslationsManager).Assembly]);

        Assert.Contains(resources, resource => resource.AssemblyName == "OrchardCore.DataLocalization" &&
            resource.Key == "UI Translations" && resource.Context.Contains("Views.Admin.Index", StringComparison.Ordinal));
        Assert.Contains(resources, resource => resource.Key == "Select a PO file no larger than 2 MB." &&
            resource.Context == "OrchardCore.DataLocalization.Controllers.UITranslationsController");
    }


    [Theory]
    [InlineData("fr", new[] { 1, 2, 1_000_000 })]
    [InlineData("es", new[] { 1, 2, 1_000_000 })]
    [InlineData("ja", new[] { 1 })]
    [InlineData("ru", new[] { 1, 2, 0 })]
    [InlineData("ar", new[] { 0, 1, 2, 3, 11, 100 })]
    public void GetPluralFormExamples_ConfiguredRuntimeRules_ReturnsRepresentativeCounts(string culture, int[] expected)
    {
        var (manager, _) = CreateManager();
        Assert.Equal(expected, manager.GetPluralFormExamples(culture));
    }

    [Fact]
    public async Task GetPluralFormExamplesAndImport_OutdatedPoPluralFormsHeader_UseCultureRule()
    {
        // Crowdin PO files use the older two-form French rule, Orchard's rule adds 'many' for millions.
        PluralizationRuleDelegate poRule = count => count > 1 ? 1 : 0;
        var po = new Mock<ITranslationProvider>();
        po.Setup(provider => provider.LoadTranslations("fr", It.IsAny<CultureDictionary>()))
            .Callback<string, CultureDictionary>((_, dictionary) => dictionary.MergeTranslations([
                new CultureDictionaryRecord("{0} item", "Context", ["One", "Many"], poRule),
            ]));
        var (manager, documents) = CreateManager(po.Object);
        documents.Setup(value => value.GetOrCreateMutableAsync(null)).ReturnsAsync(new UITranslationsDocument());
        const string header = """
            msgid ""
            msgstr ""
            "Plural-Forms: nplurals=2; plural=(n > 1);\n"

            msgctxt "Context"
            msgid "{0} item"
            msgid_plural "{0} items"

            """;

        Assert.Equal([1, 2, 1_000_000], manager.GetPluralFormExamples("fr"));
        using var twoForms = new StringReader(header + "msgstr[0] \"One\"\nmsgstr[1] \"Other\"");
        await Assert.ThrowsAsync<ArgumentException>(() => manager.ImportAsync("fr", "po", twoForms));
        using var threeForms = new StringReader(header + "msgstr[0] \"One\"\nmsgstr[1] \"Other\"\nmsgstr[2] \"Many\"");
        await manager.ImportAsync("fr", "po", threeForms);
        documents.Verify(value => value.UpdateAsync(It.IsAny<UITranslationsDocument>(), null), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UnknownContextCultureOrPlural_RejectsBeforeLoadingMutableDocument()
    {
        var (manager, documents) = CreateManager();
        await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateAsync("de", [Singular()]));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateAsync("fr", [Singular(context: "Wrong")]));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateAsync("fr", [Singular(key: "Unknown")]));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateAsync("fr", [Singular(), Singular()]));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateAsync("fr", [
            new UITranslation { Context = "Context", Key = "{0} item", Plural = "Wrong", Values = ["One", "Many"] },
        ]));
        documents.Verify(value => value.GetOrCreateMutableAsync(It.IsAny<Func<Task<UITranslationsDocument>>>()), Times.Never);
    }

    [Theory]
    [InlineData("msgctxt \"Context\"\nmsgid \"Hello\"\nmsgstr[2] \"Invalid\"")]
    [InlineData("msgctxt \"Context\"\nmsgid \"Hello\"\nmsgstr \"Unclosed")]
    [InlineData("msgctxt \"Context\"\nmsgid \"Hello\"\nmsgstr \"One\"\nmsgstr \"Two\"")]
    public void Read_MalformedPo_Rejects(string po)
    {
        using var reader = new StringReader(po);
        Assert.Throws<FormatException>(() => PortableObjectFormat.ReadResources(reader, "Module"));
    }

    [Fact]
    public async Task ImportExport_ContextPluralEscapes_RoundTripsAndRemoves()
    {
        var (manager, documents) = CreateManager();
        var mutable = new UITranslationsDocument();
        documents.Setup(value => value.GetOrCreateMutableAsync(null)).ReturnsAsync(mutable);
        documents.Setup(value => value.GetOrCreateImmutableAsync(null)).ReturnsAsync(mutable);
        await manager.UpdateAsync("FR", [
            Singular(value: "Quote \"slash\\\nnext"),
            new UITranslation { Context = "Context", Key = "{0} item", Plural = "{0} items", Values = ["{0} article", "{0} articles", "{0} d'articles"] },
        ]);
        using var exported = new StringWriter();
        await manager.ExportAsync("fr", "po", exported);
        var (importedManager, importedDocuments) = CreateManager();
        var imported = new UITranslationsDocument();
        importedDocuments.Setup(value => value.GetOrCreateMutableAsync(null)).ReturnsAsync(imported);
        using var reader = new StringReader(exported.ToString());
        await importedManager.ImportAsync("fr", "po", reader);

        Assert.Equal(2, imported.Translations["fr"].Count);
        Assert.Equal("Quote \"slash\\\nnext", imported.Translations["fr"].Single(value => value.Key == "Hello").Values[0]);
        Assert.Equal(["{0} article", "{0} articles", "{0} d'articles"], imported.Translations["fr"].Single(value => value.Plural != null).Values);
        await manager.UpdateAsync("fr", [new UITranslation { Context = "Context", Key = "Hello", Values = [] }]);
        Assert.Single(mutable.Translations["fr"]);
    }

    [Fact]
    public async Task ImportAsync_ValidFollowedByInvalidEntry_IsAtomic()
    {
        var (manager, documents) = CreateManager();
        using var reader = new StringReader("""
            msgctxt "Context"
            msgid "Hello"
            msgstr "Valid"

            msgctxt "Context"
            msgid "Unknown"
            msgstr "Invalid"
            """);
        await Assert.ThrowsAsync<ArgumentException>(() => manager.ImportAsync("fr", "po", reader));
        documents.Verify(value => value.GetOrCreateMutableAsync(It.IsAny<Func<Task<UITranslationsDocument>>>()), Times.Never);
        documents.Verify(value => value.UpdateAsync(It.IsAny<UITranslationsDocument>(), It.IsAny<Func<UITranslationsDocument, Task>>()), Times.Never);
    }

    [Theory]
    [InlineData("{0} only", true)]
    [InlineData("{1} invalid", false)]
    [InlineData("{", false)]
    public async Task UpdateAsync_CompositeFormat_ValidatesArguments(string value, bool valid)
    {
        var (manager, documents) = CreateManager();
        documents.Setup(document => document.GetOrCreateMutableAsync(null)).ReturnsAsync(new UITranslationsDocument());
        var translation = new UITranslation { Context = "Context", Key = "{0} item", Plural = "{0} items", Values = [value, value, value] };
        if (valid)
        {
            await manager.UpdateAsync("fr", [translation]);
            documents.Verify(document => document.UpdateAsync(It.IsAny<UITranslationsDocument>(), null), Times.Once);
        }
        else if (value == "{")
        {
            await Assert.ThrowsAsync<FormatException>(() => manager.UpdateAsync("fr", [translation]));
        }
        else
        {
            await Assert.ThrowsAsync<ArgumentException>(() => manager.UpdateAsync("fr", [translation]));
        }
    }

    private static UITranslation Singular(string context = "Context", string key = "Hello", string value = "Bonjour")
        => new() { Context = context, Key = key, Values = [value] };

    private static (UITranslationsManager Manager, Mock<IDocumentManager<UITranslationsDocument>> Documents) CreateManager(
        params ITranslationProvider[] translationProviders)
    {
        var singular = new UILocalizationResource { Context = "Context", Key = "Hello" };
        var plural = new UILocalizationResource { Context = "Context", Key = "{0} item", Plural = "{0} items" };
        plural.Metadata.Add("#, csharp-format");
        var localization = new Mock<ILocalizationService>();
        localization.Setup(value => value.GetSupportedCulturesAsync()).ReturnsAsync(["fr", "ja", "ar", "ru"]);
        var documents = new Mock<IDocumentManager<UITranslationsDocument>>();
        var localizationManager = new LocalizationManager([new DefaultPluralRuleProvider()], translationProviders, new MemoryCache(new MemoryCacheOptions()));
        return (new UITranslationsManager(documents.Object, new UILocalizationCatalog([singular, plural]), localization.Object, localizationManager), documents);
    }
}
