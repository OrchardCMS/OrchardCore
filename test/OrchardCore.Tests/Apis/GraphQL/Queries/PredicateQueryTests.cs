using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Predicates;
using YesSql.Indexes;

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

    [Fact]
    public void ShouldReturnQuotedColumnNameWhenAliasNotExists()
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, []);

        // Act
        var columnName = predicateQuery.GetColumnName("ListItemIndex.Value");

        // Assert
        Assert.Equal("[ListItemIndex.Value]", columnName);
    }

    [Fact]
    public void ShouldReturnQuotedAliasColumnNameWhenAliasExists()
    {
        // Arrange
        var predicateQuery = new PredicateQuery(_configuration, []);
        predicateQuery.CreateAlias("ListItemIndexPath.ValuePath", "ListItemIndexAlias.ValueAlias");

        // Act
        var columnName = predicateQuery.GetColumnName("ListItemIndexPath.ValuePath");

        // Assert
        Assert.Equal("[ListItemIndexAlias.ValueAlias]", columnName);
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
