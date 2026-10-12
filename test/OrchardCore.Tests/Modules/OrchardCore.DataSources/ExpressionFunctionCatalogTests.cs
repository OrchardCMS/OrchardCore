using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using ExpressionCompiler = OrchardCore.DataSources.Expressions.ExpressionCompiler;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class ExpressionFunctionCatalogTests
{
    private static readonly PassThroughStringLocalizer<ExpressionFunctionCatalogTests> _localizer = new();

    private static readonly DataField[] _fields =
    [
        new("Price", "Price", DataFieldType.Decimal),
        new("Name", "Name", DataFieldType.Text),
    ];

    [Fact]
    public void Functions_BuiltInProvider_ListsEveryBuiltInFunctionByCategoryAndName()
    {
        // Arrange
        var catalog = new ExpressionFunctionCatalog([new BuiltInExpressionFunctionProvider()]);

        // Act
        var functions = catalog.Functions;

        // Assert
        Assert.Equal(ExpressionFunctions.All.Count, functions.Count);
        Assert.Contains(functions, function => function.Name == "UPPER");
        Assert.Contains(functions, function => function.Name == "SUM" && function.IsAggregate);
        Assert.Equal(
            functions.OrderBy(function => function.Category, StringComparer.Ordinal).ThenBy(function => function.Name, StringComparer.Ordinal).Select(function => function.Name),
            functions.Select(function => function.Name));
    }

    [Fact]
    public void Compile_FunctionOfProvider_EvaluatesIt()
    {
        // Arrange
        var catalog = new ExpressionFunctionCatalog([new BuiltInExpressionFunctionProvider(), new TestProvider(Tax())]);

        // Act
        var compiled = ExpressionCompiler.Compile("ROUND(TAX([Price]), 2)", new FieldListExpressionScope(_fields), catalog, _localizer);
        var value = compiled.Evaluate(new ExpressionContext { Row = [100m, "Coffee"] });

        // Assert
        Assert.Equal(DataFieldType.Decimal, compiled.DataType);
        Assert.Equal(15m, Convert.ToDecimal(value));
    }

    [Fact]
    public void Compile_FunctionNotInCatalog_ThrowsUnknownFunction()
    {
        // Arrange
        var catalog = new ExpressionFunctionCatalog([new BuiltInExpressionFunctionProvider()]);

        // Act
        var exception = Assert.Throws<ExpressionException>(() => ExpressionCompiler.Compile("TAX([Price])", new FieldListExpressionScope(_fields), catalog, _localizer));

        // Assert
        Assert.Contains("TAX", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Find_LowerCaseName_FindsFunction()
    {
        // Arrange
        var catalog = new ExpressionFunctionCatalog([new TestProvider(Tax())]);

        // Act
        var function = catalog.Find("tax");

        // Assert
        Assert.NotNull(function);
        Assert.Equal("TAX", function.Name);
    }

    [Fact]
    public void Functions_LaterProviderWithSameName_ReplacesEarlierFunction()
    {
        // Arrange
        var replacement = ExpressionFunction.CreateScalar("UPPER", ExpressionFunctions.TextCategory, "UPPER(text)", "Shouts.", 1, 1, _ => DataFieldType.Text, args => "!");
        var catalog = new ExpressionFunctionCatalog([new BuiltInExpressionFunctionProvider(), new TestProvider(replacement)]);

        // Act
        var compiled = ExpressionCompiler.Compile("UPPER('a')", new FieldListExpressionScope(_fields), catalog, _localizer);

        // Assert
        Assert.Same(replacement, catalog.Find("UPPER"));
        Assert.Equal("!", compiled.Evaluate(new ExpressionContext()));
        Assert.Equal(ExpressionFunctions.All.Count, catalog.Functions.Count);
    }

    [Theory]
    [InlineData("AND")]
    [InlineData("not")]
    [InlineData("TRUE")]
    [InlineData("NULL")]
    [InlineData("MY-FUNCTION")]
    [InlineData("1ST")]
    [InlineData("")]
    public void Functions_InvalidName_Throws(string name)
    {
        // Arrange
        var function = ExpressionFunction.CreateScalar(name, "Custom", "X()", "Invalid.", 0, 0, _ => DataFieldType.Text, _ => null);
        var catalog = new ExpressionFunctionCatalog([new TestProvider(function)]);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => catalog.Functions);
    }

    [Fact]
    public void Functions_ScalarWithoutImplementation_Throws()
    {
        // Arrange
        var function = new ExpressionFunction { Name = "BROKEN", Category = "Custom", Signature = "BROKEN()", ReturnType = _ => DataFieldType.Text };
        var catalog = new ExpressionFunctionCatalog([new TestProvider(function)]);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => catalog.Functions);
    }

    [Fact]
    public void Compile_AggregateFunctionOfProvider_AggregatesTheGroup()
    {
        // Arrange
        var product = ExpressionFunction.CreateAggregate("PRODUCT", "PRODUCT(number)", "Multiplies the values of the group.", true, _ => DataFieldType.Decimal, values => values.Aggregate(1m, (total, value) => total * Convert.ToDecimal(value)));
        var catalog = new ExpressionFunctionCatalog([new TestProvider(product)]);
        var compiled = ExpressionCompiler.Compile("PRODUCT([Price])", new FieldListExpressionScope(_fields), catalog, _localizer);

        // Act
        var value = compiled.Evaluate(new ExpressionContext { Group = [[2m, "a"], [3m, "b"], [4m, "c"]] });

        // Assert
        Assert.True(compiled.IsAggregate);
        Assert.Equal(24m, value);
    }

    [Fact]
    public void AddExpressionFunction_DataSourcesCore_AddsTheFunctionToTheCatalog()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddDataSourcesCore()
            .AddExpressionFunction(Tax())
            .AddExpressionFunctionProvider<TestProvider>()
            .BuildServiceProvider();

        // Act
        var catalog = services.GetRequiredService<IExpressionFunctionCatalog>();

        // Assert
        Assert.NotNull(catalog.Find("TAX"));
        Assert.NotNull(catalog.Find("UPPER"));
        Assert.Equal(ExpressionFunctions.All.Count + 1, catalog.Functions.Count);
    }

    [Fact]
    public void AddExpressionFunction_BeforeDataSourcesCore_StillReplacesTheBuiltInFunction()
    {
        // Arrange
        var replacement = ExpressionFunction.CreateScalar("UPPER", ExpressionFunctions.TextCategory, "UPPER(text)", "Shouts.", 1, 1, _ => DataFieldType.Text, args => "!");
        var services = new ServiceCollection()
            .AddExpressionFunction(replacement)
            .AddDataSourcesCore()
            .BuildServiceProvider();

        // Act
        var function = services.GetRequiredService<IExpressionFunctionCatalog>().Find("UPPER");

        // Assert
        Assert.Same(replacement, function);
    }

    private static ExpressionFunction Tax()
        => ExpressionFunction.CreateScalar(
            "TAX",
            "Finance",
            "TAX(amount)",
            "Returns the sales tax of an amount.",
            1,
            1,
            _ => DataFieldType.Decimal,
            args => args[0] is null ? null : Convert.ToDecimal(args[0]) * 0.15m);

    private sealed class TestProvider : IExpressionFunctionProvider
    {
        private readonly ExpressionFunction[] _functions;

        public TestProvider()
            : this([])
        {
        }

        public TestProvider(params ExpressionFunction[] functions)
        {
            _functions = functions;
        }

        public IEnumerable<ExpressionFunction> GetFunctions() => _functions;
    }
}
