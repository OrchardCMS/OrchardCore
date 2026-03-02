namespace OrchardCore.Tests.Apis.Context;

public class BlogContext : SiteContext
{
    public const string luceneRecipeName = "Blog.Lucene.Query";
    public const string luceneIndexName = "Search";

    public string BlogContentItemId { get; private set; }

    static BlogContext()
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await RunRecipeAsync(luceneRecipeName);
        // await ResetLuceneIndexesAsync(luceneIndexName);

        var result = await GraphQLClient
            .Content
            .Query("blog", builder =>
            {
                builder
                    .WithField("contentItemId");
            });

        BlogContentItemId = result["data"]["blog"][0]["contentItemId"].ToString();
    }
}
