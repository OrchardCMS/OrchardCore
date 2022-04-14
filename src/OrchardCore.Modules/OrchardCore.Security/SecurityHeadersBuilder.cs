using System;

namespace OrchardCore.Security
{
    public class SecurityHeadersBuilder
    {
        private readonly SecurityHeadersOptions _options;

        private string _referrerPolicy;

        public SecurityHeadersBuilder(SecurityHeadersOptions options)
        {
            _options = options;
        }

        public SecurityHeadersBuilder AddReferrerPolicy(string policyOption)
        {
            if (String.IsNullOrEmpty(policyOption))
            {
                throw new ArgumentException($"'{nameof(policyOption)}' cannot be null or empty.", nameof(policyOption));
            }

            _referrerPolicy = policyOption;

            return this;
        }

        public SecurityHeadersOptions Build()
        {
            _options.ReferrerPolicy = _referrerPolicy;

            return _options;
        }
    }
}
