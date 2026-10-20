using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.Localization.Analyzers;

/// <summary>
/// Reports localizer keys that are not compile-time constants, which source extraction cannot collect into
/// translation templates: the indexers of <c>IStringLocalizer</c>, <c>IHtmlLocalizer</c> and <c>IViewLocalizer</c>,
/// their <c>GetString</c> and <c>GetHtml</c> methods, and the singular, plural and plural forms of <c>Plural</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalizerKeyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The identifier of the diagnostic reported for a localizer key that is not a compile-time constant.
    /// </summary>
    public const string NonConstantKeyDiagnosticId = "OCLOC104";

    private const string StringLocalizerMetadataName = "Microsoft.Extensions.Localization.IStringLocalizer";
    private const string HtmlLocalizerMetadataName = "Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer";
    private const string ViewLocalizerMetadataName = "Microsoft.AspNetCore.Mvc.Localization.IViewLocalizer";
    private const string StringLocalizerFactoryMetadataName = "Microsoft.Extensions.Localization.IStringLocalizerFactory";
    private const string HtmlLocalizerFactoryMetadataName = "Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizerFactory";
    private const string SkipAttributeMetadataName = "OrchardCore.Localization.SkipLocalizationExtractionAttribute";
    private const string Category = "Localization";
    private const string HelpLinkUri = "https://docs.orchardcore.net/en/latest/reference/modules/Localize/#analyzer-rules";

    private static readonly DiagnosticDescriptor s_nonConstantKeyRule = new(
        NonConstantKeyDiagnosticId,
        "Localizer key is not a constant",
        "The localizer key is not a compile-time constant, so it cannot be extracted to translation templates",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Source extraction only collects compile-time constant keys. Pass a constant, or, for a key that is dynamic by design because its values are declared and extracted elsewhere, disable the rule around the call with #pragma warning disable OCLOC104. Mark a member that forwards callers' keys with [SkipLocalizationExtraction].",
        helpLinkUri: HelpLinkUri);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [s_nonConstantKeyRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(InitializeCompilation);
    }

    private static void InitializeCompilation(CompilationStartAnalysisContext context)
    {
        var localizers = new[] { StringLocalizerMetadataName, HtmlLocalizerMetadataName, ViewLocalizerMetadataName }
            .Select(context.Compilation.GetTypeByMetadataName)
            .Where(type => type is not null)
            .Select(type => type!)
            .ToImmutableArray();
        if (localizers.Length == 0)
        {
            return;
        }

        var factories = new[] { StringLocalizerFactoryMetadataName, HtmlLocalizerFactoryMetadataName }
            .Select(context.Compilation.GetTypeByMetadataName)
            .Where(type => type is not null)
            .Select(type => type!)
            .ToImmutableArray();
        var skipAttributeType = context.Compilation.GetTypeByMetadataName(SkipAttributeMetadataName);
        var scope = new AnalysisScope(localizers, factories, skipAttributeType);

        context.RegisterOperationAction(scope.AnalyzeIndexer, OperationKind.PropertyReference);
        context.RegisterOperationAction(scope.AnalyzeInvocation, OperationKind.Invocation);
    }

    private sealed class AnalysisScope
    {
        private readonly ImmutableArray<INamedTypeSymbol> _localizers;
        private readonly ImmutableArray<INamedTypeSymbol> _factories;
        private readonly INamedTypeSymbol? _skipAttributeType;

        public AnalysisScope(ImmutableArray<INamedTypeSymbol> localizers, ImmutableArray<INamedTypeSymbol> factories, INamedTypeSymbol? skipAttributeType)
        {
            _localizers = localizers;
            _factories = factories;
            _skipAttributeType = skipAttributeType;
        }

        public void AnalyzeIndexer(OperationAnalysisContext context)
        {
            var reference = (IPropertyReferenceOperation)context.Operation;
            if (!reference.Property.IsIndexer || reference.Instance is null || !IsLocalizer(reference.Property.ContainingType))
            {
                return;
            }

            var key = reference.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0);
            if (key is null || IsConstant(key.Value) || IsSkipped(context.ContainingSymbol))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(s_nonConstantKeyRule, key.Value.Syntax.GetLocation()));
        }

        public void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var method = invocation.TargetMethod;
            if (method.Name is not ("GetString" or "GetHtml" or "Plural"))
            {
                return;
            }

            // The receiver is the instance, or the first parameter of an extension method.
            var receiverType = invocation.Instance?.Type ?? method.Parameters.FirstOrDefault()?.Type;
            if (receiverType is null || !(IsLocalizer(receiverType) || method.Name == "Plural" && IsFactory(receiverType)))
            {
                return;
            }

            if (IsSkipped(context.ContainingSymbol))
            {
                return;
            }

            foreach (var argument in invocation.Arguments)
            {
                var constant = argument.Parameter?.Name switch
                {
                    "name" when method.Name != "Plural" => IsConstant(argument.Value),
                    "singular" or "plural" => IsConstant(argument.Value),
                    "pluralForms" => IsConstantArray(argument.Value),
                    _ => true,
                };

                if (!constant)
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_nonConstantKeyRule, argument.Value.Syntax.GetLocation()));
                }
            }
        }

        private bool IsLocalizer(ITypeSymbol type)
            => Implements(type, _localizers);

        private bool IsFactory(ITypeSymbol type)
            => Implements(type, _factories);

        private static bool Implements(ITypeSymbol type, ImmutableArray<INamedTypeSymbol> interfaces)
        {
            if (type is not INamedTypeSymbol named)
            {
                return false;
            }

            foreach (var candidate in interfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, candidate))
                {
                    return true;
                }

                foreach (var implemented in named.AllInterfaces)
                {
                    if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, candidate))
                    {
                        return true;
                    }
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
                foreach (var attribute in symbol.GetAttributes())
                {
                    if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _skipAttributeType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsConstant(IOperation operation)
            => Unwrap(operation).ConstantValue.HasValue;

        private static bool IsConstantArray(IOperation operation)
        {
            IEnumerable<IOperation>? elements = Unwrap(operation) switch
            {
                IArrayCreationOperation { Initializer: not null } array => array.Initializer.ElementValues,
                ICollectionExpressionOperation collection => collection.Elements,
                _ => null,
            };

            return elements is not null && elements.All(IsConstant);
        }

        private static IOperation Unwrap(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
            }

            return operation;
        }
    }
}
