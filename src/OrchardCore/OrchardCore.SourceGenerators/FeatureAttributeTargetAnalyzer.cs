using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#nullable enable

namespace OrchardCore.DisplayManagement.SourceGenerators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FeatureAttributeTargetAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "OCSG002";

    private const string FeatureAttributeMetadataName = "OrchardCore.Modules.FeatureAttribute";
    private const string ManifestFeatureAttributeMetadataName = "OrchardCore.Modules.Manifest.FeatureAttribute";
    private const string ModuleAttributeMetadataName = "OrchardCore.Modules.Manifest.ModuleAttribute";
    private const string ModuleMarkerAttributeMetadataName = "OrchardCore.Modules.Manifest.ModuleMarkerAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "FeatureAttribute targets must belong to the current module",
        "Feature '{0}' is not defined by the current module '{1}', so OrchardCore will ignore this target. Known local feature IDs: {2}.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "OrchardCore resolves OrchardCore.Modules.FeatureAttribute targets only against features declared by the same assembly. Referencing a feature ID from another module is silently ignored at runtime and the attributed type falls back to the current module's feature mapping.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(InitializeCompilation);
    }

    private static void InitializeCompilation(CompilationStartAnalysisContext context)
    {
        var featureAttributeType = context.Compilation.GetTypeByMetadataName(FeatureAttributeMetadataName);
        var manifestFeatureAttributeType = context.Compilation.GetTypeByMetadataName(ManifestFeatureAttributeMetadataName);
        var moduleAttributeType = context.Compilation.GetTypeByMetadataName(ModuleAttributeMetadataName);

        if (featureAttributeType is null || manifestFeatureAttributeType is null || moduleAttributeType is null)
        {
            return;
        }

        var moduleMarkerAttributeType = context.Compilation.GetTypeByMetadataName(ModuleMarkerAttributeMetadataName);

        context.RegisterSymbolAction(
            symbolContext => AnalyzeNamedType(
                symbolContext,
                featureAttributeType,
                manifestFeatureAttributeType,
                moduleAttributeType,
                moduleMarkerAttributeType),
            SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(
        SymbolAnalysisContext context,
        INamedTypeSymbol featureAttributeType,
        INamedTypeSymbol manifestFeatureAttributeType,
        INamedTypeSymbol moduleAttributeType,
        INamedTypeSymbol? moduleMarkerAttributeType)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (namedType.TypeKind != TypeKind.Class)
        {
            return;
        }

        foreach (var attribute in namedType.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, featureAttributeType))
            {
                continue;
            }

            var targetedFeatureId = GetStringArgument(attribute, constructorIndex: 0, namedArgumentName: "FeatureName");

            if (string.IsNullOrWhiteSpace(targetedFeatureId))
            {
                continue;
            }

            var requestedFeatureId = targetedFeatureId!.Trim();

            if (!TryGetValidFeatureIds(
                namedType.ContainingAssembly,
                manifestFeatureAttributeType,
                moduleAttributeType,
                moduleMarkerAttributeType,
                out var currentModuleId,
                out var validFeatureIds))
            {
                continue;
            }

            if (validFeatureIds.Contains(requestedFeatureId))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                Rule,
                GetAttributeLocation(attribute, context.CancellationToken),
                requestedFeatureId,
                currentModuleId,
                string.Join(", ", validFeatureIds.OrderBy(id => id, StringComparer.Ordinal))));
        }
    }

    private static bool TryGetValidFeatureIds(
        IAssemblySymbol assembly,
        INamedTypeSymbol manifestFeatureAttributeType,
        INamedTypeSymbol moduleAttributeType,
        INamedTypeSymbol? moduleMarkerAttributeType,
        out string currentModuleId,
        out ImmutableHashSet<string> validFeatureIds)
    {
        var assemblyAttributes = assembly.GetAttributes();

        var explicitFeatureIds = assemblyAttributes
            .Where(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, manifestFeatureAttributeType))
            .Select(attribute => GetStringArgument(attribute, constructorIndex: 0, namedArgumentName: "Id"))
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id => id!)
            .ToImmutableHashSet(StringComparer.Ordinal);

        var moduleAttributes = assemblyAttributes
            .Where(attribute => IsSameOrDerivedFrom(attribute.AttributeClass, moduleAttributeType))
            .ToImmutableArray();

        if (explicitFeatureIds.Count == 0 && moduleAttributes.IsDefaultOrEmpty)
        {
            currentModuleId = string.Empty;
            validFeatureIds = ImmutableHashSet<string>.Empty.WithComparer(StringComparer.Ordinal);
            return false;
        }

        currentModuleId = GetCurrentModuleId(assembly, moduleAttributes, moduleMarkerAttributeType);

        validFeatureIds = explicitFeatureIds.Count > 0
            ? explicitFeatureIds
            : ImmutableHashSet.Create(StringComparer.Ordinal, currentModuleId);

        return true;
    }

    private static string GetCurrentModuleId(
        IAssemblySymbol assembly,
        ImmutableArray<AttributeData> moduleAttributes,
        INamedTypeSymbol? moduleMarkerAttributeType)
    {
        var moduleAttribute = moduleAttributes.FirstOrDefault(attribute =>
            moduleMarkerAttributeType is null ||
            !SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, moduleMarkerAttributeType))
            ?? moduleAttributes.FirstOrDefault();

        var moduleId = moduleAttribute is null
            ? null
            : GetStringArgument(moduleAttribute, constructorIndex: 0, namedArgumentName: "Id");

        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return assembly.Name;
        }

        return moduleId!;
    }

    private static bool IsSameOrDerivedFrom(INamedTypeSymbol? type, INamedTypeSymbol baseType)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }

        return false;
    }

    private static string? GetStringArgument(AttributeData attribute, int constructorIndex, string namedArgumentName)
    {
        if (attribute.ConstructorArguments.Length > constructorIndex)
        {
            var constructorValue = attribute.ConstructorArguments[constructorIndex].Value as string;

            if (!string.IsNullOrWhiteSpace(constructorValue))
            {
                return constructorValue;
            }
        }

        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key == namedArgumentName)
            {
                return namedArgument.Value.Value as string;
            }
        }

        return null;
    }

    private static Location GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken)
    {
        if (attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken) is not AttributeSyntax attributeSyntax)
        {
            return Location.None;
        }

        return attributeSyntax.ArgumentList?.Arguments.FirstOrDefault()?.GetLocation()
            ?? attributeSyntax.GetLocation();
    }
}
