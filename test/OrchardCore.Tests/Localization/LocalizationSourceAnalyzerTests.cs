using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using OrchardCore.Localization;
using OrchardCore.Localization.Analyzers;

namespace OrchardCore.Tests.Localization;

#nullable enable

public class LocalizationSourceAnalyzerTests
{
    private static readonly MetadataReference[] s_metadataReferences = GetMetadataReferences();

    [Theory]
    [InlineData("LocalizationSource.Create(\"Manage settings\")")]
    [InlineData("LocalizationSource.Create(\"Manage settings\", null)")]
    [InlineData("LocalizationSource.Create(value: \"Manage settings\", type: null)")]
    [InlineData("LocalizationSource.Create(Description)")]
    [InlineData("new LocalizationSource(\"Manage settings\")")]
    [InlineData("new LocalizationSource(\"Manage settings\", null)")]
    [InlineData("new(\"Manage settings\")")]
    public async Task Analyze_ConstantWithoutContext_ReportsMissingContext(string expression)
    {
        var source = $$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Permissions
            {
                public const string Description = "Manage settings";
                public static readonly LocalizationSource Source = {{expression}};
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source));

        Assert.Equal(LocalizationSourceAnalyzer.MissingContextDiagnosticId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Contains("Manage settings", diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.Contains("Create<Permissions>", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("LocalizationSource.Create(value)")]
    [InlineData("new LocalizationSource(value)")]
    [InlineData("LocalizationSource.Create(\"\")")]
    [InlineData("LocalizationSource.Create(\"Manage settings\", type)")]
    [InlineData("LocalizationSource.Create<Permissions>(\"Manage settings\")")]
    [InlineData("LocalizationSource.Create<Permissions>(Description)")]
    [InlineData("LocalizationSource.Create<Permissions>(\"Manage \" + nameof(Permissions))")]
    [InlineData("LocalizationSource.Create(\"Manage settings\", typeof(Permissions))")]
    [InlineData("new LocalizationSource(\"Manage settings\", typeof(Permissions))")]
    [InlineData("new(\"Manage settings\", typeof(Permissions))")]
    public async Task Analyze_ValidDeclarations_ReportsNothing(string expression)
    {
        var source = $$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Permissions
            {
                public const string Description = "Manage settings";
                public LocalizationSource Get(string value, System.Type type)
                {
                    LocalizationSource source = {{expression}};
                    return source;
                }
            }
            """;

        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Fact]
    public async Task Analyze_NestedDeclaringType_AcceptsContainingTypeAsContext()
    {
        const string source = """
            using OrchardCore.Localization;
            namespace Example;
            public class Outer
            {
                public class Inner
                {
                    public static readonly LocalizationSource Outermost = LocalizationSource.Create<Outer>("Outer message");
                    public static readonly LocalizationSource Innermost = LocalizationSource.Create<Inner>("Inner message");
                }
            }
            """;

        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Fact]
    public async Task Analyze_GenericDeclaringType_AcceptsOpenGenericContext()
    {
        const string source = """
            using OrchardCore.Localization;
            namespace Example;
            public class Owner<TModel>
            {
                private static readonly LocalizationSource s_category = LocalizationSource.Create<Owner<TModel>>("Configuration");
                private static readonly LocalizationSource s_title = LocalizationSource.Create("Settings", typeof(Owner<>));
            }
            """;

        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Theory]
    [InlineData("LocalizationSource.Create<Other>(\"Manage settings\")")]
    [InlineData("LocalizationSource.Create(\"Manage settings\", typeof(Other))")]
    [InlineData("new LocalizationSource(\"Manage settings\", typeof(Other))")]
    public async Task Analyze_ContextIsAnotherType_ReportsContextMismatch(string expression)
    {
        var source = $$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Other { }
            public class Permissions
            {
                public static readonly LocalizationSource Source = {{expression}};
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source));

        Assert.Equal(LocalizationSourceAnalyzer.ContextMismatchDiagnosticId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
        Assert.Contains("'Other'", diagnostic.GetMessage(), StringComparison.Ordinal);
        Assert.Contains("'Permissions'", diagnostic.GetMessage(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Analyze_TypeParameterContext_ReportsContextMismatch()
    {
        const string source = """
            using OrchardCore.Localization;
            namespace Example;
            public class Factory
            {
                public LocalizationSource Create<T>() => LocalizationSource.Create<T>("Generic message");
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source));

        Assert.Equal(LocalizationSourceAnalyzer.ContextMismatchDiagnosticId, diagnostic.Id);
    }

    [Theory]
    [InlineData("LocalizationSource.Create<Permissions>(value)")]
    [InlineData("LocalizationSource.Create<Permissions>(\"Edit \" + value)")]
    [InlineData("LocalizationSource.Create<Permissions>($\"Edit {value}\")")]
    [InlineData("LocalizationSource.Create(value, typeof(Permissions))")]
    [InlineData("new LocalizationSource(value, typeof(Permissions))")]
    public async Task Analyze_NonConstantValueWithContext_ReportsNonConstantValue(string expression)
    {
        var source = $$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Permissions
            {
                public LocalizationSource Get(string value) => {{expression}};
            }
            """;

        var diagnostic = Assert.Single(await GetDiagnosticsAsync(source));

        Assert.Equal(LocalizationSourceAnalyzer.NonConstantValueDiagnosticId, diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Fact]
    public async Task Analyze_SkippedDeclarations_ReportsNothing()
    {
        const string source = """
            using OrchardCore.Localization;
            namespace Example;
            [SkipLocalizationExtraction]
            public class Forwarders
            {
                public static readonly LocalizationSource Source = LocalizationSource.Create("Manage settings");
                public LocalizationSource Get(string value) => LocalizationSource.Create<Forwarders>(value);
            }
            public class Permissions
            {
                [SkipLocalizationExtraction]
                public LocalizationSource Forward(string value) => LocalizationSource.Create<Permissions>(value);

                [SkipLocalizationExtraction]
                public LocalizationSource Untranslated() => LocalizationSource.Create("Manage settings");
            }
            """;

        Assert.Empty(await GetDiagnosticsAsync(source));
    }

    [Theory]
    [InlineData("LocalizationSource.Create(\"Manage settings\")")]
    [InlineData("LocalizationSource.Create(\"Manage settings\", null)")]
    [InlineData("LocalizationSource.Create(value: \"Manage settings\", type: null)")]
    [InlineData("new LocalizationSource(\"Manage settings\")")]
    [InlineData("new LocalizationSource(\"Manage settings\", null)")]
    [InlineData("new(\"Manage settings\")")]
    public async Task Fix_MissingContext_UsesDeclaringTypeAsTypeArgument(string expression)
    {
        var source = $$"""
            using OrchardCore.Localization;
            namespace Example;
            public class Permissions
            {
                public static readonly LocalizationSource Source = {{expression}};
            }
            """;

        var fixedSource = await ApplyFixAsync(source);

        Assert.Equal(source.Replace(expression, "LocalizationSource.Create<Permissions>(\"Manage settings\")", StringComparison.Ordinal), fixedSource);
    }

    [Fact]
    public async Task Fix_StaticDeclaringType_UsesTypeOf()
    {
        const string source = """
            using OrchardCore.Localization;
            namespace Example;
            public static class Permissions
            {
                public static readonly LocalizationSource Source = LocalizationSource.Create("Manage settings");
            }
            """;

        var fixedSource = await ApplyFixAsync(source);

        Assert.Contains("LocalizationSource.Create(\"Manage settings\", typeof(Permissions))", fixedSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fix_NestedAndGenericDeclaringTypes_UseInnermostType()
    {
        const string source = """
            using OrchardCore.Localization;
            namespace Example;
            public class Outer<TModel>
            {
                public class Inner
                {
                    public static readonly LocalizationSource Source = LocalizationSource.Create("Inner message");
                }

                public static readonly LocalizationSource Source = LocalizationSource.Create("Outer message");
            }
            """;

        var fixedSource = await ApplyFixAsync(source, expectedDiagnostics: 2);

        Assert.Contains("LocalizationSource.Create<Inner>(\"Inner message\")", fixedSource, StringComparison.Ordinal);
        Assert.Contains("LocalizationSource.Create<Outer<TModel>>(\"Outer message\")", fixedSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Fix_FullyQualifiedImplicitCreation_KeepsNameResolvable()
    {
        const string source = """
            namespace Example;
            public class Permissions
            {
                public static readonly OrchardCore.Localization.LocalizationSource Source = new("Manage settings");
            }
            """;

        var fixedSource = await ApplyFixAsync(source);

        Assert.Contains("OrchardCore.Localization.LocalizationSource.Create<Permissions>(\"Manage settings\")", fixedSource, StringComparison.Ordinal);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var compilation = CSharpCompilation.Create(
            "Tests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))],
            s_metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        var diagnostics = await compilation
            .WithAnalyzers([new LocalizationSourceAnalyzer()])
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start).ToImmutableArray();
    }

    private static async Task<string> ApplyFixAsync(string source, int expectedDiagnostics = 1)
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("Tests", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest))
            .WithMetadataReferences(s_metadataReferences);
        var document = project.AddDocument("Test.cs", source);
        var provider = new LocalizationSourceContextCodeFixProvider();

        for (var iteration = 0; iteration < expectedDiagnostics; iteration++)
        {
            var compilation = await document.Project.GetCompilationAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(compilation);
            Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

            var diagnostics = await compilation.WithAnalyzers([new LocalizationSourceAnalyzer()]).GetAnalyzerDiagnosticsAsync();
            Assert.Equal(expectedDiagnostics - iteration, diagnostics.Length);

            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostics[0], (action, _) => actions.Add(action), TestContext.Current.CancellationToken);
            await provider.RegisterCodeFixesAsync(context);
            var action = Assert.Single(actions);

            var operations = await action.GetOperationsAsync(TestContext.Current.CancellationToken);
            var solution = Assert.Single(operations.OfType<ApplyChangesOperation>()).ChangedSolution;
            document = solution.GetDocument(document.Id)!;
        }

        return (await document.GetTextAsync(TestContext.Current.CancellationToken)).ToString();
    }

    private static MetadataReference[] GetMetadataReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        Assert.False(string.IsNullOrEmpty(trustedPlatformAssemblies));

        var references = trustedPlatformAssemblies!
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(LocalizationSource).Assembly.Location));

        return references.ToArray();
    }
}
