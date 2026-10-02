using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;

namespace OrchardCore.Localization.Extraction;

public sealed class LiquidLocalizationExtractor
{
    private readonly FluidParser _parser;
    private readonly HashSet<string> _filters;

    public LiquidLocalizationExtractor(FluidParser? parser = null, IEnumerable<string>? localizationFilters = null)
    {
        _parser = parser ?? new LiquidViewParser(Options.Create(new LiquidViewOptions()), Options.Create(new FluidParserOptions { AllowFunctions = true }));
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
