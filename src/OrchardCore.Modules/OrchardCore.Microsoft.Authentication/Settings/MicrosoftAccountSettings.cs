using Microsoft.AspNetCore.Http;

namespace OrchardCore.Microsoft.Authentication.Settings
{
    public class MicrosoftAccountSettings
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public PathString CallbackPath { get; set; }
        public bool SaveTokens { get; set; }
    }
}
