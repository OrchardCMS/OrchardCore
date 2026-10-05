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

    [Theory]
    [InlineData("IStringLocalizer")]
    [InlineData("IHtmlLocalizer")]
    public void Extract_AbstractClassUntypedField_UsesContainingClass(string localizerType)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using Microsoft.Extensions.Localization;
            using Microsoft.AspNetCore.Mvc.Localization;
            namespace OrchardCore.Environment.Commands;
            public abstract class DefaultCommandHandler
            {
                protected readonly {{localizerType}} S;
                protected DefaultCommandHandler({{localizerType}} localizer) => S = localizer;
                public string Get() => S["Switch was not found"].Value;
            }
            """);
        var entry = Assert.Single(catalog.Messages);
        Assert.Equal("OrchardCore.Environment.Commands.DefaultCommandHandler", entry.Context);
        Assert.Equal("Switch was not found", entry.Text);
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("IStringLocalizer")]
    [InlineData("IHtmlLocalizer")]
    public void Extract_NestedClassUntypedParameter_UsesContainingClass(string localizerType)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using Microsoft.Extensions.Localization;
            using Microsoft.AspNetCore.Mvc.Localization;
            namespace Example;
            public class Owner
            {
                public class Nested
                {
                    public string Get({{localizerType}} words)
                    {
                        string Local() => words["Local function"].Value;
                        Func<string> lambda = () => words["Lambda"].Value;
                        return Local() + lambda();
                    }
                }
            }
            """);
        Assert.Equal(2, catalog.Messages.Count);
        Assert.All(catalog.Messages, message => Assert.Equal("Example.Owner.Nested", message.Context));
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("IStringLocalizer")]
    [InlineData("IHtmlLocalizer")]
    public void Extract_GenericAbstractClassWithInitializerLambdas_UsesContainingClass(string localizerType)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using System.Collections.Generic;
            using Microsoft.Extensions.Localization;
            using Microsoft.AspNetCore.Mvc.Localization;
            namespace OrchardCore.Apis.GraphQL.Queries;
            public abstract class WhereInputObjectGraphType : WhereInputObjectGraphType<object>
            {
                protected WhereInputObjectGraphType({{localizerType}} localizer) : base(localizer) { }
            }
            public abstract class WhereInputObjectGraphType<TSourceType>
            {
                protected readonly {{localizerType}} S;
                protected WhereInputObjectGraphType({{localizerType}} localizer) => S = localizer;
                public static readonly Dictionary<string, Func<{{localizerType}}, string, string>> EqualityOperators = new()
                {
                    { "", (S, description) => S["{0} is equal to", description].Value },
                    { "_not", (S, description) => S["{0} is not equal to", description].Value },
                };
                public string Get() => S["Field message"].Value;
            }
            """);
        Assert.Equal(3, catalog.Messages.Count);
        Assert.All(catalog.Messages, message => Assert.Equal("OrchardCore.Apis.GraphQL.Queries.WhereInputObjectGraphType`1", message.Context));
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("Nested", "Example.Owner`1.Nested")]
    [InlineData("Nested<TNested>", "Example.Owner`1.Nested`1")]
    public void Extract_NestedClassInGenericClass_UsesContainingClass(string declaration, string expectedContext)
    {
        var catalog = TestWorkspace.ExtractCSharp($$"""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Owner<T>
            {
                public class {{declaration}}
                {
                    public string Get(IStringLocalizer words) => words["Nested message"];
                }
            }
            """);
        Assert.Equal(expectedContext, Assert.Single(catalog.Messages).Context);
        Assert.Empty(catalog.Diagnostics);
    }

    [Fact]
    public void Extract_GenericClassWithTypedLocalizer_PreservesResourceContext()
    {
        var catalog = TestWorkspace.ExtractCSharp("""
            using Microsoft.Extensions.Localization;
            namespace Example;
            public class Resources { }
            public class Owner<T>
            {
                private readonly IStringLocalizer words;
                public Owner(IStringLocalizer<Resources> localizer) => words = localizer;
                public string Get() => words["Resource message"];
            }
            """);
        Assert.Equal("Example.Resources", Assert.Single(catalog.Messages).Context);
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
    public void Extract_DynamicKeyAndAmbiguousContext_ReportsKeyAndUsesContainingClass()
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
        var entry = Assert.Single(catalog.Messages);
        Assert.Equal("Example.Owner", entry.Context);
        Assert.Equal("Ambiguous", entry.Text);
        Assert.Equal("OCLOC001", Assert.Single(catalog.Diagnostics).Code);
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
