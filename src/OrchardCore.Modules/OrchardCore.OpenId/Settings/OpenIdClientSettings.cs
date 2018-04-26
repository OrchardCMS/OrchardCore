using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdClientSettings
    {
        public string DisplayName { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
    }
}
