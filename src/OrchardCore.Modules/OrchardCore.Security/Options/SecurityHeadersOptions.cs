using System.Collections.Generic;
using OrchardCore.Security.Services;

namespace OrchardCore.Security.Options
{
    public class SecurityHeadersOptions
    {
        public SecurityHeadersOptions()
        {
            HeaderPolicyProviders = new List<IHeaderPolicyProvider>
            {
                new ContentSecurityPolicyHeaderPolicyProvider { Options = this },
                new ContentTypeOptionsHeaderPolicyProvider { Options = this },
                new FrameOptionsHeaderPolicyProvider { Options = this },
                new PermissionsHeaderPolicyProvider { Options = this },
                new ReferrerHeaderPolicyProvider { Options = this }
            };
        }

        public string[] ContentSecurityPolicy { get; set; } = SecurityHeaderDefaults.ContentSecurityPolicy;

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string FrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;

        public string[] PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public IList<IHeaderPolicyProvider> HeaderPolicyProviders { get; set; }

        public SecurityHeadersOptions AddContentSecurityPolicy(string policies)
            => AddContentSecurityPolicy(policies.Split(SecurityHeaderDefaults.PoliciesSeparater));

        public SecurityHeadersOptions AddContentSecurityPolicy(params string[] policies)
        {
            ContentSecurityPolicy = policies;

            return this;
        }

        public SecurityHeadersOptions AddContentTypeOptions()
        {
            ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

            return this;
        }

        public SecurityHeadersOptions AddFrameOptions(string options)
        {
            FrameOptions = options;

            return this;
        }

        public SecurityHeadersOptions AddPermissionsPolicy(string policies)
            => AddPermissionsPolicy(policies.Split(SecurityHeaderDefaults.PoliciesSeparater));

        public SecurityHeadersOptions AddPermissionsPolicy(params string[] policies)
        {
            PermissionsPolicy = policies;

            return this;
        }

        public SecurityHeadersOptions AddReferrerPolicy(string policy)
        {
            ReferrerPolicy = policy;

            return this;
        }
    }
}
