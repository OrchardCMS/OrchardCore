using Microsoft.Extensions.Options;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using OrchardCore.Localization;
using ExpressionCompiler = OrchardCore.DataSources.Expressions.ExpressionCompiler;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class DataSourceManagerTests
{
    private static readonly DataField[] _fields = [new("Id", "Id", DataFieldType.Integer)];

    [Fact]
    public void GetDataSources_SeveralSources_ListsThemWithoutCreatingThem()
    {
        // Arrange
        var created = 0;
        var manager = CreateManager(
            services => services.AddScoped(_ =>
            {
                created++;

                return new ZebraSource();
            }),
            DataSourceDescriptor.Create<ZebraSource>(),
            DataSourceDescriptor.Create<AppleSource>());

        // Act
        var sources = manager.GetDataSources();

        // Assert
        Assert.Equal(["b", "a"], sources.Select(source => source.Name));
        Assert.Equal(["Zebra", "Apple"], sources.Select(source => source.DisplayName.Value));
        Assert.Equal(0, created);
    }

    [Fact]
    public void GetDataSource_DifferentCase_FindsSource()
    {
        // Arrange
        var manager = CreateManager(services => services.AddScoped<AppleSource>(), DataSourceDescriptor.Create<AppleSource>());

        // Act & Assert
        Assert.IsType<AppleSource>(manager.GetDataSource("A"));
        Assert.Equal("a", manager.GetDescriptor("A").Name);
        Assert.Null(manager.GetDataSource("missing"));
        Assert.Null(manager.GetDataSource(null));
    }

    [Fact]
    public void Constructor_DuplicateNames_Throws()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => CreateManager(_ => { }, DataSourceDescriptor.Create<LowerXSource>(), DataSourceDescriptor.Create<UpperXSource>()));
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

    private static DataSourceManager CreateManager(Action<IServiceCollection> configure, params DataSourceDescriptor[] descriptors)
    {
        var services = new ServiceCollection();
        configure(services);

        var options = new DataSourceOptions();

        foreach (var descriptor in descriptors)
        {
            options.Add(descriptor);
        }

        return new DataSourceManager(Options.Create(options), services.BuildServiceProvider());
    }

    [Fact]
    public void Compile_UnknownField_ThrowsExpressionException()
    {
        // Arrange
        var scope = new FieldListExpressionScope([new DataField("Age", "Age", DataFieldType.Integer)]);

        // Act & Assert
        Assert.Throws<ExpressionException>(() => ExpressionCompiler.Compile("[Missing] + 1", scope, new PassThroughStringLocalizer<DataSourceManagerTests>()));
    }

    private abstract class EmptySource
    {
        public virtual Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<DataSetDescriptor>>([]);
        }

        public virtual Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DataSetSchema>(null);
        }

        public virtual IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<DataBatch>();
        }
    }

    private sealed class AppleSource : EmptySource, IDataSource
    {
        public static string Name => "a";

        public static LocalizationSource DisplayName { get; } = new("Apple");

        public static LocalizationSource Description { get; } = new(string.Empty);
    }

    private sealed class ZebraSource : EmptySource, IDataSource
    {
        public static string Name => "b";

        public static LocalizationSource DisplayName { get; } = new("Zebra");

        public static LocalizationSource Description { get; } = new(string.Empty);
    }

    private sealed class LowerXSource : EmptySource, IDataSource
    {
        public static string Name => "x";

        public static LocalizationSource DisplayName { get; } = new("x");

        public static LocalizationSource Description { get; } = new(string.Empty);
    }

    private sealed class UpperXSource : EmptySource, IDataSource
    {
        public static string Name => "X";

        public static LocalizationSource DisplayName { get; } = new("X");

        public static LocalizationSource Description { get; } = new(string.Empty);
    }
}
