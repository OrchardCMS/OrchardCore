using Microsoft.AspNetCore.Http;

namespace OrchardCore.GitHub.Settings
{
    public class GitHubAuthenticationSettings
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public PathString CallbackPath { get; set; }
        public bool SaveTokens { get; set; }
    }
}
