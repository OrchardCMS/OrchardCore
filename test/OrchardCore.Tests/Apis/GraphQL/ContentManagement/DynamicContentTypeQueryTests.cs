namespace OrchardCore.Tests.Apis.GraphQL;

public class DynamicContentTypeQueryTests
{
    [Fact]
    public async Task ShouldQueryContentFields()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query(@"product(where: {price: {amount_gt: 10}}) {
                        contentItemId
                        displayText
                        price {
                            amount
                        }
                        sku
                        metadataCode
                        metadataAvailabilityDate
                    }");

        Assert.Single(result["data"]["product"].AsArray());

        Assert.Equal(context.Product2ContentItemId, result["data"]["product"].AsArray().First()["contentItemId"].ToString());
    }

    [Fact]
    public async Task ShouldQueryCollapsedContentFields()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query(@"product(where: {sku_starts_with: ""4000""}) {
                        contentItemId
                        displayText
                        price {
                            amount
                        }
                        sku
                        metadataCode
                        metadataAvailabilityDate
                    }");

        Assert.Single(result["data"]["product"].AsArray());

        Assert.Equal(context.Product2ContentItemId, result["data"]["product"].AsArray().First()["contentItemId"].ToString());
    }

    [Fact]
    public async Task ShouldQueryCollapsedContentFieldsWithPreventCollision()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query(@"product(where: {metadataCode: ""100000987""}) {
                        contentItemId
                        displayText
                        price {
                            amount
                        }
                        sku
                        metadataCode
                        metadataAvailabilityDate
                    }");

        Assert.Single(result["data"]["product"].AsArray());

        Assert.Equal(context.Product1ContentItemId, result["data"]["product"].AsArray().First()["contentItemId"].ToString());
    }

    [Fact]
    public async Task ShouldQueryMultipleContentFields()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query(@"product(
                        where: {
                            AND: {
                                OR: {price: {amount: 10}, sku_ends_with: ""44""},
                                metadataAvailabilityDate_gt: ""2024-05-01""
                            }
                        }, 
                        orderBy: {createdUtc: ASC}
                    ) {
                        contentItemId
                        displayText
                        price {
                            amount
                        }
                        sku
                        metadataCode
                        metadataAvailabilityDate
                    }");

        Assert.Single(result["data"]["product"].AsArray());

        Assert.Equal(context.Product2ContentItemId, result["data"]["product"].AsArray().First()["contentItemId"].ToString());
    }

    [Fact]
    public async Task ShouldOrderByCreatedUtc()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var resultAsc = await context
            .GraphQLClient
            .Content
            .Query(@"product(orderBy: {createdUtc: ASC}) {
                        contentItemId
                        displayText
                        createdUtc
                    }");

        var resultDesc = await context
            .GraphQLClient
            .Content
            .Query(@"product(orderBy: {createdUtc: DESC}) {
                        contentItemId
                        displayText
                        createdUtc
                    }");

        Assert.Equal(new DateTime(2024, 04, 07, 0, 0, 0, DateTimeKind.Utc), resultAsc["data"]["product"].AsArray().First()["createdUtc"].GetValue<DateTime>());
        Assert.Equal(new DateTime(2024, 05, 18, 0, 0, 0, DateTimeKind.Utc), resultDesc["data"]["product"].AsArray().First()["createdUtc"].GetValue<DateTime>());
    }

    [Fact]
    public async Task ShouldQuerySimilarNamedContentFields()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var numberResult = await context
            .GraphQLClient
            .Content
            .Query(@"numberType(where: {value: 123}) {
                        contentItemId
                        displayText
                        value
                    }");

        var stringResult = await context
            .GraphQLClient
            .Content
            .Query(@"stringType(where: {value: ""Text123""}) {
                        contentItemId
                        displayText
                        value
                    }");

        Assert.Single(numberResult["data"]["numberType"].AsArray());
        Assert.Equal("123", numberResult["data"]["numberType"].AsArray().First()["value"].ToString());

        Assert.Single(stringResult["data"]["stringType"].AsArray());
        Assert.Equal("Text123", stringResult["data"]["stringType"].AsArray().First()["value"].ToString());
    }

    [Fact]
    public async Task ShouldDistinquishMultipleNamedContentFields()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        // Make sure values of field2 are not included by querying field1
        var result = await context
            .GraphQLClient
            .Content
            .Query(@"twoNumbersType(where: {field1: 200}) {
                        contentItemId
                        displayText
                        field1
                        field2
                    }");

        Assert.Empty(result["data"]["twoNumbersType"].AsArray());
    }
}
