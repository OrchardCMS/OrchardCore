using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Security
{
    public class SecurityHeadersBuilder
    {
        private readonly SecuritySettings _settings;

        public SecurityHeadersBuilder(SecuritySettings settings)
            => _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        public SecurityHeadersBuilder AddContentTypeOptions(string option)
        {
            if (String.IsNullOrEmpty(option))
            {
                throw new ArgumentException($"'{nameof(option)}' cannot be null or empty.", nameof(option));
            }

            _settings.ContentTypeOptions = option;

            return this;
        }

        public SecurityHeadersBuilder AddFrameOptions(string option)
        {
            if (String.IsNullOrEmpty(option))
            {
                throw new ArgumentException($"'{nameof(option)}' cannot be null or empty.", nameof(option));
            }

            _settings.FrameOptions = option;

            return this;
        }

        public SecurityHeadersBuilder AddPermissionsPolicy(IEnumerable<string> policyOptions)
        {
            if (policyOptions is null)
            {
                throw new ArgumentNullException(nameof(policyOptions));
            }

            _settings.PermissionsPolicy = policyOptions.ToList();

            return this;
        }

        public SecurityHeadersBuilder AddReferrerPolicy(string policyOption)
        {
            if (String.IsNullOrEmpty(policyOption))
            {
                throw new ArgumentException($"'{nameof(policyOption)}' cannot be null or empty.", nameof(policyOption));
            }

            _settings.ReferrerPolicy = policyOption;

            return this;
        }

        internal SecurityHeadersBuilder AddPermissionsPolicy(string policyOption)
        {
            if (String.IsNullOrEmpty(policyOption))
            {
                throw new ArgumentException($"'{nameof(policyOption)}' cannot be null or empty.", nameof(policyOption));
            }

            _settings.PermissionsPolicy.Add(policyOption);

            return this;
        }

        public SecuritySettings Build() => _settings;
    }
}
