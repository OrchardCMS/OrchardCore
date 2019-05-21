using Microsoft.AspNetCore.Http;

namespace OrchardCore.SpaServices.Settings
{
    public class SpaServicesSettings
    {
        public bool UseStaticFile { get; set; }
        public string StaticFile { get; set; }
    }
}
