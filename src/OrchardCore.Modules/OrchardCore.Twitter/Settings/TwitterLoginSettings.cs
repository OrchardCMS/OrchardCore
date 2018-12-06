using Microsoft.AspNetCore.Http;

namespace OrchardCore.Twitter.Settings
{
    public class TwitterLoginSettings
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public PathString CallbackPath { get; set; }
    }
}
