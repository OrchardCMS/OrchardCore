using Microsoft.AspNetCore.Http;

namespace OrchardCore.Google.Settings
{
    public class GoogleAuthenticationSettings
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public PathString CallbackPath { get; set; }
    }
}
