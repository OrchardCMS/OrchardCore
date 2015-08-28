using Microsoft.Dnx.Runtime;
using Orchard.Abstractions.Localization;

namespace Orchard.Hosting {
    public class ConsoleHostEnvironment : HostEnvironment {
        public ConsoleHostEnvironment(
            IApplicationEnvironment applicationEnvironment) : base(applicationEnvironment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}