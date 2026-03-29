using Fluid;
using OrchardCore.Queries.Sql;

namespace OrchardCore.Tests.OrchardCore.Queries;

public class SqlLiquidOutputExpressionDetectorTests
{
    [Theory]
    [InlineData("select * from ContentItemIndex where ContentType = '{{ Request.Query.type }}'", true)]
    [InlineData("{% assign type = Request.Query.type %}select * from ContentItemIndex where ContentType = @type", false)]
    [InlineData("{% if Request.Query.type %}select * from ContentItemIndex where ContentType = @type{% endif %}", false)]
    public void ShouldDetectLiquidOutputStatements(string query, bool expectedResult)
    {
        var detector = new SqlLiquidOutputExpressionDetector(new FluidParser());
        var result = detector.ContainsOutputStatement(query);

        Assert.Equal(expectedResult, result);
    }
}
