using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Microsoft.Authentication.Settings
{
    public class AzureADSettings
    {
        public string AppId { get; set; }
        public string TenantId { get; set; }
        public string Domain { get; set; }
        public string AppSecret { get; set; }
        public PathString CallbackPath { get; set; }
    }
}
