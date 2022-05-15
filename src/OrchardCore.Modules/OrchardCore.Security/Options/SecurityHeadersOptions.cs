using System;
using System.Collections.Generic;
using System.Linq;
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
                new PermissionsHeaderPolicyProvider { Options = this },
                new ReferrerHeaderPolicyProvider { Options = this }
            };
        }

        public string[] ContentSecurityPolicy { get; set; } = SecurityHeaderDefaults.ContentSecurityPolicy;

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string[] PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public IList<IHeaderPolicyProvider> HeaderPolicyProviders { get; set; }

        public SecurityHeadersOptions AddContentSecurityPolicy(string policies)
            => AddContentSecurityPolicy(policies.Split(SecurityHeaderDefaults.PoliciesSeparator, StringSplitOptions.RemoveEmptyEntries));

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

        public SecurityHeadersOptions AddPermissionsPolicy(Dictionary<string, string> policies)
        {
            PermissionsPolicy = policies
                .Where(x => x.Value != PermissionsPolicyOriginValue.None)
                .Select(x => x.Key + "=" + x.Value)
                .ToArray();

            return this;
        }

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
