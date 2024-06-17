using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.GraphQL;

public class DynamicContentTypeContext : SiteContext
{
    public string Product1ContentItemId { get; private set; }
    public string Product2ContentItemId { get; private set; }

    public DynamicContentTypeContext()
    {
        this.WithRecipe("DynamicContentTypeQueryTest");
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var products = await GraphQLClient
            .Content
            .Query("product", builder =>
            {
                builder
                    .WithQueryArgument("orderBy", "createdUtc", "ASC")
                    .WithField("contentItemId")
                    .WithField("displayText");
            });

        Product1ContentItemId = products["data"]["product"][0]["contentItemId"].ToString();
        Product2ContentItemId = products["data"]["product"][1]["contentItemId"].ToString();
    }
}
