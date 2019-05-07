using Microsoft.AspNetCore.Http;

namespace OrchardCore.Github.Settings
{
    public class GithubAuthenticationSettings
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public PathString CallbackPath { get; set; }
    }
}
