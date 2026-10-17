using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using ExpressionCompiler = OrchardCore.DataSources.Expressions.ExpressionCompiler;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class ExpressionCompilerTests
{
    private static readonly PassThroughStringLocalizer<ExpressionCompilerTests> _localizer = new();

    private static readonly DateTime _now = new(2026, 3, 15, 14, 30, 0);

    [Theory]
    [InlineData("1 + 2 * 3", 7L)]
    [InlineData("(1 + 2) * 3", 9L)]
    [InlineData("10 / 4", 2.5)]
    [InlineData("10 % 4", 2L)]
    [InlineData("-3 + 5", 2L)]
    [InlineData("2.5 * 2", 5.0)]
    [InlineData("ROUND(2.5)", 3L)]
    [InlineData("ROUND(2.345, 2)", 2.35)]
    [InlineData("ABS(-4)", 4L)]
    [InlineData("FLOOR(2.7)", 2L)]
    [InlineData("CEILING(2.1)", 3L)]
    [InlineData("POWER(2, 10)", 1024.0)]
    [InlineData("SQRT(16)", 4.0)]
    [InlineData("MOD(7, 3)", 1L)]
    [InlineData("SIGN(-9)", -1L)]
    [InlineData("LEAST(4, 2, 9)", 2L)]
    [InlineData("GREATEST(4, 2, 9)", 9L)]
    [InlineData("INTEGER('42.9')", 42L)]
    public void Evaluate_Arithmetic_ReturnsExpectedNumber(string formula, object expected)
    {
        // Act
        var value = Evaluate(formula);

        // Assert
        Assert.Equal(System.Convert.ToDecimal(expected), System.Convert.ToDecimal(value));
    }

    [Theory]
    [InlineData("UPPER('abc')", "ABC")]
    [InlineData("LOWER('ABC')", "abc")]
    [InlineData("TRIM('  a b  ')", "a b")]
    [InlineData("PROPER('hello WORLD')", "Hello World")]
    [InlineData("LEFT('abcdef', 2)", "ab")]
    [InlineData("RIGHT('abcdef', 2)", "ef")]
    [InlineData("MID('abcdef', 2, 3)", "bcd")]
    [InlineData("SUBSTRING('abcdef', 4)", "def")]
    [InlineData("REPLACE('a-b-c', '-', '+')", "a+b+c")]
    [InlineData("CONCAT('a', 1, NULL, 'b')", "a1b")]
    [InlineData("'a' & 'b' & 1", "ab1")]
    [InlineData("'Order ' + 5", "Order 5")]
    [InlineData("SPLIT('a,b,c', ',', 2)", "b")]
    [InlineData("TEXT(1234.5, 'N1')", "1,234.5")]
    [InlineData("IF(1 > 2, 'yes', 'no')", "no")]
    [InlineData("IFS(FALSE, 'a', 1 = 1, 'b', 'c')", "b")]
    [InlineData("IFS(FALSE, 'a', FALSE, 'b', 'c')", "c")]
    [InlineData("SWITCH('B', 'A', 'first', 'B', 'second', 'other')", "second")]
    [InlineData("SWITCH('Z', 'A', 'first', 'other')", "other")]
    [InlineData("COALESCE(NULL, '', 'x')", "x")]
    [InlineData("IFNULL(NULL, 'fallback')", "fallback")]
    [InlineData("\"it\"\"s\"", "it\"s")]
    public void Evaluate_TextAndLogic_ReturnsExpectedText(string formula, string expected)
    {
        // Act
        var value = Evaluate(formula);

        // Assert
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("1 = 1", true)]
    [InlineData("1 == 1.0", true)]
    [InlineData("'abc' = 'ABC'", true)]
    [InlineData("1 <> 2", true)]
    [InlineData("1 != 1", false)]
    [InlineData("2 >= 2 AND 3 > 2", true)]
    [InlineData("FALSE OR 1 < 0", false)]
    [InlineData("NOT TRUE", false)]
    [InlineData("!FALSE && TRUE || FALSE", true)]
    [InlineData("NOT(1 = 2)", true)]
    [InlineData("CONTAINS('Hello', 'ELL')", true)]
    [InlineData("STARTSWITH('Hello', 'he')", true)]
    [InlineData("ENDSWITH('Hello', 'LO')", true)]
    [InlineData("IN('b', 'a', 'b')", true)]
    [InlineData("IN('z', 'a', 'b')", false)]
    [InlineData("ISNULL(NULL)", true)]
    [InlineData("ISBLANK('  ')", true)]
    [InlineData("NULL = NULL", true)]
    [InlineData("NULL > 1", false)]
    public void Evaluate_Comparisons_ReturnExpectedBoolean(string formula, bool expected)
    {
        // Act
        var value = Evaluate(formula);

        // Assert
        Assert.Equal(expected, value);
    }

    [Fact]
    public void Evaluate_DateFunctions_ReturnExpectedParts()
    {
        // Act & Assert
        Assert.Equal(2026L, Evaluate("YEAR(NOW())"));
        Assert.Equal(1L, Evaluate("QUARTER(TODAY())"));
        Assert.Equal(3L, Evaluate("MONTH('2026-03-15')"));
        Assert.Equal(15L, Evaluate("DAY(TODAY())"));
        Assert.Equal(14L, Evaluate("HOUR(NOW())"));
        Assert.Equal(30L, Evaluate("MINUTE(NOW())"));
        Assert.Equal(7L, Evaluate("WEEKDAY(DATE(2026, 3, 15))"));
        Assert.Equal(11L, Evaluate("WEEK(DATE(2026, 3, 15))"));
        Assert.Equal(new DateTime(2026, 4, 15, 14, 30, 0), Evaluate("DATEADD('month', 1, NOW())"));
        Assert.Equal(new DateTime(2026, 1, 1), Evaluate("DATETRUNC('quarter', TODAY())"));
        Assert.Equal(new DateTime(2026, 3, 9), Evaluate("DATETRUNC('week', TODAY())"));
        Assert.Equal(14L, Evaluate("DATEDIFF('day', DATE(2026, 3, 1), TODAY())"));
        Assert.Equal(2L, Evaluate("DATEDIFF('month', '2026-01-31', '2026-03-01')"));
        Assert.Equal(new DateTime(2026, 3, 20), Evaluate("TODAY() + 5"));
        Assert.Equal(5m, Evaluate("DATE(2026, 3, 20) - DATE(2026, 3, 15)"));
        Assert.Null(Evaluate("DATE(2026, 2, 30)"));
    }

    [Theory]
    [InlineData("1 / 0")]
    [InlineData("MOD(1, 0)")]
    [InlineData("1 + NULL")]
    [InlineData("UPPER(NULL)")]
    [InlineData("SQRT(-1)")]
    [InlineData("LEFT(NULL, 2)")]
    public void Evaluate_InvalidOrMissingInput_ReturnsNullInsteadOfFailing(string formula)
    {
        // Act
        var value = Evaluate(formula);

        // Assert
        Assert.Null(value);
    }

    [Theory]
    [InlineData("", "empty")]
    [InlineData("1 +", "ends unexpectedly")]
    [InlineData("(1 + 2", "Expected ')'")]
    [InlineData("[Missing]", "does not exist")]
    [InlineData("FOO(1)", "does not exist")]
    [InlineData("LEFT('a')", "is written")]
    [InlineData("Name", "Unknown name")]
    [InlineData("'unterminated", "no closing quote")]
    [InlineData("[open", "no closing ']'")]
    [InlineData("1 # 2", "Unexpected character")]
    [InlineData("'a' - 1", "cannot combine")]
    [InlineData("SUM([Name])", "needs a number")]
    [InlineData("SUM([Price]) + [Price]", "mixes aggregated values")]
    [InlineData("SUM(SUM([Price]))", "cannot be used inside another aggregate")]
    public void Compile_InvalidFormula_ThrowsHelpfulError(string formula, string expectedMessage)
    {
        // Act
        var exception = Assert.Throws<ExpressionException>(() => Compile(formula));

        // Assert
        Assert.Contains(expectedMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Compile_DeeplyNestedFormula_IsRejectedInsteadOfOverflowingTheStack()
    {
        // Arrange
        var formula = new string('(', 500) + "1" + new string(')', 500);

        // Act
        var exception = Assert.Throws<ExpressionException>(() => Compile(formula));

        // Assert
        Assert.Contains("nested too deeply", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Compile_InfersResultTypes()
    {
        // Act & Assert
        Assert.Equal(DataFieldType.Integer, Compile("[Quantity] * 2").DataType);
        Assert.Equal(DataFieldType.Decimal, Compile("[Price] * [Quantity]").DataType);
        Assert.Equal(DataFieldType.Decimal, Compile("[Quantity] / 2").DataType);
        Assert.Equal(DataFieldType.Text, Compile("[Name] + '!'").DataType);
        Assert.Equal(DataFieldType.Boolean, Compile("[Price] > 10").DataType);
        Assert.Equal(DataFieldType.Date, Compile("TODAY()").DataType);
        Assert.Equal(DataFieldType.Integer, Compile("SUM([Quantity])").DataType);
        Assert.Equal(DataFieldType.Decimal, Compile("AVG([Quantity])").DataType);
        Assert.Equal(DataFieldType.Text, Compile("IF([Price] > 10, 'High', 'Low')").DataType);
        Assert.Equal(DataFieldType.Decimal, Compile("IF([Price] > 10, [Price], 0)").DataType);
    }

    [Fact]
    public void Compile_RowFormula_ReadsTheCurrentRow()
    {
        // Arrange
        var compiled = Compile("[Price] * [Quantity]");
        var context = new ExpressionContext
        {
            Row = ["Widget", 2.5m, 4L],
        };

        // Act
        var value = compiled.Evaluate(context);

        // Assert
        Assert.False(compiled.IsAggregate);
        Assert.Equal(10m, value);
        Assert.Equal(new[] { "Price", "Quantity" }, compiled.References.Order());
    }

    [Fact]
    public void Compile_AggregateFormula_ReadsTheWholeGroup()
    {
        // Arrange
        var compiled = Compile("SUM([Price] * [Quantity]) / COUNT()");
        var context = new ExpressionContext
        {
            Group =
            [
                ["A", 2m, 1L],
                ["B", 3m, 2L],
                ["C", null, 4L],
            ],
        };

        // Act
        var value = compiled.Evaluate(context);

        // Assert
        Assert.True(compiled.IsAggregate);
        Assert.Equal(8m / 3, value);
    }

    [Fact]
    public void Compile_AggregateFunctions_IgnoreMissingValues()
    {
        // Arrange
        var context = new ExpressionContext
        {
            Group =
            [
                ["A", 2m, 1L],
                ["B", null, 1L],
                ["A", 4m, 5L],
            ],
        };

        // Act & Assert
        Assert.Equal(3m, Compile("AVG([Price])").Evaluate(context));
        Assert.Equal(2L, Compile("COUNT([Price])").Evaluate(context));
        Assert.Equal(3L, Compile("COUNT()").Evaluate(context));
        Assert.Equal(2L, Compile("COUNTD([Name])").Evaluate(context));
        Assert.Equal(2m, Compile("MIN([Price])").Evaluate(context));
        Assert.Equal(5L, Compile("MAX([Quantity])").Evaluate(context));
        Assert.Equal(1m, Compile("MEDIAN([Quantity])").Evaluate(context));
        Assert.Equal(7L, Compile("SUM([Quantity])").Evaluate(context));
    }

    // A conditional expression whose branches are long and decimal silently converted whole results to decimals,
    // so SUM of whole numbers came back as 7m instead of 7L and formatted like a decimal.
    [Theory]
    [InlineData("ABS(-4)")]
    [InlineData("ROUND(2.5)")]
    [InlineData("YEAR(TODAY())")]
    [InlineData("[Quantity] * 2")]
    public void Evaluate_WholeNumberResults_StayWholeNumbers(string formula)
    {
        // Act
        var value = Evaluate(formula);

        // Assert
        Assert.IsType<long>(value);
    }

    [Fact]
    public void Compile_IntegerOverflow_ContinuesInDecimals()
    {
        // Act
        var value = Evaluate("9223372036854775807 + 1");

        // Assert
        Assert.Equal(9223372036854775808m, value);
    }

    private static object Evaluate(string formula)
    {
        return Compile(formula).Evaluate(new ExpressionContext
        {
            Now = _now,
            Row = ["Widget", 2.5m, 4L],
        });
    }

    private static CompiledExpression Compile(string formula)
    {
        return ExpressionCompiler.Compile(formula, new TestScope(), _localizer);
    }

    private sealed class TestScope : IExpressionScope
    {
        private static readonly Dictionary<string, (DataFieldType Type, int Index)> _fields = new(StringComparer.Ordinal)
        {
            ["Name"] = (DataFieldType.Text, 0),
            ["Price"] = (DataFieldType.Decimal, 1),
            ["Quantity"] = (DataFieldType.Integer, 2),
        };

        public bool TryResolve(string key, out ExpressionFieldBinding binding)
        {
            if (!_fields.TryGetValue(key, out var field))
            {
                binding = null;

                return false;
            }

            binding = new ExpressionFieldBinding
            {
                Key = key,
                DataType = field.Type,
                Index = field.Index,
            };

            return true;
        }
    }
}
