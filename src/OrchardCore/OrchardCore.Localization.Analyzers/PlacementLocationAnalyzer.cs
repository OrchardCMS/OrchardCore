using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.Localization.Analyzers;

/// <summary>
/// Reports the placement locations, tab names and card names that <see cref="PlacementCatalogGenerator"/> can't read,
/// because they aren't compile-time constants, so that the tabs and cards they set can't be listed for translation.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PlacementLocationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The identifier of the diagnostic reported for a placement location or name that is not a compile-time constant.
    /// </summary>
    public const string NonConstantPlacementDiagnosticId = "OCLOC105";

    private const string SkipAttributeMetadataName = "OrchardCore.Localization.SkipLocalizationExtractionAttribute";
    private const string Category = "Localization";
    private const string HelpLinkUri = "https://docs.orchardcore.net/en/latest/reference/modules/Localize/#analyzer-rules";

    private static readonly DiagnosticDescriptor s_nonConstantPlacementRule = new(
        NonConstantPlacementDiagnosticId,
        "Placement location is not a constant",
        "The placement location or name is not a compile-time constant, so its tab and card names cannot be listed for translation",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The tab and card names of placement locations are listed for translation from the constants read at build time. Pass a constant, or declare the possible locations on the type with [PlacementLocation], referencing the same constants. For names that come from data, list them with an ITabAndCardNameContributor and disable the rule around the call with #pragma warning disable OCLOC105.",
        helpLinkUri: HelpLinkUri);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [s_nonConstantPlacementRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(InitializeCompilation);
    }

    private static void InitializeCompilation(CompilationStartAnalysisContext context)
    {
        var placementLocations = PlacementLocations.Create(context.Compilation);
        if (placementLocations is null)
        {
            return;
        }

        var scope = new AnalysisScope(
            placementLocations,
            context.Compilation.GetTypeByMetadataName(PlacementLocations.LocationAttributeMetadataName),
            context.Compilation.GetTypeByMetadataName(SkipAttributeMetadataName));

        context.RegisterOperationAction(scope.AnalyzeInvocation, OperationKind.Invocation);
    }

    private sealed class AnalysisScope
    {
        private readonly PlacementLocations _placementLocations;
        private readonly INamedTypeSymbol? _locationAttributeType;
        private readonly INamedTypeSymbol? _skipAttributeType;

        public AnalysisScope(PlacementLocations placementLocations, INamedTypeSymbol? locationAttributeType, INamedTypeSymbol? skipAttributeType)
        {
            _placementLocations = placementLocations;
            _locationAttributeType = locationAttributeType;
            _skipAttributeType = skipAttributeType;
        }

        public void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            if (!_placementLocations.TryGetArgument(invocation, out var argument, out _) ||
                PlacementLocations.TryGetConstantValues(argument!.Value, []) ||
                IsSkipped(context.ContainingSymbol) ||
                IsDeclared(context.ContainingSymbol.ContainingType ?? context.ContainingSymbol as INamedTypeSymbol))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(s_nonConstantPlacementRule, argument.Value.Syntax.GetLocation()));
        }

        /// <summary>
        /// Gets whether the type or one of its base types declares its locations with <c>[PlacementLocation]</c>.
        /// </summary>
        private bool IsDeclared(INamedTypeSymbol? type)
        {
            if (_locationAttributeType is null)
            {
                return false;
            }

            for (; type is not null; type = type.BaseType)
            {
                if (HasAttribute(type, _locationAttributeType))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSkipped(ISymbol? symbol)
        {
            if (_skipAttributeType is null)
            {
                return false;
            }

            for (; symbol is not null; symbol = symbol.ContainingSymbol)
            {
                if (HasAttribute(symbol, _skipAttributeType))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attributeType)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
