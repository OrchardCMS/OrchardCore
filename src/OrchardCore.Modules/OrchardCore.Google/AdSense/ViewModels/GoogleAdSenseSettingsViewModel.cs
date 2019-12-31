using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.Google.AdSense.ViewModels
{
    public class GoogleAdSenseSettingsViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string PublisherID { get; set; }
    }
}