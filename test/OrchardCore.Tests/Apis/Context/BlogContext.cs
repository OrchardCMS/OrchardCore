using System.Threading.Tasks;

namespace OrchardCore.Tests.Apis.Context
{
    public class BlogContext : SiteContext
    {
        private static bool _initializing;
        private static object _sync = new object();

        public static string BlogContentItemId { get; private set; }

        static BlogContext()
        {
        }

        public async Task InitializeBlogAsync()
        {
            await InitializeSiteAsync();

            var initialize = false;

            if (BlogContentItemId == null && !_initializing)
            {
                lock (_sync)
                {
                    if (BlogContentItemId == null && !_initializing)
                    {
                        initialize = true;
                        _initializing = true;
                    }
                }
            }

            if (initialize)
            {
                var result = await GraphQLClient
                .Content
                .Query("Blog", builder =>
                {
                    builder
                        .AddField("contentItemId");
                });

                BlogContentItemId = result["data"]["blog"].First["contentItemId"].ToString();
            }

            while (BlogContentItemId == null)
            {
                await Task.Delay(1 * 1000);
            }
        }
    }
}
