using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        public static string BlogContentItemId { get; private set; }
        private static Task _initialize;

        static BlogContext()
        {
            _initialize = InitializeAsync();
        }

        public static Task InitializeBlogAsync() => _initialize;

        private static async Task InitializeAsync()
        {
            await InitializeSiteAsync();

            var result = await GraphQLClient
            .Content
            .Query("Blog", builder =>
            {
                builder
                    .AddField("contentItemId");
            });

            BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
        }
    }
}
