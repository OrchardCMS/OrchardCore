using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

public sealed class LocalizationSourceExtractorTests
{
    [Theory]
    [InlineData("LocalizationSource.Create<Resources.Nested>(\"Message \" + nameof(Resources))")]
    [InlineData("LocalizationSource.Create(type: typeof(Resources.Nested), value: \"Message \" + nameof(Resources))")]
    [InlineData("new LocalizationSource(\"Message \" + nameof(Resources), typeof(Resources.Nested))")]
    [InlineData("new(value: \"Message \" + nameof(Resources), type: typeof(Resources.Nested))")]
    public void Extract_SourceDeclaration_UsesExplicitResourceContext(string expression)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Resources { public class Nested { } }
            public class Owner
            {
                // TRANSLATORS: A deferred message.
                public static readonly LocalizationSource Message = {{expression}};
            }
            """, includeSources: true);

        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Resources.Nested", message.Context);
        Assert.Equal("Message Resources", message.Text);
        Assert.Equal("A deferred message.", Assert.Single(message.Comments));
        Assert.Equal("Messages.cs", Assert.Single(message.References).Path);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_ContextFreeSources_DoesNotExtractOrWarn()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            using Microsoft.Extensions.Localization;
            public class Owner
            {
                public LocalizationSource First = LocalizationSource.Create("Untranslated");
                public LocalizationSource Second = new("Also untranslated", null);
                public LocalizationSource Get(string value) => LocalizationSource.Create(value);
                public string Get(IStringLocalizerFactory factory)
                    => factory.Plural(2, First, "Untranslated plural").Value;
            }
            """, includeSources: true, includePlurals: true);
        Assert.Empty(catalog.Messages);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_LocalizeForwarders_ExtractsDeclarationsWithoutWarnings()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            using Microsoft.Extensions.Localization;
            using Microsoft.AspNetCore.Mvc.Localization;
            namespace Example;
            public class Owner
            {
                public static LocalizationSource Message => LocalizationSource.Create<Owner>("Deferred {0}");
                public string Get(IStringLocalizerFactory strings, IHtmlLocalizerFactory html)
                    => strings.Localize(Message, 42).Value + html.Localize(Message, 42).Value;
            }
            """, includeSources: true);
        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Owner", message.Context);
        Assert.Equal("Deferred {0}", message.Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("IStringLocalizerFactory", "Microsoft.Extensions.Localization")]
    [InlineData("IHtmlLocalizerFactory", "Microsoft.AspNetCore.Mvc.Localization")]
    public void Extract_DeferredPlural_ResolvesAliasedSourceAndPreservesBothForms(string factoryType, string factoryNamespace)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using OrchardCore.Localization;
            using {{factoryNamespace}};
            namespace Example;
            public class Resources { }
            public class Owner
            {
                // TRANSLATORS: Number of items.
                public static LocalizationSource Item => LocalizationSource.Create<Resources>("{0} item");
                public string Get({{factoryType}} factory, int count)
                {
                    var alias = Item;
                    return factory.Plural(source: alias, plural: "{0} items", count: count).Value;
                }
            }
            """, includeSources: true, includePlurals: true);
        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Resources", message.Context);
        Assert.Equal("{0} item", message.Text);
        Assert.Equal("{0} items", message.Plural);
        Assert.Equal(2, message.References.Count);
        Assert.Equal("Number of items.", Assert.Single(message.Comments));
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_InlineDeferredPlural_DoesNotCreateSingularConflict()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            using Microsoft.Extensions.Localization;
            public class Owner
            {
                public string Get(IStringLocalizerFactory factory)
                    => factory.Plural(2, new LocalizationSource("One", typeof(Owner)), "Many").Value;
            }
            """, includeSources: true, includePlurals: true);
        Assert.Equal("Many", Assert.Single(catalog.Messages).Plural);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_DynamicKeyAndContext_ReportsDiagnostics()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            public class Owner
            {
                public LocalizationSource Key(string key) => LocalizationSource.Create<Owner>(key);
                public LocalizationSource Context(Type type) => new("Message", type);
                public LocalizationSource Generic<T>() => LocalizationSource.Create<T>("Generic");
            }
            """, includeSources: true);
        Assert.Empty(catalog.Messages);
        Assert.Equal(1, catalog.Diagnostics.Count(diagnostic => diagnostic.Code == "OCLOC001"));
        Assert.Equal(2, catalog.Diagnostics.Count(diagnostic => diagnostic.Code == "OCLOC002"));
    }

    [Fact]
    public void Extract_MarkedDeclarations_SkipsSourcesAndDiagnostics()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            [SkipLocalizationExtraction]
            public class Excluded
            {
                public LocalizationSource Message = LocalizationSource.Create<Excluded>("Excluded");
            }
            public class Owner
            {
                [SkipLocalizationExtraction]
                public LocalizationSource Forward(string key) => LocalizationSource.Create<Owner>(key);
                public LocalizationSource Message = LocalizationSource.Create<Owner>("Included");
            }
            """, includeSources: true);
        Assert.Equal("Included", Assert.Single(catalog.Messages).Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_UnrelatedSource_DoesNotExtract()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            namespace Example;
            public class LocalizationSource
            {
                public static string Create<T>(string value) => value;
            }
            public class Owner
            {
                public string Message = LocalizationSource.Create<Owner>("Not a localization source");
            }
            """, includeSources: true);
        Assert.Empty(catalog.Messages);
        Assert.Empty(catalog.Diagnostics);
    }
}
