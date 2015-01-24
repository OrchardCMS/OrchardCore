using Microsoft.AspNet.Hosting;
using OrchardVNext.Localization;

namespace OrchardVNext.Environment {
    public class DefaultHostEnvironment : HostEnvironment {
        public DefaultHostEnvironment(
            IHostingEnvironment hostingEnvrionment) : base(hostingEnvrionment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}