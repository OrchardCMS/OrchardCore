using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.DisplayManagement.SourceGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SealedShapeTypeAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "OCSG001";

    private const string IShapeMetadataName = "OrchardCore.DisplayManagement.IShape";
    private const string ShapeFactoryExtensionsMetadataName = "OrchardCore.DisplayManagement.ShapeFactoryExtensions";
    private const string DisplayDriverBaseMetadataName = "OrchardCore.DisplayManagement.Handlers.DisplayDriverBase";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Sealed types can't be used as proxy-based shapes",
        "Type '{0}' is sealed and does not implement IShape, so OrchardCore can't create a proxy-based shape for this '{1}' call",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "OrchardCore creates strongly typed shapes by subclassing the model type with Castle DynamicProxy. Sealed types that do not implement IShape fail at runtime. Remove the sealed modifier, implement IShape, or use a non-proxy shape creation pattern such as View, Copy, or Dynamic.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var targetMethod = invocation.TargetMethod;

        if (!IsProxyBasedShapeMethod(targetMethod.OriginalDefinition))
        {
            return;
        }

        if (targetMethod.TypeArguments.Length == 0)
        {
            return;
        }

        var modelType = targetMethod.TypeArguments[0];
        if (modelType is not INamedTypeSymbol namedType || !namedType.IsSealed)
        {
            return;
        }

        var iShapeType = context.Compilation.GetTypeByMetadataName(IShapeMetadataName);
        if (iShapeType is not null && namedType.AllInterfaces.Contains(iShapeType, SymbolEqualityComparer.Default))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            invocation.Syntax.GetLocation(),
            namedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            targetMethod.Name));
    }

    private static bool IsProxyBasedShapeMethod(IMethodSymbol method)
    {
        if (method.TypeParameters.Length == 0 || !method.TypeParameters[0].HasReferenceTypeConstraint)
        {
            return false;
        }

        var containingType = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (containingType == $"global::{DisplayDriverBaseMetadataName}" && method.Name == "Initialize")
        {
            return true;
        }

        if (containingType != $"global::{ShapeFactoryExtensionsMetadataName}" || method.Name != "CreateAsync")
        {
            return false;
        }

        foreach (var parameter in method.Parameters)
        {
            if (IsDelegateReceivingModel(parameter.Type, method.TypeParameters[0]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsDelegateReceivingModel(ITypeSymbol type, ITypeParameterSymbol modelTypeParameter)
    {
        if (type is not INamedTypeSymbol namedType || !namedType.IsGenericType || namedType.TypeArguments.Length == 0)
        {
            return false;
        }

        if (!SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], modelTypeParameter))
        {
            return false;
        }

        var originalDefinition = namedType.ConstructedFrom.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return originalDefinition is "global::System.Action<T>"
            or "global::System.Action<T1, T2>"
            or "global::System.Func<T, global::System.Threading.Tasks.ValueTask>"
            or "global::System.Func<T1, T2, global::System.Threading.Tasks.ValueTask>";
    }
}
