using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OrchardCore.Localization.Extraction;

public sealed class DataAnnotationsLocalizationExtractor
{
    private const string Context = "OrchardCore.Localization.DataAnnotations.DataAnnotationsDefaultErrorMessages";
    private readonly Compilation _compilation;
    private readonly ExtractionOptions _options;
    private readonly LocalizationCatalog _catalog;
    private readonly INamedTypeSymbol? _validationAttribute;
    private readonly INamedTypeSymbol? _displayAttribute;
    private readonly INamedTypeSymbol? _skipAttribute;

    public DataAnnotationsLocalizationExtractor(Compilation compilation, ExtractionOptions options, LocalizationCatalog catalog)
    {
        _compilation = compilation;
        _options = options;
        _catalog = catalog;
        _validationAttribute = compilation.GetTypeByMetadataName("System.ComponentModel.DataAnnotations.ValidationAttribute");
        _displayAttribute = compilation.GetTypeByMetadataName("System.ComponentModel.DataAnnotations.DisplayAttribute");
        _skipAttribute = compilation.GetTypeByMetadataName("OrchardCore.Localization.SkipLocalizationExtractionAttribute");
    }

    public void Extract(SyntaxTree tree, CancellationToken cancellationToken = default)
    {
        var model = _compilation.GetSemanticModel(tree);
        foreach (var syntax in tree.GetRoot(cancellationToken).DescendantNodes().OfType<AttributeSyntax>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (model.GetSymbolInfo(syntax, cancellationToken).Symbol is not IMethodSymbol constructor || !IsValidationAttribute(constructor.ContainingType))
            {
                continue;
            }

            var owner = GetOwner(syntax, model, cancellationToken);
            if (ShouldSkip(owner))
            {
                continue;
            }

            var argument = syntax.ArgumentList?.Arguments.FirstOrDefault(argument => argument.NameEquals?.Name.Identifier.ValueText == "ErrorMessage");
            if (argument is null)
            {
                continue;
            }

            var constant = model.GetConstantValue(argument.Expression, cancellationToken);
            if (constant is not { HasValue: true, Value: string message })
            {
                continue;
            }

            var span = argument.GetLocation().GetMappedLineSpan();
            var source = _options.GetSource(span.Path, span.StartLinePosition.Line + 1);
            _catalog.Add(Context, message, null, source);

            var display = owner?.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _displayAttribute));
            var name = display?.NamedArguments.FirstOrDefault(pair => pair.Key == "Name").Value.Value as string ?? owner?.Name;
            if (!string.IsNullOrEmpty(name))
            {
                var fallback = message.Replace(name, "{0}", StringComparison.Ordinal);
                if (fallback != message)
                {
                    _catalog.Add(Context, fallback, null, source);
                }
            }
        }
    }

    private bool IsValidationAttribute(INamedTypeSymbol? type)
    {
        if (_validationAttribute is null)
        {
            return false;
        }

        for (; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type, _validationAttribute))
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldSkip(ISymbol? symbol)
    {
        if (_skipAttribute is null)
        {
            return false;
        }

        for (; symbol is not null; symbol = symbol.ContainingSymbol)
        {
            if (symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, _skipAttribute)))
            {
                return true;
            }
        }

        return false;
    }

    private static ISymbol? GetOwner(AttributeSyntax syntax, SemanticModel model, CancellationToken cancellationToken)
    {
        var declaration = syntax.Parent?.Parent;
        if (declaration is FieldDeclarationSyntax field)
        {
            return model.GetDeclaredSymbol(field.Declaration.Variables[0], cancellationToken);
        }

        return declaration is null ? null : model.GetDeclaredSymbol(declaration, cancellationToken);
    }
}
