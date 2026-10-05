using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.Localization.Extraction;

public sealed record LocalizationCall(IOperation Receiver, IOperation Key, IOperation? Plural = null, IOperation? PluralForms = null);

public interface ICSharpLocalizationAdapter
{
    bool TryGetCall(IInvocationOperation invocation, out LocalizationCall? call);
}

public sealed class CSharpLocalizationExtractor
{
    private readonly Compilation _compilation;
    private readonly ExtractionOptions _options;
    private readonly LocalizationCatalog _catalog;
    private readonly IReadOnlyList<ICSharpLocalizationAdapter> _adapters;
    private readonly INamedTypeSymbol? _skipExtractionAttribute;
    private readonly LocalizationSourceExtractor _sources;
    private readonly Dictionary<ISymbol, List<IOperation>> _assignments = new(SymbolEqualityComparer.Default);

    public CSharpLocalizationExtractor(Compilation compilation, ExtractionOptions options, LocalizationCatalog catalog, IEnumerable<ICSharpLocalizationAdapter>? adapters = null)
    {
        _compilation = compilation;
        _options = options;
        _catalog = catalog;
        _adapters = adapters?.ToArray() ?? [];
        _skipExtractionAttribute = compilation.GetTypeByMetadataName("OrchardCore.Localization.SkipLocalizationExtractionAttribute");
        IndexAssignments();
        _sources = new LocalizationSourceExtractor(compilation, options, catalog, this);
    }

    public void Extract(SyntaxTree tree, string? viewContext = null, CancellationToken cancellationToken = default)
    {
        _sources.Extract(tree, cancellationToken);
        var model = _compilation.GetSemanticModel(tree);
        foreach (var node in tree.GetRoot(cancellationToken).DescendantNodes())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (node is not (ElementAccessExpressionSyntax or InvocationExpressionSyntax) || ShouldSkip(model.GetEnclosingSymbol(node.SpanStart, cancellationToken)))
            {
                continue;
            }

            LocalizationCall? call = null;
            if (node is ElementAccessExpressionSyntax && model.GetOperation(node, cancellationToken) is IPropertyReferenceOperation property && property.Property.IsIndexer && IsLocalizer(property.Property.ContainingType) && property.Instance is not null)
            {
                var key = property.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0);
                if (key is not null)
                {
                    call = new LocalizationCall(property.Instance, key.Value);
                }
            }
            else if (node is InvocationExpressionSyntax && model.GetOperation(node, cancellationToken) is IInvocationOperation invocation)
            {
                call = GetCall(invocation);
                foreach (var adapter in _adapters)
                {
                    if (call is null && adapter.TryGetCall(invocation, out var adapted))
                    {
                        call = adapted;
                    }
                }
            }

            if (call is not null)
            {
                ExtractCall(call, node, viewContext, model);
            }
        }
    }

    internal bool ShouldSkip(ISymbol? symbol)
    {
        if (_skipExtractionAttribute is null)
        {
            return false;
        }

        for (; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            if (symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _skipExtractionAttribute)))
            {
                return true;
            }
        }

        return false;
    }

    private static LocalizationCall? GetCall(IInvocationOperation invocation)
    {
        var method = invocation.TargetMethod;
        if (method.Name is not ("GetString" or "GetHtml" or "Plural"))
        {
            return null;
        }

        var container = method.ContainingType.ToDisplayString();
        var supportedExtension = container is "Microsoft.Extensions.Localization.StringLocalizerExtensions" or "Microsoft.AspNetCore.Mvc.Localization.HtmlLocalizerExtensions" or "Microsoft.AspNetCore.Mvc.Localization.ViewLocalizerExtensions";
        var receiver = invocation.Instance;
        if (supportedExtension)
        {
            receiver ??= invocation.Arguments.FirstOrDefault(argument => IsLocalizer(argument.Parameter?.Type))?.Value;
        }
        else if (!IsLocalizer(method.ContainingType) || method.Name == "Plural")
        {
            return null;
        }

        if (receiver is null)
        {
            return null;
        }

        var forms = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "pluralForms")?.Value;
        var key = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name is "name" or "singular")?.Value;
        if (key is null && forms is null)
        {
            return null;
        }

        var plural = method.Name == "Plural" ? invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "plural")?.Value : null;
        return new LocalizationCall(receiver, key ?? forms!, plural, forms);
    }

    private void ExtractCall(LocalizationCall call, SyntaxNode node, string? viewContext, SemanticModel model)
    {
        var span = node.GetLocation().GetMappedLineSpan();
        var source = _options.GetSource(span.Path, span.StartLinePosition.Line + 1);
        string? key;
        string? plural = null;
        if (call.PluralForms is not null)
        {
            var forms = GetForms(call.PluralForms, new HashSet<ISymbol>(SymbolEqualityComparer.Default));
            if (forms is null || forms.Length != 2)
            {
                _catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC003", "Gettext extraction requires exactly two constant source plural forms.", source));
                return;
            }

            key = forms[0];
            plural = forms[1];
        }
        else
        {
            key = GetConstant(call.Key);
            if (call.Plural is not null)
            {
                plural = GetConstant(call.Plural);
            }
        }

        if (key is null || call.Plural is not null && plural is null)
        {
            _catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC001", "The localization key is not a compile-time constant string.", source));
            return;
        }

        var context = ResolveContext(call.Receiver, viewContext, new HashSet<ISymbol>(SymbolEqualityComparer.Default));
        if (context is null && model.GetEnclosingSymbol(node.SpanStart)?.ContainingType is { } containingType)
        {
            context = GetResourceName(containingType, allowGenericDefinition: true);
        }

        if (context is null)
        {
            _catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC002", "The localizer's runtime context cannot be resolved statically. Use a typed localizer or an extraction adapter.", source));
            return;
        }

        _catalog.Add(context, key, plural, source, GetComment(node));
    }

    internal static string? GetConstant(IOperation operation)
    {
        operation = Unwrap(operation);
        return operation.ConstantValue is { HasValue: true, Value: string value } ? value : null;
    }

    private string[]? GetForms(IOperation operation, HashSet<ISymbol> visited)
    {
        operation = Unwrap(operation);
        IEnumerable<IOperation>? elements = operation switch
        {
            IArrayCreationOperation { Initializer: not null } array => array.Initializer.ElementValues,
            IArrayInitializerOperation initializer => initializer.ElementValues,
            ICollectionExpressionOperation collection => collection.Elements,
            _ => null,
        };
        if (elements is not null)
        {
            var values = elements.Select(GetConstant).ToArray();
            return values.All(value => value is not null) ? values.Select(value => value!).ToArray() : null;
        }

        var symbol = GetSymbol(operation);
        if (symbol is null || !visited.Add(symbol) || !_assignments.TryGetValue(symbol, out var assignments) || assignments.Count != 1)
        {
            return null;
        }

        return GetForms(assignments[0], visited);
    }

    private string? ResolveContext(IOperation operation, string? viewContext, HashSet<ISymbol> visited)
    {
        operation = Unwrap(operation);
        foreach (var type in GetTypes(operation.Type))
        {
            var definition = GetTypeIdentity(type);
            if (definition is "Microsoft.Extensions.Localization.IStringLocalizer`1" or "Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer`1")
            {
                return GetResourceName(type.TypeArguments[0]);
            }

            if (definition == "Microsoft.AspNetCore.Mvc.Localization.IViewLocalizer")
            {
                return viewContext;
            }
        }

        if (operation is IInvocationOperation invocation && invocation.TargetMethod.Name == "Create" && GetTypes(invocation.TargetMethod.ContainingType).Any(type => type.ToDisplayString() is "Microsoft.Extensions.Localization.IStringLocalizerFactory" or "Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizerFactory"))
        {
            var resourceType = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0)?.Value;
            if (resourceType is not null && Unwrap(resourceType) is ITypeOfOperation typeOf)
            {
                return GetResourceName(typeOf.TypeOperand);
            }

            var baseName = resourceType is null ? null : GetConstant(resourceType);
            var locationOperation = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 1)?.Value;
            var location = locationOperation is null ? null : GetConstant(locationOperation);
            if (baseName is not null && location is not null)
            {
                baseName = baseName.Replace('+', '.');
                var index = baseName.StartsWith(location, StringComparison.OrdinalIgnoreCase) ? location.Length : 0;
                if (baseName.Length > index && baseName[index] == '.')
                {
                    index++;
                }

                if (baseName.IndexOf("Areas.", index, StringComparison.Ordinal) == index)
                {
                    index += "Areas.".Length;
                }

                return baseName.Substring(index);
            }
        }

        var symbol = GetSymbol(operation);
        if (symbol is null || !visited.Add(symbol))
        {
            return null;
        }

        if (symbol is IFieldSymbol field && field.ContainingType.ToDisplayString() == "OrchardCore.Localization.DataAnnotations.DataAnnotationsDefaultErrorMessages")
        {
            return field.ContainingType.ToDisplayString();
        }

        if (!_assignments.TryGetValue(symbol, out var values) || values.Count == 0)
        {
            return null;
        }

        var contexts = values.Select(value => ResolveContext(value, viewContext, new HashSet<ISymbol>(visited, SymbolEqualityComparer.Default))).Distinct(StringComparer.Ordinal).ToArray();
        return contexts.Length == 1 ? contexts[0] : null;
    }

    internal static string? GetResourceName(ITypeSymbol type, bool allowGenericDefinition = false)
    {
        if (type is not INamedTypeSymbol named)
        {
            return null;
        }

        var names = new Stack<string>();
        for (var current = named; current is not null; current = current.ContainingType)
        {
            if (current.IsGenericType && !allowGenericDefinition)
            {
                return null;
            }

            names.Push(current.Name);
        }

        var prefix = named.ContainingNamespace.IsGlobalNamespace ? "" : named.ContainingNamespace.ToDisplayString() + ".";
        return prefix + string.Join('.', names);
    }

    private void IndexAssignments()
    {
        foreach (var tree in _compilation.SyntaxTrees)
        {
            var model = _compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is VariableDeclaratorSyntax { Initializer: not null } variable && model.GetDeclaredSymbol(variable) is { } symbol && model.GetOperation(variable.Initializer.Value) is { } value)
                {
                    AddAssignment(symbol, value);
                }
                else if (node is AssignmentExpressionSyntax assignment && model.GetOperation(assignment) is IAssignmentOperation operation && GetSymbol(operation.Target) is { } target)
                {
                    AddAssignment(target, operation.Value);
                }
                else if (node is PropertyDeclarationSyntax property && model.GetDeclaredSymbol(property) is { } propertySymbol)
                {
                    var expression = property.ExpressionBody?.Expression ?? property.Initializer?.Value;
                    if (expression is not null && model.GetOperation(expression) is { } propertyValue)
                    {
                        AddAssignment(propertySymbol, propertyValue);
                    }
                }
            }
        }
    }

    private void AddAssignment(ISymbol symbol, IOperation value)
    {
        if (!_assignments.TryGetValue(symbol, out var values))
        {
            values = [];
            _assignments.Add(symbol, values);
        }

        values.Add(value);
    }

    internal IOperation? GetAssignedValue(ISymbol symbol)
        => _assignments.TryGetValue(symbol, out var values) && values.Count == 1 ? values[0] : null;

    internal static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    internal static ISymbol? GetSymbol(IOperation operation)
        => Unwrap(operation) switch
        {
            IFieldReferenceOperation field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            _ => null,
        };

    private static bool IsLocalizer(ITypeSymbol? type)
        => GetTypes(type).Any(candidate => GetTypeIdentity(candidate) is "Microsoft.Extensions.Localization.IStringLocalizer" or "Microsoft.Extensions.Localization.IStringLocalizer`1" or "Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer" or "Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer`1" or "Microsoft.AspNetCore.Mvc.Localization.IViewLocalizer");

    private static string GetTypeIdentity(INamedTypeSymbol type)
        => type.ContainingNamespace.ToDisplayString() + "." + type.OriginalDefinition.MetadataName;

    private static IEnumerable<INamedTypeSymbol> GetTypes(ITypeSymbol? type)
    {
        if (type is INamedTypeSymbol named)
        {
            yield return named;
            foreach (var implemented in named.AllInterfaces)
            {
                yield return implemented;
            }
        }
    }

    internal static string? GetComment(SyntaxNode node)
    {
        var text = node.SyntaxTree.GetText();
        var line = text.Lines.GetLineFromPosition(node.SpanStart).LineNumber;
        if (line > 0)
        {
            var preceding = text.Lines[line - 1].ToString().Trim();
            const string prefix = "// TRANSLATORS:";
            if (preceding.StartsWith(prefix, StringComparison.Ordinal))
            {
                return preceding.Substring(prefix.Length).Trim();
            }
        }

        return null;
    }
}
