using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Services;

namespace OrchardCore.GitHub.Settings
{
    public class GitHubAuthenticationSettings : OAuthSettings
    {
        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public PathString CallbackPath { get; set; }

        public bool SaveTokens { get; set; }
    }
}
