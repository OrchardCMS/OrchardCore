using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

#nullable enable

namespace OrchardCore.DisplayManagement.SourceGenerators;

/// <summary>
/// Generates the <c>PlacementCatalog</c> assembly attributes that list, for each type, the constant placement locations
/// with a tab or a card and the tab and card names it sets, so that data localization can list them for translation
/// without running the display drivers.
/// </summary>
/// <remarks>
/// The values are those of <c>ShapeResult.Location()</c>, of <c>PlacementLocationBuilder.Tab()</c> and <c>Card()</c>,
/// and of the <c>[PlacementLocation]</c> attributes of the type. The locations are kept as they are written and are
/// parsed at runtime by the placement parser. <see cref="PlacementLocationAnalyzer"/> reports the calls whose values
/// can't be read.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class PlacementCatalogGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var calls = context.SyntaxProvider
            .CreateSyntaxProvider(IsCandidateCall, GetCallEntries)
            .SelectMany(static (entries, _) => entries);

        var declarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                PlacementLocations.LocationAttributeMetadataName,
                static (node, _) => node is ClassDeclarationSyntax,
                GetDeclaredEntries)
            .SelectMany(static (entries, _) => entries);

        var hasCatalogAttribute = context.CompilationProvider
            .Select(static (compilation, _) => compilation.GetTypeByMetadataName(PlacementLocations.CatalogAttributeMetadataName) is not null);

        var entries = calls.Collect()
            .Combine(declarations.Collect())
            .Combine(hasCatalogAttribute);

        context.RegisterSourceOutput(entries, static (context, source) =>
        {
            var ((callEntries, declaredEntries), hasAttribute) = source;
            if (!hasAttribute || callEntries.Length + declaredEntries.Length == 0)
            {
                return;
            }

            context.AddSource("PlacementCatalog.g.cs", Generate(callEntries.Concat(declaredEntries)));
        });
    }

    private static bool IsCandidateCall(SyntaxNode node, CancellationToken cancellationToken)
    {
        var name = node switch
        {
            InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } => memberAccess.Name.Identifier.ValueText,
            InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax memberBinding } => memberBinding.Name.Identifier.ValueText,
            _ => null,
        };

        return name is "Location" or "Tab" or "Card";
    }

    private static ImmutableArray<PlacementEntry> GetCallEntries(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.SemanticModel.GetOperation(context.Node, cancellationToken) is not IInvocationOperation invocation)
        {
            return [];
        }

        var placementLocations = PlacementLocations.Create(context.SemanticModel.Compilation);
        if (placementLocations is null ||
            !placementLocations.TryGetArgument(invocation, out var argument, out var kind) ||
            GetDeclaringType(context.SemanticModel.GetEnclosingSymbol(context.Node.SpanStart, cancellationToken)) is not { } declaringType)
        {
            return [];
        }

        var values = new List<string?>();
        if (!PlacementLocations.TryGetConstantValues(argument!.Value, kind, values))
        {
            return [];
        }

        return CreateEntries(declaringType, kind, values);
    }

    private static ImmutableArray<PlacementEntry> GetDeclaredEntries(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        if (GetDeclaringType(context.TargetSymbol) is not { } declaringType)
        {
            return [];
        }

        var values = context.Attributes
            .Where(attribute => attribute.ConstructorArguments.Length == 1)
            .Select(attribute => attribute.ConstructorArguments[0].Value as string)
            .ToList();

        return CreateEntries(declaringType, PlacementValueKind.Location, values);
    }

    private static ImmutableArray<PlacementEntry> CreateEntries(string declaringType, PlacementValueKind kind, List<string?> values)
    {
        var entries = ImmutableArray.CreateBuilder<PlacementEntry>();

        foreach (var value in values)
        {
            if (string.IsNullOrEmpty(value) || kind == PlacementValueKind.Location && !PlacementLocations.HasTabOrCard(value!))
            {
                continue;
            }

            entries.Add(new PlacementEntry(declaringType, kind, value!));
        }

        return entries.ToImmutable();
    }

    /// <summary>
    /// Gets the type that declares a symbol as the runtime resolves it with <c>Assembly.GetType()</c>: the metadata name
    /// of its generic type definition, with <c>+</c> between nested types. A name is valid whatever the accessibility of
    /// the type and of its containing types, and needs no escaping.
    /// </summary>
    private static string? GetDeclaringType(ISymbol? symbol)
    {
        var type = symbol as INamedTypeSymbol ?? symbol?.ContainingType;

        return type is null ? null : GetMetadataName(type.OriginalDefinition);
    }

    private static string GetMetadataName(INamedTypeSymbol type)
    {
        if (type.ContainingType is not null)
        {
            return $"{GetMetadataName(type.ContainingType)}+{type.MetadataName}";
        }

        var namespaceName = GetNamespaceName(type.ContainingNamespace);

        return namespaceName.Length == 0 ? type.MetadataName : $"{namespaceName}.{type.MetadataName}";
    }

    private static string GetNamespaceName(INamespaceSymbol? @namespace)
        => @namespace is null || @namespace.IsGlobalNamespace
            ? string.Empty
            : GetNamespaceName(@namespace.ContainingNamespace) is { Length: > 0 } parent
                ? $"{parent}.{@namespace.MetadataName}"
                : @namespace.MetadataName;

    private static string Generate(IEnumerable<PlacementEntry> entries)
    {
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine();

        foreach (var type in entries.GroupBy(entry => entry.DeclaringType).OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            builder.Append("[assembly: global::").Append(PlacementLocations.CatalogAttributeMetadataName).Append('(').Append(SymbolDisplay.FormatLiteral(type.Key, quote: true));
            AppendValues(builder, "Locations", type, PlacementValueKind.Location);
            AppendValues(builder, "Tabs", type, PlacementValueKind.Tab);
            AppendValues(builder, "Cards", type, PlacementValueKind.Card);
            builder.AppendLine(")]");
        }

        return builder.ToString();
    }

    private static void AppendValues(StringBuilder builder, string property, IEnumerable<PlacementEntry> entries, PlacementValueKind kind)
    {
        var values = entries
            .Where(entry => entry.Kind == kind)
            .Select(entry => entry.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToList();

        if (values.Count == 0)
        {
            return;
        }

        builder.Append(", ").Append(property).Append(" = new string[] { ");
        builder.Append(string.Join(", ", values.Select(value => SymbolDisplay.FormatLiteral(value, quote: true))));
        builder.Append(" }");
    }

    private readonly struct PlacementEntry : IEquatable<PlacementEntry>
    {
        public PlacementEntry(string declaringType, PlacementValueKind kind, string value)
        {
            DeclaringType = declaringType;
            Kind = kind;
            Value = value;
        }

        public string DeclaringType { get; }

        public PlacementValueKind Kind { get; }

        public string Value { get; }

        public bool Equals(PlacementEntry other)
            => DeclaringType == other.DeclaringType && Kind == other.Kind && Value == other.Value;

        public override bool Equals(object? obj)
            => obj is PlacementEntry other && Equals(other);

        public override int GetHashCode()
            => (DeclaringType.GetHashCode() * 31 + (int)Kind) * 31 + Value.GetHashCode();
    }
}
