using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using ExpressionCompiler = OrchardCore.DataSources.Expressions.ExpressionCompiler;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class DataSourceManagerTests
{
    private static readonly DataField[] _fields = [new("Id", "Id", DataFieldType.Integer)];

    [Fact]
    public void GetDataSources_SeveralSources_OrdersByDisplayName()
    {
        // Arrange
        var manager = new DataSourceManager([new InMemoryDataSource("b", "Zebra"), new InMemoryDataSource("a", "Apple")]);

        // Act
        var sources = manager.GetDataSources();

        // Assert
        Assert.Equal(["a", "b"], sources.Select(source => source.Name));
    }

    [Fact]
    public void GetDataSource_DifferentCase_FindsSource()
    {
        // Arrange
        var manager = new DataSourceManager([new InMemoryDataSource("Contents")]);

        // Act & Assert
        Assert.NotNull(manager.GetDataSource("contents"));
        Assert.Null(manager.GetDataSource("missing"));
        Assert.Null(manager.GetDataSource(null));
    }

    [Fact]
    public void Constructor_DuplicateNames_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new DataSourceManager([new InMemoryDataSource("x"), new InMemoryDataSource("X")]));
    }

    [Fact]
    public async Task ReadRowsAsync_MoreRowsThanLimit_ReturnsLimitAndTruncated()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("Numbers", _fields, [1L], [2L], [3L]);

        // Act
        var result = await source.ReadRowsAsync(new DataSourceQuery { DataSet = "Numbers", BatchSize = 1 }, 2, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Truncated);
        Assert.Equal([1L, 2L], result.Rows.Select(row => row[0]));
        Assert.Equal(3, source.Queries.Single().MaxRows);
    }

    [Fact]
    public async Task ReadRowsAsync_FewerRowsThanLimit_IsNotTruncated()
    {
        // Arrange
        var source = new InMemoryDataSource().Add("Numbers", _fields, [1L]);

        // Act
        var result = await source.ReadRowsAsync(new DataSourceQuery { DataSet = "Numbers" }, 5, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.Truncated);
        Assert.Single(result.Rows);
        Assert.Equal("Id", Assert.Single(result.Fields).Name);
    }

    [Fact]
    public void Evaluate_TextFunctionsUnderTurkishCulture_UseInvariantCasing()
    {
        // Arrange
        var culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("tr-TR");

        try
        {
            var expression = ExpressionCompiler.Compile("UPPER('title')", new FieldListExpressionScope([]), new PassThroughStringLocalizer<DataSourceManagerTests>());

            // Act
            var value = expression.Evaluate(new ExpressionContext());

            // Assert
            Assert.Equal("TITLE", value);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public void Compile_FieldListScope_ResolvesFieldsByNameAndPosition()
    {
        // Arrange
        DataField[] fields = [new("First name", "First name", DataFieldType.Text), new("Age", "Age", DataFieldType.Integer)];
        var scope = new FieldListExpressionScope(fields);

        // Act
        var expression = ExpressionCompiler.Compile("[First name] & ' (' & ([Age] + 1) & ')'", scope, new PassThroughStringLocalizer<DataSourceManagerTests>());
        var value = expression.Evaluate(new ExpressionContext { Row = ["Ada", 36L] });

        // Assert
        Assert.Equal(DataFieldType.Text, expression.DataType);
        Assert.Equal("Ada (37)", value);
        Assert.Equal(["Age", "First name"], expression.References.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Compile_UnknownField_ThrowsExpressionException()
    {
        // Arrange
        var scope = new FieldListExpressionScope([new DataField("Age", "Age", DataFieldType.Integer)]);

        // Act & Assert
        Assert.Throws<ExpressionException>(() => ExpressionCompiler.Compile("[Missing] + 1", scope, new PassThroughStringLocalizer<DataSourceManagerTests>()));
    }
}
