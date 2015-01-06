using OrchardVNext.Localization;
using Microsoft.Framework.Runtime;
using System;

namespace OrchardVNext.Environment {
    public class DefaultHostEnvironment : HostEnvironment {
        public DefaultHostEnvironment(
            IApplicationEnvironment applicationEnvrionment) : base(applicationEnvrionment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}