using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.GitHub.Settings
{
    public class GitHubAuthenticationSettings : IAsyncOptions
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public PathString CallbackPath { get; set; }
        public bool SaveTokens { get; set; }
    }
}
