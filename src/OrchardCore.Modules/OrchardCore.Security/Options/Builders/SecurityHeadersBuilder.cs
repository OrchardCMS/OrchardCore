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

        public SecurityHeadersBuilder AddContentTypeOptions()
        {
            _settings.ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

            return this;
        }

        public FrameOptionsHeaderBuilder AddFrameOptions() => new(this);

        public SecurityHeadersBuilder AddFrameOptions(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
            }

            _settings.FrameOptions = value;

            return this;
        }

        public SecurityHeadersBuilder AddPermissionsPolicy(ICollection<string> policies)
        {
            if (policies is null)
            {
                throw new ArgumentNullException(nameof(policies));
            }

            _settings.PermissionsPolicy = policies;

            return this;
        }

        public SecurityHeadersBuilder AddPermissionsPolicy(PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _settings.PermissionsPolicy = options.Values.Select(v => v.ToString()).ToList();
            _settings.PermissionsPolicyOrigin = options.Origin;

            return this;
        }

        public ReferrerPolicyHeaderBuilder AddReferrerPolicy() => new(this);

        public SecurityHeadersBuilder AddReferrerPolicy(string policy)
        {
            if (String.IsNullOrEmpty(policy))
            {
                throw new ArgumentException($"'{nameof(policy)}' cannot be null or empty.", nameof(policy));
            }

            _settings.ReferrerPolicy = policy;

            return this;
        }

        public SecurityHeadersBuilder AddStrictTransportSecurity(StrictTransportSecurityOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _settings.StrictTransportSecurity = options;

            return this;
        }

        public SecuritySettings Build() => _settings;
    }
}
