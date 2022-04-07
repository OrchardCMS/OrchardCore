using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        private const string luceneRecipePath = "Areas/TheBlogTheme/Recipes";
        private const string luceneRecipeName = "blog.lucene.query.recipe.json";
        private const string luceneIndexName = "Search";

        public string BlogContentItemId { get; private set; }

        public static IShellHost ShellHost { get; }

        static BlogContext()
        {
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await RunRecipeAsync(ShellHost, luceneRecipeName, luceneRecipePath);
            await ResetLuceneIndiciesAsync(ShellHost, luceneIndexName);

            var result = await GraphQLClient
                .Content
                .Query("blog", builder =>
                {
                    builder
                        .WithField("contentItemId");
                });

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
