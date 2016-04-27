using Microsoft.AspNetCore.Hosting; 
using Microsoft.Extensions.PlatformAbstractions;
using Orchard.Localization;

namespace Orchard.Hosting
{
    public class WebHostEnvironment : HostEnvironment
    {
        public WebHostEnvironment(IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}