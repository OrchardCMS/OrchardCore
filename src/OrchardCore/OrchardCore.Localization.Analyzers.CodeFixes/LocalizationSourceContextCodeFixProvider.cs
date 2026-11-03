using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace OrchardCore.Localization.Analyzers;

/// <summary>
/// Declares the type containing a context-free <c>LocalizationSource</c> as its context, with
/// <c>LocalizationSource.Create&lt;T&gt;(value)</c>, or <c>LocalizationSource.Create(value, typeof(T))</c> when the
/// declaring type is static and cannot be a type argument.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LocalizationSourceContextCodeFixProvider))]
[Shared]
public sealed class LocalizationSourceContextCodeFixProvider : CodeFixProvider
{
    private const string LocalizationSourceMetadataName = "OrchardCore.Localization.LocalizationSource";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds => [LocalizationSourceAnalyzer.MissingContextDiagnosticId];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null || semanticModel is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var expression = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ExpressionSyntax>(node => node is InvocationExpressionSyntax or BaseObjectCreationExpressionSyntax);
        if (expression is null)
        {
            return;
        }

        var declaringType = GetDeclaringType(semanticModel.GetEnclosingSymbol(expression.SpanStart, context.CancellationToken));
        var sourceType = semanticModel.Compilation.GetTypeByMetadataName(LocalizationSourceMetadataName);
        if (declaringType is null || sourceType is null)
        {
            return;
        }

        var typeName = declaringType.ToMinimalDisplayString(semanticModel, expression.SpanStart);
        var title = declaringType.IsStatic
            ? $"Use typeof({typeName}) as the localization context"
            : $"Use '{typeName}' as the localization context";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                cancellationToken => Task.FromResult(Apply(context.Document, root, semanticModel, expression, declaringType, sourceType, typeName)),
                equivalenceKey: nameof(LocalizationSourceContextCodeFixProvider)),
            diagnostic);
    }

    private static Document Apply(Document document, SyntaxNode root, SemanticModel semanticModel, ExpressionSyntax expression, INamedTypeSymbol declaringType, INamedTypeSymbol sourceType, string typeName)
    {
        var valueArgument = GetValueArgument(expression);
        if (valueArgument is null)
        {
            return document;
        }

        var receiver = GetReceiver(expression, semanticModel, sourceType);
        var typeSyntax = SyntaxFactory.ParseTypeName(typeName);
        var value = SyntaxFactory.Argument(valueArgument.Expression.WithoutTrivia());

        InvocationExpressionSyntax replacement;
        if (declaringType.IsStatic)
        {
            replacement = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiver, SyntaxFactory.IdentifierName("Create")),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([value, SyntaxFactory.Argument(SyntaxFactory.TypeOfExpression(typeSyntax))])));
        }
        else
        {
            var name = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Create"), SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(typeSyntax)));
            replacement = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, receiver, name),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(value)));
        }

        replacement = replacement.WithTriviaFrom(expression).WithAdditionalAnnotations(Formatter.Annotation);

        return document.WithSyntaxRoot(root.ReplaceNode(expression, replacement));
    }

    private static ArgumentSyntax? GetValueArgument(ExpressionSyntax expression)
    {
        var arguments = expression switch
        {
            InvocationExpressionSyntax invocation => invocation.ArgumentList.Arguments,
            BaseObjectCreationExpressionSyntax creation => creation.ArgumentList?.Arguments ?? default,
            _ => default,
        };

        return arguments.FirstOrDefault(argument => argument.NameColon?.Name.Identifier.ValueText == "value")
            ?? arguments.FirstOrDefault(argument => argument.NameColon is null);
    }

    private static ExpressionSyntax GetReceiver(ExpressionSyntax expression, SemanticModel semanticModel, INamedTypeSymbol sourceType)
    {
        switch (expression)
        {
            case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }:
                return memberAccess.Expression.WithoutTrivia();

            case ObjectCreationExpressionSyntax creation:
                return SyntaxFactory.ParseExpression(creation.Type.WithoutTrivia().ToString());

            default:
                // Implicit creation, or Create imported statically: name the type as briefly as the location allows.
                return SyntaxFactory.ParseExpression(sourceType.ToMinimalDisplayString(semanticModel, expression.SpanStart));
        }
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
}
