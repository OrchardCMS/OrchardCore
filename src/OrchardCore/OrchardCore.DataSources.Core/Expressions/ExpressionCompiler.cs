using Microsoft.Extensions.Localization;

namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Compiles a formula into a delegate after checking it against a scope: every field must exist, every function
/// must be known and called with the right number of arguments, operators must suit their operand types, and a formula
/// must not mix aggregated values with row-level values or nest one aggregate inside another.
/// </summary>
public sealed class ExpressionCompiler
{
    private readonly IExpressionScope _scope;
    private readonly IStringLocalizer S;
    private readonly HashSet<string> _references = new(StringComparer.Ordinal);

    private ExpressionCompiler(IExpressionScope scope, IStringLocalizer localizer)
    {
        _scope = scope;
        S = localizer;
    }

    private enum Level
    {
        Constant,
        Row,
        Aggregate,
    }

    /// <summary>
    /// Parses and compiles a formula.
    /// </summary>
    /// <param name="formula">The formula.</param>
    /// <param name="scope">The fields the formula may refer to.</param>
    /// <param name="localizer">The localizer used for error messages.</param>
    /// <returns>The compiled formula.</returns>
    /// <exception cref="ExpressionException">The formula is malformed or does not make sense in the scope.</exception>
    public static CompiledExpression Compile(string formula, IExpressionScope scope, IStringLocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(localizer);

        var root = ExpressionParser.Parse(formula, localizer);
        var compiler = new ExpressionCompiler(scope, localizer);
        var compiled = compiler.CompileNode(root, insideAggregate: false);

        return new CompiledExpression(
            compiled.DataType,
            compiled.Level == Level.Aggregate,
            compiler._references,
            compiled.Evaluate);
    }

    private Node CompileNode(ExpressionNode node, bool insideAggregate)
    {
        return node switch
        {
            LiteralNode literal => CompileLiteral(literal),
            FieldNode field => CompileField(field, insideAggregate),
            UnaryNode unary => CompileUnary(unary, insideAggregate),
            BinaryNode binary => CompileBinary(binary, insideAggregate),
            FunctionNode function => CompileFunction(function, insideAggregate),
            _ => throw new ExpressionException(S["The formula contains an unsupported element."], node.Position),
        };
    }

    private static Node CompileLiteral(LiteralNode literal)
    {
        var value = literal.Value;
        var dataType = value is null ? DataFieldType.Text : DataValues.InferType(value);

        return new Node(dataType, Level.Constant, _ => value, value is null);
    }

    private Node CompileField(FieldNode field, bool insideAggregate)
    {
        if (!_scope.TryResolve(field.Key, out var binding))
        {
            throw new ExpressionException(S["The field [{0}] does not exist.", field.Key], field.Position);
        }

        _references.Add(binding.Key);

        if (binding.References is not null)
        {
            _references.UnionWith(binding.References);
        }

        if (binding.Aggregate is not null)
        {
            if (insideAggregate)
            {
                throw new ExpressionException(S["The field [{0}] is already aggregated and cannot be aggregated again.", field.Key], field.Position);
            }

            var aggregate = binding.Aggregate.Evaluate;

            return new Node(binding.DataType, Level.Aggregate, aggregate);
        }

        var index = binding.Index;

        return new Node(binding.DataType, Level.Row, context => context.Row?[index]);
    }

    private Node CompileUnary(UnaryNode unary, bool insideAggregate)
    {
        var operand = CompileNode(unary.Operand, insideAggregate);
        var evaluate = operand.Evaluate;

        if (unary.Operator == "NOT")
        {
            return new Node(DataFieldType.Boolean, operand.Level, context => !ExpressionOperations.IsTrue(evaluate(context)));
        }

        if (!operand.IsNull && !DataValues.IsNumeric(operand.DataType))
        {
            throw new ExpressionException(S["The operator '{0}' needs a number.", unary.Operator], unary.Position);
        }

        return unary.Operator == "-"
            ? new Node(operand.DataType, operand.Level, context => ExpressionOperations.Negate(evaluate(context)))
            : operand;
    }

    private Node CompileBinary(BinaryNode binary, bool insideAggregate)
    {
        var left = CompileNode(binary.Left, insideAggregate);
        var right = CompileNode(binary.Right, insideAggregate);
        var level = Combine(left.Level, right.Level, binary.Position);
        var dataType = InferBinaryType(binary, left, right);
        var op = binary.Operator;
        var evaluateLeft = left.Evaluate;
        var evaluateRight = right.Evaluate;

        return op switch
        {
            "AND" => new Node(dataType, level, context => ExpressionOperations.IsTrue(evaluateLeft(context)) && ExpressionOperations.IsTrue(evaluateRight(context))),
            "OR" => new Node(dataType, level, context => ExpressionOperations.IsTrue(evaluateLeft(context)) || ExpressionOperations.IsTrue(evaluateRight(context))),
            _ => new Node(dataType, level, context => ExpressionOperations.Apply(op, evaluateLeft(context), evaluateRight(context))),
        };
    }

    private DataFieldType InferBinaryType(BinaryNode binary, Node left, Node right)
    {
        switch (binary.Operator)
        {
            case "&":
                return DataFieldType.Text;

            case "=" or "!=" or "<" or "<=" or ">" or ">=" or "AND" or "OR":
                return DataFieldType.Boolean;
        }

        if (left.IsNull || right.IsNull)
        {
            return left.IsNull ? right.DataType : left.DataType;
        }

        var leftType = left.DataType;
        var rightType = right.DataType;

        if (binary.Operator == "+" && (leftType == DataFieldType.Text || rightType == DataFieldType.Text))
        {
            return DataFieldType.Text;
        }

        if (binary.Operator is "+" or "-")
        {
            if (DataValues.IsTemporal(leftType) && DataValues.IsNumeric(rightType))
            {
                return leftType;
            }

            if (binary.Operator == "+" && DataValues.IsNumeric(leftType) && DataValues.IsTemporal(rightType))
            {
                return rightType;
            }

            if (binary.Operator == "-" && DataValues.IsTemporal(leftType) && DataValues.IsTemporal(rightType))
            {
                return DataFieldType.Decimal;
            }
        }

        if (!DataValues.IsNumeric(leftType) || !DataValues.IsNumeric(rightType))
        {
            throw new ExpressionException(S["The operator '{0}' cannot combine {1} and {2} values.", binary.Operator, leftType, rightType], binary.Position);
        }

        if (binary.Operator == "/")
        {
            return DataFieldType.Decimal;
        }

        return leftType == DataFieldType.Integer && rightType == DataFieldType.Integer
            ? DataFieldType.Integer
            : DataFieldType.Decimal;
    }

    private Node CompileFunction(FunctionNode call, bool insideAggregate)
    {
        var function = ExpressionFunctions.Find(call.Name) ??
            throw new ExpressionException(S["The function {0} does not exist.", call.Name], call.Position);

        if (call.Arguments.Count < function.MinArguments || call.Arguments.Count > function.MaxArguments)
        {
            throw new ExpressionException(S["The function {0} is written {1}.", function.Name, function.Signature], call.Position);
        }

        if (function.IsAggregate)
        {
            return CompileAggregate(call, function, insideAggregate);
        }

        var arguments = call.Arguments.Select(argument => CompileNode(argument, insideAggregate)).ToArray();
        var level = Level.Constant;

        foreach (var argument in arguments)
        {
            level = Combine(level, argument.Level, call.Position);
        }

        var dataType = function.ReturnType(arguments.Select(argument => argument.DataType).ToArray());
        var evaluators = arguments.Select(argument => argument.Evaluate).ToArray();
        var invoke = function.Invoke;

        return new Node(dataType, level, context =>
        {
            var values = new object[evaluators.Length];

            for (var index = 0; index < evaluators.Length; index++)
            {
                values[index] = evaluators[index](context);
            }

            return invoke(values, context);
        });
    }

    private Node CompileAggregate(FunctionNode call, ExpressionFunction function, bool insideAggregate)
    {
        if (insideAggregate)
        {
            throw new ExpressionException(S["The function {0} cannot be used inside another aggregate function.", function.Name], call.Position);
        }

        Func<ExpressionContext, object> evaluateArgument = _ => true;
        var argumentTypes = Array.Empty<DataFieldType>();

        if (call.Arguments.Count == 1)
        {
            var argument = CompileNode(call.Arguments[0], insideAggregate: true);

            if (function.RequiresNumericArgument && !argument.IsNull && !DataValues.IsNumeric(argument.DataType))
            {
                throw new ExpressionException(S["The function {0} needs a number, but receives a {1} value.", function.Name, argument.DataType], call.Position);
            }

            evaluateArgument = argument.Evaluate;
            argumentTypes = [argument.DataType];
        }

        var aggregate = function.Aggregate;

        return new Node(function.ReturnType(argumentTypes), Level.Aggregate, context =>
        {
            var rows = context.Group ?? (context.Row is null ? [] : [context.Row]);
            var saved = context.Row;
            var values = new object[rows.Count];

            try
            {
                for (var index = 0; index < rows.Count; index++)
                {
                    context.Row = rows[index];
                    values[index] = evaluateArgument(context);
                }
            }
            finally
            {
                context.Row = saved;
            }

            return aggregate(values);
        });
    }

    private Level Combine(Level left, Level right, int position)
    {
        if ((left == Level.Row && right == Level.Aggregate) || (left == Level.Aggregate && right == Level.Row))
        {
            throw new ExpressionException(S["The formula mixes aggregated values (such as SUM) with row-level fields. Wrap each field in an aggregate function."], position);
        }

        return (Level)Math.Max((int)left, (int)right);
    }

    private sealed record Node(DataFieldType DataType, Level Level, Func<ExpressionContext, object> Evaluate, bool IsNull = false);
}
