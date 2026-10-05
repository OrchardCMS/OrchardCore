using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

public sealed class CSharpLocalizationExtractorTests
{
    [Fact]
    public void Extract_MarkedPartialClass_SkipsNestedCallsAndDiagnostics()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            using Skip = OrchardCore.Localization.SkipLocalizationExtractionAttribute;
            [Skip]
            public partial class Wrapper { }
            public partial class Wrapper
            {
                public string Forward(IStringLocalizer<Wrapper> words, string key) => words[key];
                public class Nested
                {
                    public string Get(IStringLocalizer<Nested> words) => words["Excluded"];
                }
            }
            public class Caller
            {
                public string Get(IStringLocalizer<Caller> words) => words["Included"];
            }
            """);
        Assert.Equal("Included", Assert.Single(catalog.Messages).Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_MarkedMethod_SkipsLambdasAndLocalFunctionsOnly()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            using OrchardCore.Localization;
            public class Owner
            {
                [SkipLocalizationExtraction]
                public string Forward(IStringLocalizer<Owner> words, string key)
                {
                    string Local() => words[key];
                    Func<string> lambda = () => words["Excluded"];
                    return Local() + lambda();
                }
                public string Get(IStringLocalizer<Owner> words, string key)
                    => words["Included"] + words[key];
            }
            """);
        Assert.Equal("Included", Assert.Single(catalog.Messages).Text);
        Assert.Equal("OCLOC001", Assert.Single(catalog.Diagnostics).Code);
    }

    [Fact]
    public void Extract_UnrelatedAttribute_DoesNotSkipExtraction()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public sealed class SkipLocalizationExtractionAttribute : Attribute { }
            [SkipLocalizationExtraction]
            public class Owner
            {
                public string Get(IStringLocalizer<Owner> words, string key)
                    => words["Included"] + words[key];
            }
            """);
        Assert.Equal("Included", Assert.Single(catalog.Messages).Text);
        Assert.Equal("OCLOC001", Assert.Single(catalog.Diagnostics).Code);
    }

    [Fact]
    public void Extract_MarkedLocalizerMethod_PreservesCallerExtraction()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            using OrchardCore.Localization;
            public class Wrapper : IStringLocalizer<Wrapper>
            {
                private readonly IStringLocalizer inner;
                public Wrapper(IStringLocalizer inner) => this.inner = inner;
                public LocalizedString this[string name] => new(name, name);
                public LocalizedString this[string name, params object[] arguments] => new(name, name);
                public System.Collections.Generic.IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
                [SkipLocalizationExtraction]
                public LocalizedString GetString(string name) => inner[name];
            }
            public class Caller
            {
                public string Get(Wrapper words) => words.GetString("Included").Value;
            }
            """);
        Assert.Equal("Included", Assert.Single(catalog.Messages).Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_ConstructorAssignedUntypedField_UsesResourceType()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Resources { public class Nested { } }
            public class Owner
            {
                private readonly IStringLocalizer arbitrary;
                public Owner(IStringLocalizer<Resources.Nested> injected) => arbitrary = injected;
                public string Get() => arbitrary["Message " + nameof(Resources), 42];
            }
            """);
        var entry = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Resources.Nested", entry.Context);
        Assert.Equal("Message Resources", entry.Text);
        Assert.Equal("Messages.cs", Assert.Single(entry.References).Path);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_FactoryContexts_MatchesRuntimeNormalization()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Resource { public class Nested { } }
            public class Owner
            {
                public string Get(IStringLocalizerFactory factory)
                {
                    var first = factory.Create(typeof(Resource.Nested));
                    var second = factory.Create(location: "Application", baseName: "Application.Areas.Module.Views.Index");
                    return first["Typed factory"] + second["View factory"];
                }
            }
            """);
        Assert.Contains(catalog.Messages, message => message.Context == "Example.Resource.Nested" && message.Text == "Typed factory");
        Assert.Contains(catalog.Messages, message => message.Context == "Module.Views.Index" && message.Text == "View factory");
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_HtmlAndViewLocalizers_PreservesHtmlAndViewContext()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.AspNetCore.Mvc.Localization;
            namespace Example;
            public class Resources { }
            public class Owner
            {
                public string Get(IHtmlLocalizer<Resources> markup, IViewLocalizer labels)
                    => markup["<b>{0}</b>", 42].Value + labels.GetString("View text").Value;
            }
            """, viewContext: "Module.Pages.Index");
        Assert.Contains(catalog.Messages, message => message.Context == "Example.Resources" && message.Text == "<b>{0}</b>");
        Assert.Contains(catalog.Messages, message => message.Context == "Module.Pages.Index" && message.Text == "View text");
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_NamedPluralArgumentsAndArrays_ExtractsTwoSourceForms()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Owner
            {
                private static readonly string[] Forms = ["A file", "Files"];
                public string Get(IStringLocalizer<Owner> words, int count)
                    => words.Plural(plural: "{0} items", singular: "One item", count: count).Value
                        + words.Plural(pluralForms: Forms, count: count).Value;
            }
            """, includePlurals: true);
        Assert.Contains(catalog.Messages, message => message.Text == "One item" && message.Plural == "{0} items");
        Assert.Contains(catalog.Messages, message => message.Text == "A file" && message.Plural == "Files");
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_StaticExtensionCall_UsesResolvedParameters()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Owner
            {
                public string Get(IStringLocalizer<Owner> words)
                    => StringLocalizerExtensions.GetString(words, name: "Static call").Value;
            }
            """);
        Assert.Equal("Static call", Assert.Single(catalog.Messages).Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_DynamicKeyAndAmbiguousContext_ReportsWithoutGuessing()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Other { }
            public class Owner
            {
                public string Get(IStringLocalizer<Owner> first, IStringLocalizer<Other> second, string key)
                {
                    IStringLocalizer words = first;
                    words = second;
                    return first[key] + words["Ambiguous"];
                }
            }
            """);
        Assert.Empty(catalog.Messages);
        Assert.Contains(catalog.Diagnostics, diagnostic => diagnostic.Code == "OCLOC001");
        Assert.Contains(catalog.Diagnostics, diagnostic => diagnostic.Code == "OCLOC002");
    }

    [Fact]
    public void Extract_UnrepresentablePluralArray_ReportsDiagnostic()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Owner
            {
                public string Get(IStringLocalizer<Owner> words)
                    => words.Plural(1, ["First", "Second", "Third"]).Value;
            }
            """, includePlurals: true);
        Assert.Empty(catalog.Messages);
        Assert.Equal("OCLOC003", Assert.Single(catalog.Diagnostics).Code);
    }

    [Fact]
    public void Extract_UnrelatedIndexer_IgnoresNamingConvention()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using System.Collections.Generic;
            public class Owner
            {
                public string Get(Dictionary<string, string> S) => S["Not a localization key"];
            }
            """);
        Assert.Empty(catalog.Messages);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_TranslatorCommentAndRepeatedKey_MergesReferences()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            public class Owner
            {
                public string Get(IStringLocalizer<Owner> words)
                {
                    // TRANSLATORS: A navigation label.
                    var first = words["Label"];
                    return first + words["Label"];
                }
            }
            """);
        var entry = Assert.Single(catalog.Messages);
        Assert.Equal(2, entry.References.Count);
        Assert.Equal("A navigation label.", Assert.Single(entry.Comments));
    }
}
