using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.Localization.Extraction;

internal sealed class LocalizationSourceExtractor
{
    private readonly Compilation _compilation;
    private readonly ExtractionOptions _options;
    private readonly LocalizationCatalog _catalog;
    private readonly CSharpLocalizationExtractor _csharp;
    private readonly INamedTypeSymbol? _sourceType;
    private readonly HashSet<(SyntaxTree Tree, int Start)> _pluralSources = [];

    internal LocalizationSourceExtractor(Compilation compilation, ExtractionOptions options, LocalizationCatalog catalog, CSharpLocalizationExtractor csharp)
    {
        _compilation = compilation;
        _options = options;
        _catalog = catalog;
        _csharp = csharp;
        _sourceType = compilation.GetTypeByMetadataName("OrchardCore.Localization.LocalizationSource");
        if (_sourceType is not null)
        {
            foreach (var tree in compilation.SyntaxTrees)
            {
                var model = compilation.GetSemanticModel(tree);
                foreach (var node in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (!_csharp.ShouldSkip(model.GetEnclosingSymbol(node.SpanStart)) && model.GetOperation(node) is IInvocationOperation invocation && IsDeferredPlural(invocation))
                    {
                        var argument = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "source")?.Value;
                        var source = argument is null ? null : ResolveSource(argument, new HashSet<ISymbol>(SymbolEqualityComparer.Default));
                        if (source is not null)
                        {
                            _pluralSources.Add((source.Syntax.SyntaxTree, source.Syntax.SpanStart));
                        }
                    }
                }
            }
        }
    }

    public void Extract(SyntaxTree tree, CancellationToken cancellationToken = default)
    {
        if (_sourceType is null)
        {
            return;
        }

        var model = _compilation.GetSemanticModel(tree);
        foreach (var node in tree.GetRoot(cancellationToken).DescendantNodes())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (node is not (InvocationExpressionSyntax or BaseObjectCreationExpressionSyntax) || _csharp.ShouldSkip(model.GetEnclosingSymbol(node.SpanStart, cancellationToken)))
            {
                continue;
            }

            var operation = model.GetOperation(node, cancellationToken);
            var source = operation is null ? null : GetSource(operation);
            IOperation? plural = null;
            if (operation is IInvocationOperation invocation && IsDeferredPlural(invocation))
            {
                var argument = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "source")?.Value;
                if (argument is null)
                {
                    continue;
                }

                source = ResolveSource(argument, new HashSet<ISymbol>(SymbolEqualityComparer.Default));
                plural = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Name == "plural")?.Value;
                if (source is null)
                {
                    AddDiagnostic("OCLOC002", "The localization source's runtime context cannot be resolved statically. Use an explicit LocalizationSource declaration.", node);
                    continue;
                }
            }

            if (source is null || source.ContextFree)
            {
                continue;
            }

            if (plural is null && _pluralSources.Contains((source.Syntax.SyntaxTree, source.Syntax.SpanStart)))
            {
                continue;
            }

            var key = CSharpLocalizationExtractor.GetConstant(source.Value);
            var pluralText = plural is null ? null : CSharpLocalizationExtractor.GetConstant(plural);
            if (key is null || plural is not null && pluralText is null)
            {
                AddDiagnostic("OCLOC001", "The localization key is not a compile-time constant string.", node);
                continue;
            }

            var context = source.Type is null ? null : CSharpLocalizationExtractor.GetResourceName(source.Type);
            if (context is null)
            {
                AddDiagnostic("OCLOC002", "The localization source's runtime context cannot be resolved statically. Use a generic Create call or typeof resource type.", node);
                continue;
            }

            var span = node.GetLocation().GetMappedLineSpan();
            _catalog.Add(context, key, pluralText, _options.GetSource(span.Path, span.StartLinePosition.Line + 1), CSharpLocalizationExtractor.GetComment(node));
            if (plural is not null)
            {
                var declaration = source.Syntax.GetLocation().GetMappedLineSpan();
                _catalog.Add(context, key, pluralText, _options.GetSource(declaration.Path, declaration.StartLinePosition.Line + 1), CSharpLocalizationExtractor.GetComment(source.Syntax));
            }
        }
    }

    private SourceCall? GetSource(IOperation operation)
    {
        operation = CSharpLocalizationExtractor.Unwrap(operation);
        IEnumerable<IArgumentOperation> arguments;
        ITypeSymbol? type = null;
        if (operation is IInvocationOperation invocation && invocation.TargetMethod.Name == "Create" && SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, _sourceType))
        {
            arguments = invocation.Arguments;
            if (invocation.TargetMethod.IsGenericMethod)
            {
                type = invocation.TargetMethod.TypeArguments[0];
            }
        }
        else if (operation is IObjectCreationOperation creation && SymbolEqualityComparer.Default.Equals(creation.Type, _sourceType))
        {
            arguments = creation.Arguments;
        }
        else
        {
            return null;
        }

        var value = arguments.FirstOrDefault(argument => argument.Parameter?.Name == "value")?.Value;
        if (value is null)
        {
            return null;
        }

        if (type is not null)
        {
            return new SourceCall(value, type, false, operation.Syntax);
        }

        var resource = arguments.FirstOrDefault(argument => argument.Parameter?.Name == "type")?.Value;
        if (resource is null || resource.ConstantValue is { HasValue: true, Value: null })
        {
            return new SourceCall(value, null, true, operation.Syntax);
        }

        return new SourceCall(value, CSharpLocalizationExtractor.Unwrap(resource) is ITypeOfOperation typeOf ? typeOf.TypeOperand : null, false, operation.Syntax);
    }

    private SourceCall? ResolveSource(IOperation operation, HashSet<ISymbol> visited)
    {
        var source = GetSource(operation);
        if (source is not null)
        {
            return source;
        }

        var symbol = CSharpLocalizationExtractor.GetSymbol(operation);
        if (symbol is null || !visited.Add(symbol) || _csharp.GetAssignedValue(symbol) is not { } value)
        {
            return null;
        }

        return ResolveSource(value, visited);
    }

    private void AddDiagnostic(string code, string message, SyntaxNode node)
    {
        var span = node.GetLocation().GetMappedLineSpan();
        _catalog.Diagnostics.Add(new ExtractionDiagnostic(code, message, _options.GetSource(span.Path, span.StartLinePosition.Line + 1)));
    }

    private static bool IsDeferredPlural(IInvocationOperation invocation)
        => invocation.TargetMethod.Name == "Plural" && invocation.TargetMethod.ContainingType.ToDisplayString() is "Microsoft.Extensions.Localization.StringLocalizerFactoryPluralExtensions" or "Microsoft.AspNetCore.Mvc.Localization.HtmlLocalizerFactoryPluralExtensions";

    private sealed record SourceCall(IOperation Value, ITypeSymbol? Type, bool ContextFree, SyntaxNode Syntax);
}
