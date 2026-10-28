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

    [Theory]
    [InlineData("LocalizationSource.Create<SiteSettingsPropertyDeploymentStep<TModel>>(\"Configuration\")")]
    [InlineData("LocalizationSource.Create(\"Configuration\", typeof(SiteSettingsPropertyDeploymentStep<TModel>))")]
    [InlineData("new LocalizationSource(\"Configuration\", typeof(SiteSettingsPropertyDeploymentStep<TModel>))")]
    [InlineData("new(\"Configuration\", typeof(SiteSettingsPropertyDeploymentStep<TModel>))")]
    public void Extract_GenericSourceDeclaration_UsesClassNameWithoutArity(string expression)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using OrchardCore.Localization;
            namespace OrchardCore.Settings.Deployment;
            public class SiteSettingsPropertyDeploymentStep<TModel> where TModel : class, new()
            {
                private static readonly LocalizationSource s_category = {{expression}};
            }
            """, includeSources: true);

        var message = Assert.Single(catalog.Messages);
        Assert.Equal("OrchardCore.Settings.Deployment.SiteSettingsPropertyDeploymentStep", message.Context);
        Assert.Equal("Configuration", message.Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("Resources<int>", "Example.Resources")]
    [InlineData("Resources<>", "Example.Resources")]
    [InlineData("Resources<int>.Nested", "Example.Resources.Nested")]
    [InlineData("Resources<int>.GenericNested<string>", "Example.Resources.GenericNested")]
    public void Extract_GenericTypeOfSource_UsesClassNamesWithoutArity(string resourceType, string expectedContext)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Resources<T>
            {
                public class Nested { }
                public class GenericNested<TNested> { }
            }
            public class Owner
            {
                public static readonly LocalizationSource Message = LocalizationSource.Create("Message", typeof({{resourceType}}));
            }
            """, includeSources: true);

        Assert.Equal(expectedContext, Assert.Single(catalog.Messages).Context);
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("IStringLocalizerFactory", "Microsoft.Extensions.Localization")]
    [InlineData("IHtmlLocalizerFactory", "Microsoft.AspNetCore.Mvc.Localization")]
    public void Extract_GenericSourceDeferredPlural_UsesResourceClassName(string factoryType, string factoryNamespace)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using OrchardCore.Localization;
            using {{factoryNamespace}};
            namespace Example;
            public class Resources<T> { }
            public class Owner<T>
            {
                public static readonly LocalizationSource Item = LocalizationSource.Create<Resources<T>>("One item");
                public string Get({{factoryType}} factory, int count)
                    => factory.Plural(count, Item, "Many items").Value;
            }
            """, includeSources: true, includePlurals: true);

        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Resources", message.Context);
        Assert.Equal("One item", message.Text);
        Assert.Equal("Many items", message.Plural);
        Assert.Equal(2, message.References.Count);
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
    public void Extract_SourceNoteAndArgumentNames_DescribeMessages()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            using Microsoft.Extensions.Localization;
            using Microsoft.AspNetCore.Mvc.Localization;
            using static OrchardCore.Localization.LocalizationMetadata;
            namespace Example;
            public class Owner
            {
                public static LocalizationSource Greeting => LocalizationSource.Create<Owner>("Hello {0}", note: "Greeting shown on the dashboard.");
                public static LocalizationSource Years => LocalizationSource.Create("in 1 year", typeof(Owner), "Relative date shown for future dates.");
                public static LocalizationSource Plain => new LocalizationSource("Plain", typeof(Owner), "A constructor note.");
                public string Get(IStringLocalizerFactory strings, IHtmlLocalizerFactory html, string user, int years)
                    => strings.Localize(Greeting, Arg("user name", user)).Value
                        + html.Plural(Arg("years", years), Years, "in {0} years").Value
                        + strings.Localize(Plain).Value;
            }
            """, includeSources: true, includePlurals: true);

        Assert.Empty(catalog.Diagnostics);
        var greeting = Assert.Single(catalog.Messages, message => message.Text == "Hello {0}");
        Assert.Equal("Example.Owner", greeting.Context);
        Assert.Equal("Greeting shown on the dashboard.", Assert.Single(greeting.Comments));
        Assert.Equal("user name", Assert.Single(greeting.FormatArguments[0]));
        Assert.Single(greeting.References);

        var years = Assert.Single(catalog.Messages, message => message.Text == "in 1 year");
        Assert.Equal("in {0} years", years.Plural);
        Assert.Equal("Relative date shown for future dates.", Assert.Single(years.Comments));
        Assert.Equal("years", Assert.Single(years.FormatArguments[0]));

        var plain = Assert.Single(catalog.Messages, message => message.Text == "Plain");
        Assert.Equal("A constructor note.", Assert.Single(plain.Comments));
        Assert.Empty(plain.FormatArguments);
    }

    [Fact]
    public void Extract_DynamicSourceNoteOrArgumentName_ReportsAndKeepsMessage()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            using Microsoft.Extensions.Localization;
            using static OrchardCore.Localization.LocalizationMetadata;
            namespace Example;
            public class Owner
            {
                public static string Note = "Dynamic";
                public static LocalizationSource Greeting => LocalizationSource.Create<Owner>("Hello {0}", Note);
                public string Get(IStringLocalizerFactory strings, string name, string user)
                    => strings.Localize(Greeting, Arg(name, user)).Value;
            }
            """, includeSources: true);

        Assert.Equal(2, catalog.Diagnostics.Count(diagnostic => diagnostic.Code == "OCLOC010"));
        var message = Assert.Single(catalog.Messages);
        Assert.Empty(message.Comments);
        Assert.Equal("user", Assert.Single(message.FormatArguments[0]));
    }

    [Fact]
    public void Extract_DeferredPluralRange_WritesRangeFlag()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using OrchardCore.Localization;
            using Microsoft.Extensions.Localization;
            using static OrchardCore.Localization.LocalizationMetadata;
            namespace Example;
            public class Owner
            {
                public static LocalizationSource Hours => LocalizationSource.Create<Owner>("1 hour", "Hours of the day.");
                public static LocalizationSource Greeting => LocalizationSource.Create<Owner>("Hello {0}");
                public string Get(IStringLocalizerFactory strings, int hours, string user)
                    => strings.Plural(Arg("hours", Range(0, 23, hours)), Hours, "{0} hours").Value
                        + strings.Localize(Greeting, Range(0, 1, hours)).Value;
            }
            """, includeSources: true, includePlurals: true);

        var diagnostic = Assert.Single(catalog.Diagnostics);
        Assert.Equal("OCLOC011", diagnostic.Code);
        var hours = Assert.Single(catalog.Messages, message => message.Text == "1 hour");
        Assert.Equal(new PluralRange(0, 23), hours.Range);
        Assert.Equal("hours", Assert.Single(hours.FormatArguments[0]));
        Assert.Null(Assert.Single(catalog.Messages, message => message.Text == "Hello {0}").Range);
        Assert.Contains("#, csharp-format, range: 0..23\nmsgctxt \"Example.Owner\"\nmsgid \"1 hour\"\n", PotWriter.Write(catalog, "Example"));
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
