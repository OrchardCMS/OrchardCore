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
}
