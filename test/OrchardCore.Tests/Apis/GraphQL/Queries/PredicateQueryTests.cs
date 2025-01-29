using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Predicates;
using YesSql.Indexes;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;

namespace OrchardCore.Tests.Apis.GraphQL;

public class PredicateQueryTests
{
    private readonly YesSql.IConfiguration _configuration;

    public PredicateQueryTests()
    {
        _configuration = new Configuration()
            .UseSqlServer("Fake database connection string for testing;", "TenantSchema")
            .SetTablePrefix("Tenant1");
    }

    [Theory]
    [InlineData("Value", "[Value]")]
    [InlineData("ListItemIndex.Value", "[ListItemIndex].[Value]")]
    [InlineData("[ListItemIndex.Value]", "[ListItemIndex.Value]")]
    [InlineData("[ListItemIndex].[Value]", "[ListItemIndex].[Value]")]
    public void ShouldReturnQuotedColumnNameWhenAliasNotExists(string propertyPath, string expectedColumnName)
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, []);

        // Act
        var columnName = predicateQuery.GetColumnName(propertyPath);

        // Assert
        Assert.Equal(expectedColumnName, columnName);
    }

    [Theory]
    [InlineData("Path", "Alias", "[Alias]")]
    [InlineData("ListItemIndexPath.ValuePath", "ListItemIndexAlias.ValueAlias", "[ListItemIndexAlias].[ValueAlias]")]
    [InlineData("ListItemIndexPath.ValuePath", "[ListItemIndexAlias.ValueAlias]", "[ListItemIndexAlias.ValueAlias]")]
    [InlineData("ListItemIndexPath.ValuePath", "[ListItemIndexAlias].[ValueAlias]", "[ListItemIndexAlias].[ValueAlias]")]
    public void ShouldReturnQuotedAliasColumnNameWhenAliasExists(string propertyPath, string alias, string expectedColumnName)
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, []);
        predicateQuery.CreateAlias(propertyPath, alias);

        // Act
        var columnName = predicateQuery.GetColumnName(propertyPath);

        // Assert
        Assert.Equal(expectedColumnName, columnName);
    }

    [Theory]
    [InlineData("[ListItemIndex.Value]", "[ListItemIndex.Value]")]
    [InlineData("[ListItemIndex].[Value]", "[ListItemIndex].[Value]")]
    [InlineData("ListItemIndex.[Value]", "[ListItemIndex].[Value]")]
    [InlineData("[ListItemIndex].Value", "[ListItemIndex].[Value]")]
    public void DoesNotQuoteWhenPathIsQuoted(string propertyPath, string expectedColumnName)
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, []);

        // Act
        var columnName = predicateQuery.GetColumnName(propertyPath);

        // Assert
        Assert.Equal(expectedColumnName, columnName);
    }

    [Theory]
    [InlineData("`ListItemIndex.Value`", "`ListItemIndex.Value`", "MySql")]
    [InlineData("[ListItemIndex.Value]", "[ListItemIndex.Value]", "Sqlite")]
    [InlineData("[ListItemIndex.Value]", "[ListItemIndex.Value]", "SqlServer")]
    [InlineData("\"ListItemIndex.Value\"", "\"ListItemIndex.Value\"", "Postgre")]
    public void DetectsProviderDependentQuoteChars(string propertyPath, string expectedColumnName, string dialect)
    {
        // Arrange
        var configuration = new Configuration();

        if (dialect == "MySql")
        {
            configuration
                .UseMySql("Fake database connection string for testing;", "TenantSchema")
                .SetTablePrefix("Tenant1");
        }
        else if (dialect == "Sqlite")
        {
            configuration
                .UseSqLite("Fake database connection string for testing;")
                .SetTablePrefix("Tenant1");
        }
        else if (dialect == "SqlServer")
        {
            configuration
                .UseSqlServer("Fake database connection string for testing;", "TenantSchema")
                .SetTablePrefix("Tenant1");
        }
        else if (dialect == "Postgre")
        {
            configuration
                .UsePostgreSql("Fake database connection string for testing;", "TenantSchema")
                .SetTablePrefix("Tenant1");
        }
        else
        {
            throw new ArgumentException("Unknown dialect", nameof(dialect));
        }

        var predicateQuery = new PredicateQuery(configuration, []);

        // Act
        var columnName = predicateQuery.GetColumnName(propertyPath);

        // Assert
        Assert.Equal(expectedColumnName, columnName);
    }

    [Fact]
    public void ShouldReturnQuotedTableAliasAndColumnNameWhenProviderExists()
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, [new IndexPropertyProvider<ListItemIndex>()]);

        predicateQuery.CreateAlias("ListItemIndexPath", nameof(ListItemIndex));
        predicateQuery.CreateTableAlias(nameof(ListItemIndex), "ListItemIndexAlias");

        // Act
        var columnName = predicateQuery.GetColumnName("ListItemIndexPath.Value");

        // Assert
        Assert.Equal("[ListItemIndexAlias].[Value]", columnName);
    }

    [Fact]
    public void ShouldReturnQuotedTableAliasAndPathWhenProviderNotExists()
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, []);

        predicateQuery.CreateAlias("ListItemIndexPath", nameof(ListItemIndex));
        predicateQuery.CreateTableAlias(nameof(ListItemIndex), "ListItemIndexAlias");

        // Act
        var columnName = predicateQuery.GetColumnName("ListItemIndexPath.Value");

        // Assert
        Assert.Equal("[ListItemIndexAlias].[Value]", columnName);
    }

    public class ListItemIndex : MapIndex
    {
        public string Value { get; set; }
    }
}
