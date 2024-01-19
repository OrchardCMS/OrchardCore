using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.Google.Authentication.Settings
{
    public class GoogleAuthenticationSettings : IAsyncOptions
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public PathString CallbackPath { get; set; }
        public bool SaveTokens { get; set; }
    }
}
