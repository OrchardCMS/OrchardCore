using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Services;

namespace OrchardCore.Microsoft.Authentication.Settings
{
    public class MicrosoftAccountSettings : SocialAuthenticationSettings
    {
        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public PathString CallbackPath { get; set; }

        public bool SaveTokens { get; set; }
    }
}
