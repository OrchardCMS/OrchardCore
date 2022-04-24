using System;

namespace OrchardCore.Security.Options
{
    public abstract class SourceContentSecurityPolicyOptionsBase : ContentSecurityPolicyOptionsBase
    {
        public string Source { get; set; } = ContentSecurityPolicySourceValue.None;

        public string[] AllowedSources { get; set; } = Array.Empty<string>();

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Source))
            {
                return Name;
            }

            return AllowedSources.Length == 0
                ? $"{Name} {Source}"
                : $"{Name} {Source} {String.Join(' ', AllowedSources)}";
        }
    }
}
