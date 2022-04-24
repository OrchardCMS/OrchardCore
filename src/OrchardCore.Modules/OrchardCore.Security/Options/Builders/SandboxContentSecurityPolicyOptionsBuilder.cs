namespace OrchardCore.Security.Options.Builders
{
    public class SandboxContentSecurityPolicyOptionsBuilder
    {
        private readonly SandboxContentSecurityPolicyOptions _options;

        public SandboxContentSecurityPolicyOptionsBuilder(SandboxContentSecurityPolicyOptions options)
            => _options = options;

        public SandboxContentSecurityPolicyOptionsBuilder AllowDownloads()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowDownloads;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowForms()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowForms;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowModals()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowModals;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowOrientationLock()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowOrientationLock;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowPointerLock()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowPointerLock;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowPopup()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowPopup;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowPopupsToEscapeSandbox()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowPopupsToEscapeSandbox;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowPresentation()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowPresentation;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowSameOrigin()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowSameOrigin;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowScripts()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowScripts;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowsStorageAccessByUserActivation()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowsStorageAccessByUserActivation;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowTopNavigation()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowTopNavigation;

            return this;
        }

        public SandboxContentSecurityPolicyOptionsBuilder AllowTopNavigationByUseActivation()
        {
            _options.Value += SandboxContentSecurityPolicyOptions.Separator + SandboxContentSecurityPolicyValue.AllowTopNavigationByUseActivation;

            return this;
        }
    }
}
