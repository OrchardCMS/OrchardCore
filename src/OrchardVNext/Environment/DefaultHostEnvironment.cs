using OrchardVNext.Localization;
using Microsoft.Framework.Runtime;
using System;

namespace OrchardVNext.Environment {
    public class DefaultHostEnvironment : HostEnvironment {
        private const string WebConfigPath = "~/web.config";
        private const string RefreshHtmlPath = "~/refresh.html";
        private const string HostRestartPath = "~/bin/HostRestart";

        public DefaultHostEnvironment(
            IApplicationEnvironment applicationEnvrionment) : base(applicationEnvrionment) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void RestartAppDomain() {
            Logger.Error("TODO: Restart App Domain");
        }
    }
}