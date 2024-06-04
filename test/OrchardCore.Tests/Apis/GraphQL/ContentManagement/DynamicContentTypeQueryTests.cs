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
            .Query("product(where: {price: {price_gt: 10}}) {contentItemId, displayText}");

        Assert.Single(
            result["data"]["product"].AsArray().Where(node => node["contentItemId"].ToString() == context.Product2ContentItemId));
    }

    [Fact]
    public async Task ShouldQueryCollapsedContentFields()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query("product(where: {sku_starts_with: \"4000\"}) {contentItemId, displayText}");

        Assert.Single(
            result["data"]["product"].AsArray().Where(node => node["contentItemId"].ToString() == context.Product2ContentItemId));
    }

    [Fact]
    public async Task ShouldQueryCollapsedContentFieldsWithPreventCollision()
    {
        using var context = new DynamicContentTypeContext();
        await context.InitializeAsync();

        var result = await context
            .GraphQLClient
            .Content
            .Query("product(where: {productCodeCode: \"100000987\"}) {contentItemId, displayText}");

        Assert.Single(
            result["data"]["product"].AsArray().Where(node => node["contentItemId"].ToString() == context.Product1ContentItemId));
    }
}