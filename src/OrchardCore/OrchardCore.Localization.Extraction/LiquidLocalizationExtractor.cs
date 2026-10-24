using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace OrchardCore.Localization.Extraction;

public sealed class LiquidLocalizationExtractor
{
    private readonly FluidParser _parser;
    private readonly HashSet<string> _filters;

    public LiquidLocalizationExtractor(FluidParser? parser = null, IEnumerable<string>? localizationFilters = null)
    {
        _parser = parser ?? CreateDefaultParser();
        _filters = new HashSet<string>(localizationFilters ?? ["t"], StringComparer.Ordinal);
    }

    public void Extract(string content, ExtractionFile file, ExtractionOptions options, LocalizationCatalog catalog, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var source = options.GetSource(file.Path);
        if (!_parser.TryParse(content, out var template, out var error))
        {
            catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC007", "Liquid parsing failed. Supply a parser adapter for custom syntax: " + error, source, true));
            return;
        }

        new LocalizationVisitor(options.GetViewContext(file.LogicalPath), source, catalog, _filters, cancellationToken).VisitTemplate(template);
    }

    private static ExtractionLiquidParser CreateDefaultParser()
        => new ExtractionLiquidParser();

    private static ValueTask<Completion> IgnoreTagAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
    {
        _ = writer;
        _ = encoder;
        _ = context;
        return ValueTask.FromResult(Completion.Normal);
    }

    private static ValueTask<Completion> IgnoreExpressionTagAsync(Expression expression, TextWriter writer, TextEncoder encoder, TemplateContext context)
    {
        _ = expression;
        _ = writer;
        _ = encoder;
        _ = context;
        return ValueTask.FromResult(Completion.Normal);
    }

    private static ValueTask<Completion> IgnoreParserTagAsync<T>(T arguments, TextWriter writer, TextEncoder encoder, TemplateContext context)
    {
        _ = arguments;
        _ = writer;
        _ = encoder;
        _ = context;
        return ValueTask.FromResult(Completion.Normal);
    }

    private static ValueTask<Completion> IgnoreParserBlockAsync(IReadOnlyList<FilterArgument> arguments, IReadOnlyList<Statement> statements, TextWriter writer, TextEncoder encoder, TemplateContext context)
    {
        _ = arguments;
        _ = statements;
        _ = writer;
        _ = encoder;
        _ = context;
        return ValueTask.FromResult(Completion.Normal);
    }

    private sealed class ExtractionLiquidParser : FluidParser
    {
        public ExtractionLiquidParser()
            : base(new FluidParserOptions { AllowFunctions = true })
        {
            RegisterEmptyTag("render_body", IgnoreTagAsync);
            RegisterEmptyTag("antiforgerytoken", IgnoreTagAsync);
            RegisterExpressionTag("layout", IgnoreExpressionTagAsync);
            RegisterExpressionTag("shape_clear_alternates", IgnoreExpressionTagAsync);
            RegisterExpressionTag("shape_clear_wrappers", IgnoreExpressionTagAsync);
            RegisterExpressionTag("shape_clear_classes", IgnoreExpressionTagAsync);
            RegisterExpressionTag("shape_clear_attributes", IgnoreExpressionTagAsync);

            foreach (var tag in new[]
            {
                "render_section", "page_title", "page_title_add_segment", "httpcontext_add_items",
                "meta", "link", "script", "style", "resources", "helper", "shape", "cache_dependency", "cache_expires_on", "cache_expires_after",
                "cache_expires_sliding", "shape_add_properties", "shape_remove_property", "shape_remove_item",
                "shape_pager",
            })
            {
                RegisterParserTag(tag, ArgumentsList, IgnoreParserTagAsync);
            }

            RegisterParserTag("shape_add_alternates", Primary.And(Primary), IgnoreParserTagAsync);
            RegisterParserTag("shape_add_wrappers", Primary.And(Primary), IgnoreParserTagAsync);
            RegisterParserTag("shape_add_classes", Primary.And(Primary), IgnoreParserTagAsync);
            RegisterParserTag("shape_add_attributes", Primary.And(ArgumentsList), IgnoreParserTagAsync);
            RegisterParserTag("shape_type", Primary.And(Primary), IgnoreParserTagAsync);
            RegisterParserTag("shape_display_type", Primary.And(Primary), IgnoreParserTagAsync);
            RegisterParserTag("shape_position", Primary.And(Primary), IgnoreParserTagAsync);
            RegisterParserTag("httpcontext_remove_items", Primary, IgnoreParserTagAsync);

            foreach (var tag in new[] { "zone", "form", "cache", "a", "block" })
            {
                RegisterParserBlock(tag, ArgumentsList, IgnoreParserBlockAsync);
            }
        }
    }

    private sealed class LocalizationVisitor : AstVisitor
    {
        private readonly string _context;
        private readonly SourceReference _source;
        private readonly LocalizationCatalog _catalog;
        private readonly HashSet<string> _filters;
        private readonly CancellationToken _cancellationToken;

        public LocalizationVisitor(string context, SourceReference source, LocalizationCatalog catalog, HashSet<string> filters, CancellationToken cancellationToken)
        {
            _context = context;
            _source = source;
            _catalog = catalog;
            _filters = filters;
            _cancellationToken = cancellationToken;
        }

        protected override Expression VisitFilterExpression(FilterExpression expression)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (_filters.Contains(expression.Name))
            {
                if (expression.Input is LiteralExpression { Value: StringValue value })
                {
                    _catalog.Add(_context, value.ToStringValue(), null, _source);
                }
                else
                {
                    _catalog.Diagnostics.Add(new ExtractionDiagnostic("OCLOC001", "The Liquid localization input is dynamic or transformed before the localization filter.", _source));
                }
            }

            return base.VisitFilterExpression(expression);
        }

        protected override Statement VisitParserTagStatement<T>(Fluid.Parser.ParserTagStatement<T> statement)
        {
            VisitArguments(statement.Value);
            return base.VisitParserTagStatement(statement);
        }

        protected override Statement VisitParserBlockStatement<T>(Fluid.Parser.ParserBlockStatement<T> statement)
        {
            VisitArguments(statement.Value);
            return base.VisitParserBlockStatement(statement);
        }

        public override Statement VisitOtherStatement(Statement statement)
        {
            if (statement is TagStatement tag)
            {
                foreach (var child in tag.Statements)
                {
                    Visit(child);
                }

                return statement;
            }

            return base.VisitOtherStatement(statement);
        }

        private void VisitArguments(object? value)
        {
            switch (value)
            {
                case Expression expression:
                    Visit(expression);
                    break;
                case IReadOnlyList<FilterArgument> arguments:
                    foreach (var argument in arguments)
                    {
                        Visit(argument.Expression);
                    }

                    break;
                case ValueTuple<Expression, Expression> pair:
                    Visit(pair.Item1);
                    Visit(pair.Item2);
                    break;
                case ValueTuple<Expression, IReadOnlyList<FilterArgument>> pair:
                    Visit(pair.Item1);
                    VisitArguments(pair.Item2);
                    break;
            }
        }
    }
}
