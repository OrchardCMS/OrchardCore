using Microsoft.AspNetCore.Http;

namespace OrchardCore.Twitter.Settings;

public class TwitterAuthenticationSettings
{
    public string ConsumerKey { get; set; }

    public string ConsumerSecret { get; set; }

    public string AccessToken { get; set; }

    public string AccessTokenSecret { get; set; }

    public PathString CallbackPath { get; set; }

    public bool SaveTokens { get; set; }
}
