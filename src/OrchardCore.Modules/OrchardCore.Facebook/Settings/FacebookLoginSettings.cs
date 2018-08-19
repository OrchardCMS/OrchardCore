using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Facebook.Settings
{
    public class FacebookLoginSettings : FacebookCoreSettings
    {
        public string CallbackPath { get; set; }
    }
}
