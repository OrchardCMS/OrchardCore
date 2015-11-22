using Microsoft.Extensions.PlatformAbstractions;
using Orchard.Localization;
namespace Orchard.Hosting
{
    public class WebHostEnvironment : HostEnvironment
    {
        public WebHostEnvironment(
            IApplicationEnvironment applicationEnvironment) : base(applicationEnvironment)
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}