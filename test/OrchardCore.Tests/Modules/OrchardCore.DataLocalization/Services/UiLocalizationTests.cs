using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.DataLocalization.Models;
using OrchardCore.Documents;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.DataLocalization.Services.Tests;

public sealed class UiLocalizationTests
{
    [Fact]
    public void ApplyTranslations_OverridesAndRemoval_RestoresUnmodifiedPoFallback()
    {
        var fallback = new CultureDictionary("fr", count => count == 1 ? 0 : 1);
        fallback.MergeTranslations([
            new CultureDictionaryRecord("Hello", "Context", ["PO"]),
            new CultureDictionaryRecord("Other", "OtherContext", ["Unchanged"]),
        ]);
        var provider = new UiTranslationProvider();
        var first = new UiTranslationsDocument();
        first.Translations["fr"] = [new UiTranslation { Context = "Context", Key = "Hello", Values = ["Database"] }];
        var key = CultureDictionaryRecord.GetKey("Hello", "Context");

        var overlaid = provider.ApplyTranslations(fallback, first);

        Assert.Equal("Database", overlaid[key]);
        Assert.Equal("Unchanged", overlaid[CultureDictionaryRecord.GetKey("Other", "OtherContext")]);
        Assert.Equal("PO", fallback[key]);
        Assert.Same(overlaid, provider.ApplyTranslations(fallback, first));
        var updated = new UiTranslationsDocument();
        updated.Translations["fr"] = [new UiTranslation { Context = "Context", Key = "Hello", Values = ["Updated"] }];
        Assert.Equal("Updated", provider.ApplyTranslations(fallback, updated)[key]);
        Assert.Same(fallback, provider.ApplyTranslations(fallback, new UiTranslationsDocument()));
    }

    [Fact]
    public void LocalizationManager_OverrideProvider_AppliesAfterStandardProviders()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var po = new Mock<ITranslationProvider>();
        po.Setup(provider => provider.LoadTranslations("fr", It.IsAny<CultureDictionary>()))
            .Callback<string, CultureDictionary>((_, dictionary) =>
                dictionary.MergeTranslations([new CultureDictionaryRecord("Hello", "Context", ["PO"])]));
        var documents = new UiTranslationsDocument();
        documents.Translations["fr"] = [new UiTranslation { Key = "Hello", Context = "Context", Values = ["Override"] }];
        var provider = new UiTranslationProvider();
        var overrides = new Mock<ITranslationOverrideProvider>();
        overrides.Setup(value => value.ApplyTranslations(It.IsAny<CultureDictionary>()))
            .Returns<CultureDictionary>(dictionary => provider.ApplyTranslations(dictionary, documents));
        var manager = new LocalizationManager([new DefaultPluralRuleProvider()], [po.Object], cache, [overrides.Object]);
        var key = CultureDictionaryRecord.GetKey("Hello", "Context");

        Assert.Equal("Override", manager.GetDictionary(CultureInfo.GetCultureInfo("fr"))[key]);
        documents = new UiTranslationsDocument();
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

        var resources = UiPortableObject.Read(reader, "Module");

        Assert.Equal(2, resources.Count);
        Assert.Equal("Views.Example", resources[0].Context);
        Assert.Equal("{0} items", resources[0].Plural);
        Assert.Equal(["", ""], resources[0].Values);
        Assert.Equal(3, resources[0].Metadata.Count);
        Assert.Equal("Module", resources[0].AssemblyName);
        Assert.Equal("Hello world", resources[1].Key);
    }

    [Fact]
    public void Discover_ModuleAssembly_FindsGeneratedCatalogIncludingRazor()
    {
        var resources = EmbeddedUiLocalizationCatalog.Discover([typeof(UiTranslationsManager).Assembly]);

        Assert.Contains(resources, resource => resource.AssemblyName == "OrchardCore.DataLocalization" &&
            resource.Key == "UI Translations" && resource.Context.Contains("Views.Admin.Index", StringComparison.Ordinal));
        Assert.Contains(resources, resource => resource.Key == "Select a PO file no larger than 2 MB." &&
            resource.Context == "OrchardCore.DataLocalization.Controllers.UiTranslationsController");
    }

    [Theory]
    [InlineData("fr", 2)]
    [InlineData("ja", 1)]
    [InlineData("ru", 3)]
    [InlineData("ar", 6)]
    public void GetPluralFormCount_ConfiguredRuntimeRules_ReturnsRequiredForms(string culture, int count)
    {
        var (manager, _) = CreateManager();
        Assert.Equal(count, manager.GetPluralFormCount(culture));
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
            new UiTranslation { Context = "Context", Key = "{0} item", Plural = "Wrong", Values = ["One", "Many"] },
        ]));
        documents.Verify(value => value.GetOrCreateMutableAsync(It.IsAny<Func<Task<UiTranslationsDocument>>>()), Times.Never);
    }

    [Theory]
    [InlineData("msgctxt \"Context\"\nmsgid \"Hello\"\nmsgstr[2] \"Invalid\"")]
    [InlineData("msgctxt \"Context\"\nmsgid \"Hello\"\nmsgstr \"Unclosed")]
    [InlineData("msgctxt \"Context\"\nmsgid \"Hello\"\nmsgstr \"One\"\nmsgstr \"Two\"")]
    public void Read_MalformedPo_Rejects(string po)
    {
        using var reader = new StringReader(po);
        Assert.Throws<FormatException>(() => UiPortableObject.Read(reader, "Module"));
    }

    [Fact]
    public async Task ImportExport_ContextPluralEscapes_RoundTripsAndRemoves()
    {
        var (manager, documents) = CreateManager();
        var mutable = new UiTranslationsDocument();
        documents.Setup(value => value.GetOrCreateMutableAsync(null)).ReturnsAsync(mutable);
        documents.Setup(value => value.GetOrCreateImmutableAsync(null)).ReturnsAsync(mutable);
        await manager.UpdateAsync("FR", [
            Singular(value: "Quote \"slash\\\nnext"),
            new UiTranslation { Context = "Context", Key = "{0} item", Plural = "{0} items", Values = ["{0} article", "{0} articles"] },
        ]);
        var exported = await manager.ExportAsync("fr");
        var (importedManager, importedDocuments) = CreateManager();
        var imported = new UiTranslationsDocument();
        importedDocuments.Setup(value => value.GetOrCreateMutableAsync(null)).ReturnsAsync(imported);
        using var reader = new StringReader(exported);
        await importedManager.ImportAsync("fr", reader);

        Assert.Equal(2, imported.Translations["fr"].Count);
        Assert.Equal("Quote \"slash\\\nnext", imported.Translations["fr"].Single(value => value.Key == "Hello").Values[0]);
        Assert.Equal(["{0} article", "{0} articles"], imported.Translations["fr"].Single(value => value.Plural != null).Values);
        await manager.UpdateAsync("fr", [new UiTranslation { Context = "Context", Key = "Hello", Values = [] }]);
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
        await Assert.ThrowsAsync<ArgumentException>(() => manager.ImportAsync("fr", reader));
        documents.Verify(value => value.GetOrCreateMutableAsync(It.IsAny<Func<Task<UiTranslationsDocument>>>()), Times.Never);
        documents.Verify(value => value.UpdateAsync(It.IsAny<UiTranslationsDocument>(), It.IsAny<Func<UiTranslationsDocument, Task>>()), Times.Never);
    }

    [Theory]
    [InlineData("{0} only", true)]
    [InlineData("{1} invalid", false)]
    [InlineData("{", false)]
    public async Task UpdateAsync_CompositeFormat_ValidatesArguments(string value, bool valid)
    {
        var (manager, documents) = CreateManager();
        documents.Setup(document => document.GetOrCreateMutableAsync(null)).ReturnsAsync(new UiTranslationsDocument());
        var translation = new UiTranslation { Context = "Context", Key = "{0} item", Plural = "{0} items", Values = [value, value] };
        if (valid)
        {
            await manager.UpdateAsync("fr", [translation]);
            documents.Verify(document => document.UpdateAsync(It.IsAny<UiTranslationsDocument>(), null), Times.Once);
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

    private static UiTranslation Singular(string context = "Context", string key = "Hello", string value = "Bonjour")
        => new() { Context = context, Key = key, Values = [value] };

    private static (UiTranslationsManager Manager, Mock<IDocumentManager<UiTranslationsDocument>> Documents) CreateManager()
    {
        var singular = new UiLocalizationResource { Context = "Context", Key = "Hello" };
        var plural = new UiLocalizationResource { Context = "Context", Key = "{0} item", Plural = "{0} items" };
        plural.Metadata.Add("#, csharp-format");
        var catalog = new Mock<IUiLocalizationCatalog>();
        catalog.Setup(value => value.GetResources()).Returns([singular, plural]);
        var localization = new Mock<ILocalizationService>();
        localization.Setup(value => value.GetSupportedCulturesAsync()).ReturnsAsync(["fr", "ja", "ar", "ru"]);
        var documents = new Mock<IDocumentManager<UiTranslationsDocument>>();
        return (new UiTranslationsManager(documents.Object, catalog.Object, localization.Object, [new DefaultPluralRuleProvider()]), documents);
    }
}
