using Microsoft.Extensions.PlatformAbstractions;
using Orchard.Localization;

namespace Orchard.Hosting
{
    public class ConsoleHostEnvironment : HostEnvironment
    {
        public ConsoleHostEnvironment(IHostingEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}