using OrchardCore.Security.Services;

namespace OrchardCore.Twitter.Settings
{
    public class TwitterSettings : OAuthSettings
    {
        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string AccessToken { get; set; }

        public string AccessTokenSecret { get; set; }
    }
}
