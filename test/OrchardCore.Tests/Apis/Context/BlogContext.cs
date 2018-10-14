using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        private static Task _initialize;
        public static string BlogContentItemId { get; private set; }
        public static Task InitializeBlogAsync() => _initialize;

        static BlogContext()
        {
            _initialize = InitializeAsync();
        }

        private static async Task InitializeAsync()
        {
            await InitializeSiteAsync();

            var result = await GraphQLClient
            .Content
            .Query("blog", builder =>
            {
                builder
                    .AddField("contentItemId");
            });

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
