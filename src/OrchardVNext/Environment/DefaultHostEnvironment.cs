using OrchardVNext.Localization;
using Microsoft.Framework.Runtime;
using System;

namespace OrchardVNext.Environment {
    public class DefaultHostEnvironment : HostEnvironment {
        private const string RefreshHtmlPath = "~/refresh.html";
        private const string HostRestartPath = "~/bin/HostRestart";

        public DefaultHostEnvironment(
            IApplicationEnvironment applicationEnvrionment) : base(applicationEnvrionment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}