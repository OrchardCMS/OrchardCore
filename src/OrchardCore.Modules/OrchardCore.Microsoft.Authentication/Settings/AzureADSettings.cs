using Microsoft.AspNetCore.Http;

namespace OrchardCore.Microsoft.Authentication.Settings
{
    public class AzureADSettings
    {
        public string DisplayName { get; set; }
        public string AppId { get; set; }
        public string TenantId { get; set; }
        public PathString CallbackPath { get; set; }
        public bool SaveTokens { get; set; }
    }
}
