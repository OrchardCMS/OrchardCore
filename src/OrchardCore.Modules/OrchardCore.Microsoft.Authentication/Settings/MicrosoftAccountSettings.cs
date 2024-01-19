using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.Microsoft.Authentication.Settings
{
    public class MicrosoftAccountSettings : IAsyncOptions
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public PathString CallbackPath { get; set; }
        public bool SaveTokens { get; set; }
    }
}
