using Microsoft.AspNetCore.Http;

namespace OrchardCore.Twitter.Signin.Settings
{
    public class TwitterSigninSettings
    {
        public PathString CallbackPath { get; set; }

        public bool SaveTokens { get; set; }
    }
}
