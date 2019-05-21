using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.SpaServices.ViewModels
{
    public class SpaServicesSettingsViewModel
    {
        public bool IsHomepage{ get; set; }

        public bool SetHomepage { get; set; }
        public bool UseStaticFile { get; set; }
        [FileExtensions(Extensions = "html,htm", ErrorMessage = "Please use a valid extension")]
        public string StaticFile { get; set; }
    }
}
