using Microsoft.Framework.Runtime;
using OrchardVNext.Localization;

namespace OrchardVNext.Environment {
    public class DefaultHostEnvironment : HostEnvironment {
        public DefaultHostEnvironment(
            IApplicationEnvironment applicationEnvironment) : base(applicationEnvironment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}