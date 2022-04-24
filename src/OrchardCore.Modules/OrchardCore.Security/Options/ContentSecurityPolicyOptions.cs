using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Security.Options
{
    public class ContentSecurityPolicyOptions
    {
        internal static readonly string Separator = ", ";

        public SourceContentSecurityPolicyOptionsBase BaseUri { get; set; } = new BaseUriContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase ChildSource { get; set; } = new ChildSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase ConnectSource { get; set; } = new ConnectSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase DefaultSource { get; set; } = new DefaultSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase FontSource { get; set; } = new FontSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase FormAction { get; set; } = new FormActionContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase FrameSource { get; set; } = new FrameSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase FrameAncestors { get; set; } = new FrameAncestorsContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase ImageSource { get; set; } = new ImageSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase ManifestSource { get; set; } = new ManifestSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase MediaSource { get; set; } = new MediaSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase ObjectSource { get; set; } = new ObjectSourceContentSecurityPolicyOptions();

        public ContentSecurityPolicyOptionsBase ReportUri { get; set; } = new ReportUriContentSecurityPolicyOptions();

        public SandboxContentSecurityPolicyOptions Sandbox { get; set; } = new SandboxContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase ScriptSource { get; set; } = new ScriptSourceContentSecurityPolicyOptions();

        public SourceContentSecurityPolicyOptionsBase StyleSource { get; set; } = new StyleSourceContentSecurityPolicyOptions();

        public ContentSecurityPolicyOptionsBase UpgradeInsecureRequests { get; set; } = new UpgradeInsecureRequestsContentSecurityPolicyOptions();

        public override string ToString()
        {
            var options = new List<ContentSecurityPolicyOptionsBase>
            {
                BaseUri,
                ChildSource,
                ConnectSource,
                DefaultSource,
                FontSource,
                FormAction,
                FrameAncestors,
                FrameSource,
                ImageSource,
                MediaSource,
                ManifestSource,
                ObjectSource,
                Sandbox,
                ScriptSource,
                StyleSource,
                UpgradeInsecureRequests
            };

            //var sourceOptionsValues = sourceOptions.Select(o => o.Source == ContentSecurityPolicyOriginValue.None
            //    ? $"{o.Name} {o.Source}"
            //    : $"{o.Name} {o.Source} {String.Join(' ', o.AllowedSources)}");

            //var nonSourceOptionsValues = nonSourceOptions.Select(o => String.IsNullOrEmpty(o.Value)
            //    ? o.Name
            //    : $"{o.Name} {o.Value}");

            return String.Join(Separator, options.Select(o => o.ToString()));
        }
    }
}
