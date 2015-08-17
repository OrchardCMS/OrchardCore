using Microsoft.Dnx.Runtime;
using OrchardVNext.Abstractions.Localization;

namespace OrchardVNext.Hosting {
    public class WebHostEnvironment : HostEnvironment {
        public WebHostEnvironment(
            IApplicationEnvironment applicationEnvironment) : base(applicationEnvironment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}