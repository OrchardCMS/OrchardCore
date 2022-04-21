using System;

namespace OrchardCore.Security.Options
{
    public class SecurityHeadersOptionsBuilder
    {
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersOptionsBuilder(SecurityHeadersOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public SecurityHeadersOptionsBuilder AddContentTypeOptions()
        {
            _options.ContentTypeOptions = new ContentTypeOptionsOptions();

            return this;
        }

        public SecurityHeadersOptionsBuilder AddFrameOptions(FrameOptionsOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options.FrameOptions = options;

            return this;
        }

        public SecurityHeadersOptionsBuilder AddFrameOptions(Action<FrameOptionsOptionsBuilder> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var options = new FrameOptionsOptions();
            var builder = new FrameOptionsOptionsBuilder(options);

            action(builder);

            return AddFrameOptions(options);
        }

        public SecurityHeadersOptionsBuilder AddPermissionsPolicy(PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options.PermissionsPolicy = options;

            return this;
        }

        public SecurityHeadersOptionsBuilder AddPermissionsPolicy(Action<PermissionsPolicyOptionsBuilder> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var options = new PermissionsPolicyOptions();
            var builder = new PermissionsPolicyOptionsBuilder(options);

            action(builder);

            return AddPermissionsPolicy(options);
        }

        public SecurityHeadersOptionsBuilder AddReferrerPolicy(ReferrerPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options.ReferrerPolicy = options;

            return this;
        }

        public SecurityHeadersOptionsBuilder AddReferrerPolicy(Action<ReferrerPolicyOptionsBuilder> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var options = new ReferrerPolicyOptions();
            var builder = new ReferrerPolicyOptionsBuilder(options);

            action(builder);

            return AddReferrerPolicy(options);
        }

        public SecurityHeadersOptionsBuilder AddStrictTransportSecurity(StrictTransportSecurityOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options.StrictTransportSecurity = options;

            return this;
        }

        public SecurityHeadersOptionsBuilder AddStrictTransportSecurity(Action<StrictTransportSecurityOptionsBuilder> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var options = new StrictTransportSecurityOptions();
            var builder = new StrictTransportSecurityOptionsBuilder(options);

            action(builder);

            return AddStrictTransportSecurity(options);
        }
    }
}
