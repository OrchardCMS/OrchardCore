using OrchardCore.Tests.Apis.GraphQL.Context;

namespace OrchardCore.Tests.Apis.GraphQL.Blog
{
    public class BlogContext : SiteContext
    {
        public BlogContext() : base()
        {
            BlogContentItemId = Client
                .Content
                .CreateAsync("Blog", builder =>
                {
                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Hi There!");
                })
                .GetAwaiter()
                .GetResult();
        }

        public string BlogContentItemId { get; }
    }
}
