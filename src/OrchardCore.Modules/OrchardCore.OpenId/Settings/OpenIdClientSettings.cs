using System;
using System.Collections.Generic;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdClientSettings
    {
        public string DisplayName { get; set; }
        public Uri Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string CallbackPath { get; set; }
        public string SignedOutRedirectUri { get; set; }
        public string SignedOutCallbackPath { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
        public bool StoreExternalTokens { get; set; }
        public ParameterSetting[] Parameters { get; set; } = Array.Empty<ParameterSetting>();
    }

    public class ParameterSetting
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
