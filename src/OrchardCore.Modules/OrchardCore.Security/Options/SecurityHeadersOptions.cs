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

        public SecurityHeadersOptions AddContentSecurityPolicy(Dictionary<string, string> policies)
        {
            ContentSecurityPolicy = policies
                .Where(kvp => kvp.Value != null ||
                    kvp.Key == ContentSecurityPolicyValue.Sandbox ||
                    kvp.Key == ContentSecurityPolicyValue.UpgradeInsecureRequests)
                .Select(kvp => kvp.Key + (kvp.Value != null ? " " + kvp.Value : String.Empty))
                .ToArray();

            return this;
        }

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

        public SecurityHeadersOptions AddPermissionsPolicy(IDictionary<string, string> policies)
        {
            PermissionsPolicy = policies
                .Where(kvp => kvp.Value != PermissionsPolicyOriginValue.None)
                .Select(kvp => kvp.Key + "=" + kvp.Value)
                .ToArray();

            return this;
        }

        public SecurityHeadersOptions AddReferrerPolicy(string policy)
        {
            ReferrerPolicy = policy;

            return this;
        }
    }
}
