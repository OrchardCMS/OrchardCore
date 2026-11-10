using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.Localization.Analyzers;

/// <summary>
/// Reports <c>LocalizationSource</c> declarations that PO files cannot translate as written: constant values declared
/// without a localization context, values that are not compile-time constants, and contexts that differ from the
/// declaring type. The context is the <c>msgctxt</c> of the PO entry, so a source without one is neither extracted
/// to translation templates nor looked up with a context at run time.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LocalizationSourceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The identifier of the diagnostic reported for a constant value declared without a context.
    /// </summary>
    public const string MissingContextDiagnosticId = "OCLOC101";

    /// <summary>
    /// The identifier of the diagnostic reported for a context that is not the declaring type.
    /// </summary>
    public const string ContextMismatchDiagnosticId = "OCLOC102";

    /// <summary>
    /// The identifier of the diagnostic reported for a value that is not a compile-time constant.
    /// </summary>
    public const string NonConstantValueDiagnosticId = "OCLOC103";

    private const string LocalizationSourceMetadataName = "OrchardCore.Localization.LocalizationSource";
    private const string SkipAttributeMetadataName = "OrchardCore.Localization.SkipLocalizationExtractionAttribute";
    private const string Category = "Localization";
    private const string HelpLinkUri = "https://docs.orchardcore.net/en/latest/reference/modules/Localize/#analyzer-rules";
    private const int PreviewLength = 40;

    private static readonly DiagnosticDescriptor s_missingContextRule = new(
        MissingContextDiagnosticId,
        "Localization source has no context",
        "The localization source '{0}' has no context, so it is not extracted to translation templates. Declare it with LocalizationSource.Create<{1}>(...) or pass typeof({1}).",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A localization source without a context is only translated by a context-free PO entry, and source extraction skips it. Use the declaring type as the context, like IStringLocalizer<T> does, so the value is extracted under that msgctxt and translated at display time.",
        helpLinkUri: HelpLinkUri);

    private static readonly DiagnosticDescriptor s_contextMismatchRule = new(
        ContextMismatchDiagnosticId,
        "Localization source context differs from the declaring type",
        "The localization source '{0}' uses '{1}' as its context instead of the declaring type '{2}'",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "By convention, a localization source uses the type that declares it as its context, which is where translators expect to find it. Sharing another type's context is legitimate when the strings belong together; this rule only points out the difference.",
        helpLinkUri: HelpLinkUri);

    private static readonly DiagnosticDescriptor s_nonConstantValueRule = new(
        NonConstantValueDiagnosticId,
        "Localization source value is not a constant",
        "The value of the localization source is not a compile-time constant, so it cannot be extracted to translation templates",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Source extraction only collects compile-time constant values. Pass a constant, or mark a member that forwards values declared elsewhere with [SkipLocalizationExtraction].",
        helpLinkUri: HelpLinkUri);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [s_missingContextRule, s_contextMismatchRule, s_nonConstantValueRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(InitializeCompilation);
    }

    private static void InitializeCompilation(CompilationStartAnalysisContext context)
    {
        var sourceType = context.Compilation.GetTypeByMetadataName(LocalizationSourceMetadataName);
        if (sourceType is null)
        {
            return;
        }

        var skipAttributeType = context.Compilation.GetTypeByMetadataName(SkipAttributeMetadataName);

        context.RegisterOperationAction(
            operationContext => AnalyzeOperation(operationContext, sourceType, skipAttributeType),
            OperationKind.Invocation,
            OperationKind.ObjectCreation);
    }

    private static void AnalyzeOperation(OperationAnalysisContext context, INamedTypeSymbol sourceType, INamedTypeSymbol? skipAttributeType)
    {
        ImmutableArray<IArgumentOperation> arguments;
        ITypeSymbol? contextType = null;
        SyntaxNode? contextSyntax = null;

        switch (context.Operation)
        {
            case IInvocationOperation invocation when invocation.TargetMethod.Name == "Create" && SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, sourceType):
                arguments = invocation.Arguments;
                if (invocation.TargetMethod.IsGenericMethod && invocation.TargetMethod.TypeArguments.Length == 1)
                {
                    contextType = invocation.TargetMethod.TypeArguments[0];
                    contextSyntax = invocation.Syntax;
                }

                break;

            case IObjectCreationOperation creation when SymbolEqualityComparer.Default.Equals(creation.Type, sourceType):
                arguments = creation.Arguments;
                break;

            default:
                return;
        }

        if (IsSkipped(context.ContainingSymbol, skipAttributeType))
        {
            return;
        }

        var value = arguments.FirstOrDefault(argument => argument.Parameter?.Name == "value")?.Value;
        if (value is null)
        {
            return;
        }

        var contextFree = false;
        if (contextType is null)
        {
            var typeArgument = arguments.FirstOrDefault(argument => argument.Parameter?.Name == "type");
            if (typeArgument is null || typeArgument.ArgumentKind == ArgumentKind.DefaultValue || typeArgument.Value.ConstantValue is { HasValue: true, Value: null })
            {
                contextFree = true;
            }
            else if (Unwrap(typeArgument.Value) is ITypeOfOperation typeOf)
            {
                contextType = typeOf.TypeOperand;
                contextSyntax = typeOf.Syntax;
            }
            else
            {
                // The context is a runtime Type, as in a member forwarding a caller's type. Nothing can be checked statically.
                return;
            }
        }

        var constant = value.ConstantValue is { HasValue: true, Value: string text } ? text : null;

        if (contextFree)
        {
            if (!string.IsNullOrWhiteSpace(constant))
            {
                var declaringType = GetDeclaringType(context.ContainingSymbol);
                context.ReportDiagnostic(Diagnostic.Create(
                    s_missingContextRule,
                    context.Operation.Syntax.GetLocation(),
                    Preview(constant!),
                    declaringType?.Name ?? "T"));
            }

            return;
        }

        if (constant is null)
        {
            if (!value.ConstantValue.HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_nonConstantValueRule, value.Syntax.GetLocation()));
            }

            return;
        }

        var declaring = GetDeclaringType(context.ContainingSymbol);
        if (declaring is not null && contextType is not null && !IsDeclaringContext(contextType, declaring))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                s_contextMismatchRule,
                (contextSyntax ?? context.Operation.Syntax).GetLocation(),
                Preview(constant),
                contextType.Name,
                declaring.Name));
        }
    }

    private static bool IsSkipped(ISymbol? symbol, INamedTypeSymbol? skipAttributeType)
    {
        if (skipAttributeType is null)
        {
            return false;
        }

        for (; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, skipAttributeType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsDeclaringContext(ITypeSymbol contextType, INamedTypeSymbol declaringType)
    {
        for (var current = declaringType; current is not null; current = current.ContainingType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, contextType.OriginalDefinition))
            {
                return true;
            }
        }

        return false;
    }

    private static INamedTypeSymbol? GetDeclaringType(ISymbol? symbol)
    {
        for (; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            if (symbol is INamedTypeSymbol type && type.TypeKind != TypeKind.Delegate)
            {
                return type;
            }
        }

        return null;
    }

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    private static string Preview(string value)
    {
        var text = value.Replace('\r', ' ').Replace('\n', ' ');

        return text.Length <= PreviewLength ? text : text.Substring(0, PreviewLength - 1) + "…";
    }
}
