using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#nullable enable

namespace OrchardCore.DisplayManagement.SourceGenerators;

[Generator]
public class ShapeFactoryGenerator : IIncrementalGenerator
{
    private const string ShapeFactoryExtensionsFullName = "OrchardCore.DisplayManagement.ShapeFactoryExtensions";
    private const string IShapeFactoryFullName = "OrchardCore.DisplayManagement.IShapeFactory";
    private const string IShapeFullName = "OrchardCore.DisplayManagement.IShape";
    private const string DisplayDriverBaseFullName = "OrchardCore.DisplayManagement.Handlers.DisplayDriverBase";
    private const string ValueTaskFullName = "System.Threading.Tasks.ValueTask";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var invocations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax,
                transform: static (context, ct) => GetInvocationInfo(context, ct))
            .Where(static info => info is not null);

        context.RegisterSourceOutput(
            invocations.Collect(),
            static (spc, invocations) => Execute(invocations!, spc));
    }

    private static InvocationInfo? GetInvocationInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, cancellationToken);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return null;
        }

        var targetMethod = methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition;

        if (!methodSymbol.IsGenericMethod ||
            methodSymbol.TypeArguments.Length == 0 ||
            methodSymbol.TypeArguments[0] is not INamedTypeSymbol modelType ||
            modelType.TypeKind != TypeKind.Class ||
            modelType.IsAbstract ||
            ImplementsIShape(modelType) ||
            !HasAccessibleParameterlessConstructor(modelType))
        {
            return null;
        }

        var logicalParameters = GetLogicalParameters(methodSymbol);
        var invocationKind = GetInvocationKind(targetMethod, logicalParameters, modelType);

        if (invocationKind is null)
        {
            return null;
        }

        var location = context.SemanticModel.GetInterceptableLocation(invocation, cancellationToken);
        if (location is null)
        {
            return null;
        }

        return new InvocationInfo(location, modelType, invocationKind.Value, GetStateType(invocationKind.Value, logicalParameters));
    }

    private static ImmutableArray<IParameterSymbol> GetLogicalParameters(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Parameters.Length > 0 &&
            methodSymbol.Parameters[0].Type.ToDisplayString() == IShapeFactoryFullName)
        {
            return [.. methodSymbol.Parameters.Skip(1)];
        }

        return methodSymbol.Parameters;
    }

    private static ITypeSymbol? GetStateType(InvocationKind invocationKind, ImmutableArray<IParameterSymbol> logicalParameters)
        => invocationKind is InvocationKind.ActionWithState or InvocationKind.FuncWithState ? logicalParameters[2].Type : null;

    private static InvocationKind? GetInvocationKind(IMethodSymbol targetMethod, ImmutableArray<IParameterSymbol> logicalParameters, INamedTypeSymbol modelType)
    {
        if (targetMethod.Name == "CreateAsync" &&
            targetMethod.ContainingType?.ToDisplayString() == ShapeFactoryExtensionsFullName)
        {
            return GetShapeFactoryInvocationKind(logicalParameters, modelType);
        }

        if (targetMethod.Name == "Initialize" &&
            InheritsFrom(targetMethod.ContainingType, DisplayDriverBaseFullName))
        {
            return GetDisplayDriverInvocationKind(logicalParameters, modelType);
        }

        return null;
    }

    private static InvocationKind? GetShapeFactoryInvocationKind(ImmutableArray<IParameterSymbol> logicalParameters, INamedTypeSymbol modelType)
    {
        if (logicalParameters.IsDefaultOrEmpty)
        {
            return null;
        }

        if (logicalParameters.Length == 1)
        {
            if (IsAction(logicalParameters[0].Type, modelType))
            {
                return InvocationKind.ActionWithoutShapeType;
            }

            if (IsFunc(logicalParameters[0].Type, modelType, null))
            {
                return InvocationKind.FuncWithoutShapeType;
            }

            return null;
        }

        if (logicalParameters.Length == 2 && logicalParameters[0].Type.SpecialType == SpecialType.System_String)
        {
            if (IsAction(logicalParameters[1].Type, modelType))
            {
                return InvocationKind.ActionWithShapeType;
            }

            if (IsFunc(logicalParameters[1].Type, modelType, null))
            {
                return InvocationKind.FuncWithShapeType;
            }

            return null;
        }

        if (logicalParameters.Length == 3 && logicalParameters[0].Type.SpecialType == SpecialType.System_String)
        {
            if (IsAction(logicalParameters[1].Type, modelType, logicalParameters[2].Type))
            {
                return InvocationKind.ActionWithState;
            }

            if (IsFunc(logicalParameters[1].Type, modelType, logicalParameters[2].Type))
            {
                return InvocationKind.FuncWithState;
            }
        }

        return null;
    }

    private static InvocationKind? GetDisplayDriverInvocationKind(ImmutableArray<IParameterSymbol> logicalParameters, INamedTypeSymbol modelType)
    {
        if (logicalParameters.IsDefaultOrEmpty)
        {
            return null;
        }

        if (logicalParameters.Length == 1)
        {
            if (IsAction(logicalParameters[0].Type, modelType))
            {
                return InvocationKind.DisplayDriverActionWithoutShapeType;
            }

            if (IsFunc(logicalParameters[0].Type, modelType, null))
            {
                return InvocationKind.DisplayDriverFuncWithoutShapeType;
            }

            return null;
        }

        if (logicalParameters.Length == 2 && logicalParameters[0].Type.SpecialType == SpecialType.System_String)
        {
            if (IsAction(logicalParameters[1].Type, modelType))
            {
                return InvocationKind.DisplayDriverActionWithShapeType;
            }

            if (IsFunc(logicalParameters[1].Type, modelType, null))
            {
                return InvocationKind.DisplayDriverFuncWithShapeType;
            }
        }

        return null;
    }

    private static bool IsAction(ITypeSymbol typeSymbol, INamedTypeSymbol modelType, ITypeSymbol? stateType = null)
        => typeSymbol is INamedTypeSymbol namedType &&
           namedType.ContainingNamespace?.ToDisplayString() == "System" &&
           namedType.Name == "Action" &&
           ((stateType is null &&
             namedType.TypeArguments.Length == 1 &&
             SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], modelType)) ||
            (stateType is not null &&
             namedType.TypeArguments.Length == 2 &&
             SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], modelType) &&
             SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[1], stateType)));

    private static bool IsFunc(ITypeSymbol typeSymbol, INamedTypeSymbol modelType, ITypeSymbol? stateType)
    {
        if (typeSymbol is not INamedTypeSymbol namedType ||
            namedType.ContainingNamespace?.ToDisplayString() != "System" ||
            namedType.Name != "Func")
        {
            return false;
        }

        if (stateType is null)
        {
            return namedType.TypeArguments.Length == 2 &&
                SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], modelType) &&
                namedType.TypeArguments[1].ToDisplayString() == ValueTaskFullName;
        }

        return namedType.TypeArguments.Length == 3 &&
            SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[0], modelType) &&
            SymbolEqualityComparer.Default.Equals(namedType.TypeArguments[1], stateType) &&
            namedType.TypeArguments[2].ToDisplayString() == ValueTaskFullName;
    }

    private static bool ImplementsIShape(INamedTypeSymbol typeSymbol)
        => typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == IShapeFullName);

    private static bool InheritsFrom(INamedTypeSymbol? typeSymbol, string fullName)
    {
        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == fullName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasAccessibleParameterlessConstructor(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.InstanceConstructors.Length == 0)
        {
            return true;
        }

        return typeSymbol.InstanceConstructors.Any(ctor =>
            ctor.Parameters.Length == 0 &&
            ctor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.Protected or Accessibility.ProtectedOrInternal);
    }

    private static void Execute(ImmutableArray<InvocationInfo> invocations, SourceProductionContext context)
    {
        if (invocations.IsDefaultOrEmpty)
        {
            return;
        }

        var validInvocations = invocations
            .Where(static invocation => invocation is not null)
            .Distinct(InvocationInfoComparer.Instance)
            .ToArray();

        if (validInvocations.Length == 0)
        {
            return;
        }

        var modelTypes = validInvocations
            .Select(static invocation => invocation.ModelType)
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .ToArray();

        var sb = new StringBuilder();

        sb.Append("""
                    // <auto-generated />
                    #nullable enable

                    namespace System.Runtime.CompilerServices
                    {
                        [global::System.Diagnostics.Conditional("DEBUG")]
                        [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                        sealed file class InterceptsLocationAttribute : global::System.Attribute
                        {
                            public InterceptsLocationAttribute(int version, string data)
                            {
                                _ = version;
                                _ = data;
                            }
                        }
                    }

                    namespace OrchardCore.DisplayManagement.Generated
                    {
                        file static class ShapeFactoryInterceptorHelpers
                        {
                            public static async global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> Awaited(global::System.Threading.Tasks.ValueTask task, global::OrchardCore.DisplayManagement.IShape shape)
                            {
                                await task;
                                return shape;
                            }
                        }

                    """);

        sb.AppendLine();

        foreach (var modelType in modelTypes)
        {
            GenerateShapeType(sb, modelType);
        }

        foreach (var invocation in validInvocations)
        {
            GenerateInterceptor(sb, invocation);
        }

        sb.AppendLine("}");

        context.AddSource("ShapeFactoryGenerator.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateShapeType(StringBuilder sb, INamedTypeSymbol modelType)
    {
        var baseTypeName = modelType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var generatedTypeName = GetGeneratedTypeName(modelType);

        sb.AppendLine($"    file sealed class {generatedTypeName} : {baseTypeName}, global::OrchardCore.DisplayManagement.IShape, global::OrchardCore.DisplayManagement.IPositioned");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly global::OrchardCore.DisplayManagement.Views.ShapeViewModel _shape = new();");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Metadata")}global::OrchardCore.DisplayManagement.Shapes.ShapeMetadata Metadata => _shape.Metadata;");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Position")}string Position");
        sb.AppendLine("        {");
        sb.AppendLine("            get => _shape.Position;");
        sb.AppendLine("            set => _shape.Position = value;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Id")}string Id");
        sb.AppendLine("        {");
        sb.AppendLine("            get => _shape.Id;");
        sb.AppendLine("            set => _shape.Id = value;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "TagName")}string TagName");
        sb.AppendLine("        {");
        sb.AppendLine("            get => _shape.TagName;");
        sb.AppendLine("            set => _shape.TagName = value;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Classes")}global::System.Collections.Generic.IList<string> Classes => _shape.Classes;");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Attributes")}global::System.Collections.Generic.IDictionary<string, string> Attributes => _shape.Attributes;");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Properties")}global::System.Collections.Generic.IDictionary<string, object> Properties => _shape.Properties;");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "Items")}global::System.Collections.Generic.IReadOnlyList<global::OrchardCore.DisplayManagement.IPositioned> Items => _shape.Items;");
        sb.AppendLine();
        sb.AppendLine($"        public {GetNewModifier(modelType, "AddAsync", 2)}global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> AddAsync(object item, string position)");
        sb.AppendLine("            => _shape.AddAsync(item, position);");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string GetNewModifier(INamedTypeSymbol modelType, string memberName, int parameterCount = 0)
        => HasConflictingMember(modelType, memberName, parameterCount) ? "new " : string.Empty;

    private static bool HasConflictingMember(INamedTypeSymbol modelType, string memberName, int parameterCount)
        => modelType.GetMembers(memberName).Any(member =>
            member switch
            {
                IPropertySymbol => parameterCount == 0,
                IMethodSymbol method => method.Parameters.Length == parameterCount,
                _ => false,
            });

    private static void GenerateInterceptor(StringBuilder sb, InvocationInfo invocation)
    {
        var interceptorClassName = $"Interceptor_{GetStableId(invocation.Location.Data)}";
        var generatedTypeName = GetGeneratedTypeName(invocation.ModelType);
        var modelTypeName = invocation.ModelType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        sb.AppendLine($"    file static class {interceptorClassName}");
        sb.AppendLine("    {");
        sb.AppendLine($"        [global::System.Runtime.CompilerServices.InterceptsLocation({invocation.Location.Version}, \"{invocation.Location.Data}\")]");

        switch (invocation.Kind)
        {
            case InvocationKind.ActionWithoutShapeType:
                sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> InterceptCreateAsync(this global::OrchardCore.DisplayManagement.IShapeFactory factory, global::System.Action<{modelTypeName}>? initialize = null)");
                sb.AppendLine("            => factory.CreateAsync(");
                sb.AppendLine($"                \"{invocation.ModelType.Name}\",");
                sb.AppendLine("                static initialize => ShapeFactory(initialize),");
                sb.AppendLine("                initialize);");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> ShapeFactory(global::System.Action<{modelTypeName}>? initialize)");
                sb.AppendLine("        {");
                sb.AppendLine($"            var shape = (global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}();");
                sb.AppendLine($"            initialize?.Invoke(({modelTypeName})shape);");
                sb.AppendLine("            return global::System.Threading.Tasks.ValueTask.FromResult(shape);");
                sb.AppendLine("        }");
                break;

            case InvocationKind.ActionWithShapeType:
                sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> InterceptCreateAsync(this global::OrchardCore.DisplayManagement.IShapeFactory factory, string shapeType, global::System.Action<{modelTypeName}>? initialize = null)");
                sb.AppendLine("            => factory.CreateAsync(");
                sb.AppendLine("                shapeType,");
                sb.AppendLine("                static initialize => ShapeFactory(initialize),");
                sb.AppendLine("                initialize);");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> ShapeFactory(global::System.Action<{modelTypeName}>? initialize)");
                sb.AppendLine("        {");
                sb.AppendLine($"            var shape = (global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}();");
                sb.AppendLine($"            initialize?.Invoke(({modelTypeName})shape);");
                sb.AppendLine("            return global::System.Threading.Tasks.ValueTask.FromResult(shape);");
                sb.AppendLine("        }");
                break;

            case InvocationKind.ActionWithState:
                var actionStateType = invocation.StateType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> InterceptCreateAsync(this global::OrchardCore.DisplayManagement.IShapeFactory factory, string shapeType, global::System.Action<{modelTypeName}, {actionStateType}> initialize, {actionStateType} state)");
                sb.AppendLine("            => factory.CreateAsync(");
                sb.AppendLine("                shapeType,");
                sb.AppendLine("                static state => ShapeFactory(state.initialize, state.state),");
                sb.AppendLine("                (initialize, state));");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> ShapeFactory(global::System.Action<{modelTypeName}, {actionStateType}> initialize, {actionStateType} state)");
                sb.AppendLine("        {");
                sb.AppendLine($"            var shape = (global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}();");
                sb.AppendLine($"            initialize?.Invoke(({modelTypeName})shape, state);");
                sb.AppendLine("            return global::System.Threading.Tasks.ValueTask.FromResult(shape);");
                sb.AppendLine("        }");
                break;

            case InvocationKind.FuncWithoutShapeType:
                sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> InterceptCreateAsync(this global::OrchardCore.DisplayManagement.IShapeFactory factory, global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("            => factory.CreateAsync(");
                sb.AppendLine($"                \"{invocation.ModelType.Name}\",");
                sb.AppendLine("                static initializeAsync => ShapeFactory(initializeAsync),");
                sb.AppendLine("                initializeAsync);");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> ShapeFactory(global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("        {");
                sb.AppendLine($"            var shape = (global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}();");
                sb.AppendLine($"            var task = initializeAsync?.Invoke(({modelTypeName})shape) ?? global::System.Threading.Tasks.ValueTask.CompletedTask;");
                sb.AppendLine();
                sb.AppendLine("            if (!task.IsCompletedSuccessfully)");
                sb.AppendLine("            {");
                sb.AppendLine("                return ShapeFactoryInterceptorHelpers.Awaited(task, shape);");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return global::System.Threading.Tasks.ValueTask.FromResult(shape);");
                sb.AppendLine("        }");
                break;

            case InvocationKind.FuncWithShapeType:
                sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> InterceptCreateAsync(this global::OrchardCore.DisplayManagement.IShapeFactory factory, string shapeType, global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("            => factory.CreateAsync(");
                sb.AppendLine("                shapeType,");
                sb.AppendLine("                static initializeAsync => ShapeFactory(initializeAsync),");
                sb.AppendLine("                initializeAsync);");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> ShapeFactory(global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("        {");
                sb.AppendLine($"            var shape = (global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}();");
                sb.AppendLine($"            var task = initializeAsync?.Invoke(({modelTypeName})shape) ?? global::System.Threading.Tasks.ValueTask.CompletedTask;");
                sb.AppendLine();
                sb.AppendLine("            if (!task.IsCompletedSuccessfully)");
                sb.AppendLine("            {");
                sb.AppendLine("                return ShapeFactoryInterceptorHelpers.Awaited(task, shape);");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return global::System.Threading.Tasks.ValueTask.FromResult(shape);");
                sb.AppendLine("        }");
                break;

            case InvocationKind.FuncWithState:
                var funcStateType = invocation.StateType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"        public static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> InterceptCreateAsync(this global::OrchardCore.DisplayManagement.IShapeFactory factory, string shapeType, global::System.Func<{modelTypeName}, {funcStateType}, global::System.Threading.Tasks.ValueTask> initializeAsync, {funcStateType} state)");
                sb.AppendLine("            => factory.CreateAsync(");
                sb.AppendLine("                shapeType,");
                sb.AppendLine("                static state => ShapeFactory(state.initializeAsync, state.state),");
                sb.AppendLine("                (initializeAsync, state));");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.ValueTask<global::OrchardCore.DisplayManagement.IShape> ShapeFactory(global::System.Func<{modelTypeName}, {funcStateType}, global::System.Threading.Tasks.ValueTask> initializeAsync, {funcStateType} state)");
                sb.AppendLine("        {");
                sb.AppendLine($"            var shape = (global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}();");
                sb.AppendLine($"            var task = initializeAsync?.Invoke(({modelTypeName})shape, state) ?? global::System.Threading.Tasks.ValueTask.CompletedTask;");
                sb.AppendLine();
                sb.AppendLine("            if (!task.IsCompletedSuccessfully)");
                sb.AppendLine("            {");
                sb.AppendLine("                return ShapeFactoryInterceptorHelpers.Awaited(task, shape);");
                sb.AppendLine("            }");
                sb.AppendLine();
                sb.AppendLine("            return global::System.Threading.Tasks.ValueTask.FromResult(shape);");
                sb.AppendLine("        }");
                break;

            case InvocationKind.DisplayDriverActionWithoutShapeType:
                sb.AppendLine($"        public static global::OrchardCore.DisplayManagement.Views.ShapeResult InterceptInitialize(this global::OrchardCore.DisplayManagement.Handlers.DisplayDriverBase driver, global::System.Action<{modelTypeName}> initialize)");
                sb.AppendLine($"            => driver.Factory(\"{invocation.ModelType.Name}\",");
                sb.AppendLine($"                static _ => global::System.Threading.Tasks.ValueTask.FromResult<global::OrchardCore.DisplayManagement.IShape>((global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}()),");
                sb.AppendLine($"                shape => InterceptInitialize(({modelTypeName})shape, initialize));");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.Task InterceptInitialize({modelTypeName} shape, global::System.Action<{modelTypeName}> initialize)");
                sb.AppendLine("        {");
                sb.AppendLine("            initialize?.Invoke(shape);");
                sb.AppendLine("            return global::System.Threading.Tasks.Task.CompletedTask;");
                sb.AppendLine("        }");
                break;

            case InvocationKind.DisplayDriverActionWithShapeType:
                sb.AppendLine($"        public static global::OrchardCore.DisplayManagement.Views.ShapeResult InterceptInitialize(this global::OrchardCore.DisplayManagement.Handlers.DisplayDriverBase driver, string shapeType, global::System.Action<{modelTypeName}> initialize)");
                sb.AppendLine("            => driver.Factory(shapeType,");
                sb.AppendLine($"                static _ => global::System.Threading.Tasks.ValueTask.FromResult<global::OrchardCore.DisplayManagement.IShape>((global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}()),");
                sb.AppendLine($"                shape => InterceptInitialize(({modelTypeName})shape, initialize));");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.Task InterceptInitialize({modelTypeName} shape, global::System.Action<{modelTypeName}> initialize)");
                sb.AppendLine("        {");
                sb.AppendLine("            initialize?.Invoke(shape);");
                sb.AppendLine("            return global::System.Threading.Tasks.Task.CompletedTask;");
                sb.AppendLine("        }");
                break;

            case InvocationKind.DisplayDriverFuncWithoutShapeType:
                sb.AppendLine($"        public static global::OrchardCore.DisplayManagement.Views.ShapeResult InterceptInitialize(this global::OrchardCore.DisplayManagement.Handlers.DisplayDriverBase driver, global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine($"            => driver.Factory(\"{invocation.ModelType.Name}\",");
                sb.AppendLine($"                static _ => global::System.Threading.Tasks.ValueTask.FromResult<global::OrchardCore.DisplayManagement.IShape>((global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}()),");
                sb.AppendLine($"                shape => InterceptInitialize(({modelTypeName})shape, initializeAsync));");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.Task InterceptInitialize({modelTypeName} shape, global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("            => initializeAsync?.Invoke(shape).AsTask() ?? global::System.Threading.Tasks.Task.CompletedTask;");
                break;

            case InvocationKind.DisplayDriverFuncWithShapeType:
                sb.AppendLine($"        public static global::OrchardCore.DisplayManagement.Views.ShapeResult InterceptInitialize(this global::OrchardCore.DisplayManagement.Handlers.DisplayDriverBase driver, string shapeType, global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("            => driver.Factory(shapeType,");
                sb.AppendLine($"                static _ => global::System.Threading.Tasks.ValueTask.FromResult<global::OrchardCore.DisplayManagement.IShape>((global::OrchardCore.DisplayManagement.IShape)new {generatedTypeName}()),");
                sb.AppendLine($"                shape => InterceptInitialize(({modelTypeName})shape, initializeAsync));");
                sb.AppendLine();
                sb.AppendLine($"        private static global::System.Threading.Tasks.Task InterceptInitialize({modelTypeName} shape, global::System.Func<{modelTypeName}, global::System.Threading.Tasks.ValueTask> initializeAsync)");
                sb.AppendLine("            => initializeAsync?.Invoke(shape).AsTask() ?? global::System.Threading.Tasks.Task.CompletedTask;");
                break;
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string GetGeneratedTypeName(INamedTypeSymbol typeSymbol)
        => $"GeneratedShape_{GetStableId(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))}";

    private static string GetStableId(string value)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));

        return BitConverter.ToString(hash).Replace("-", "");
    }

    private sealed class InvocationInfo
    {
        public InvocationInfo(InterceptableLocation location, INamedTypeSymbol modelType, InvocationKind kind, ITypeSymbol? stateType)
        {
            Location = location;
            ModelType = modelType;
            Kind = kind;
            StateType = stateType;
        }

        public InterceptableLocation Location { get; }
        public INamedTypeSymbol ModelType { get; }
        public InvocationKind Kind { get; }
        public ITypeSymbol? StateType { get; }
    }

    private sealed class InvocationInfoComparer : IEqualityComparer<InvocationInfo>
    {
        public static InvocationInfoComparer Instance { get; } = new();

        public bool Equals(InvocationInfo? x, InvocationInfo? y)
            => x?.Location.Data == y?.Location.Data &&
               x?.Location.Version == y?.Location.Version;

        public int GetHashCode(InvocationInfo obj)
            => ((obj.Location.Data?.GetHashCode() ?? 0) * 397) ^ obj.Location.Version;
    }

    private enum InvocationKind
    {
        ActionWithoutShapeType,
        ActionWithShapeType,
        ActionWithState,
        FuncWithoutShapeType,
        FuncWithShapeType,
        FuncWithState,
        DisplayDriverActionWithoutShapeType,
        DisplayDriverActionWithShapeType,
        DisplayDriverFuncWithoutShapeType,
        DisplayDriverFuncWithShapeType,
    }
}
