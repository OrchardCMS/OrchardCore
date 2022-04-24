using System;

namespace OrchardCore.Security.Options
{
    public class SandboxContentSecurityPolicyOptions : ContentSecurityPolicyOptionsBase
    {
        internal static readonly string Separator = " ";

        public override string Name => ContentSecurityPolicyValue.Sandbox;

        public string Value { get; set; }
        //public bool AllowDownloads { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowModals { get; set; }

        //public bool AllowOrientationLock { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowForms { get; set; }

        //public bool AllowForms { get; set; }

        public override string ToString() => String.IsNullOrEmpty(Value)
            ? Name
            : Name + Separator + Value.TrimStart();
    }
}
