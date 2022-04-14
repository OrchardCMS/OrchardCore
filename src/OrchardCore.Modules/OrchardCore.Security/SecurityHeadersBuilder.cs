using System;

namespace OrchardCore.Security
{
    public class SecurityHeadersBuilder
    {
        private readonly SecuritySettings _settings;

        public SecurityHeadersBuilder(SecuritySettings settings)
            => _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public SecurityHeadersBuilder AddReferrerPolicy(string policyOption)
        {
            if (String.IsNullOrEmpty(policyOption))
            {
                throw new ArgumentException($"'{nameof(policyOption)}' cannot be null or empty.", nameof(policyOption));
            }

            _settings.ReferrerPolicy = policyOption;

            return this;
        }

        public SecuritySettings Build() => _settings;
    }
}
