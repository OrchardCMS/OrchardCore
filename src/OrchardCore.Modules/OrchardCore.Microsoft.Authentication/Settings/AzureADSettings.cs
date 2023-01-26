using Microsoft.AspNetCore.Http;
using OrchardCore.Security;

namespace OrchardCore.Microsoft.Authentication.Settings
{
    public class AzureADSettings : OAuthSettings
    {
        public string DisplayName { get; set; }

        public string AppId { get; set; }

        public string TenantId { get; set; }

        public PathString CallbackPath { get; set; }

        public bool SaveTokens { get; set; }
    }
}
