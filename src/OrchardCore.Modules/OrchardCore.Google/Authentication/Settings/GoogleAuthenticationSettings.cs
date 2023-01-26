using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Services;

namespace OrchardCore.Google.Authentication.Settings
{
    public class GoogleAuthenticationSettings : SocialAuthenticationSettings
    {
        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public PathString CallbackPath { get; set; }

        public bool SaveTokens { get; set; }
    }
}
