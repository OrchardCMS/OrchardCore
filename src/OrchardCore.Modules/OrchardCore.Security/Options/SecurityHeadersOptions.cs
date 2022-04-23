using System;
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
                new ContentTypeOptionsHeaderPolicyProvider { Options = this },
                new FrameOptionsHeaderPolicyProvider { Options = this },
                new PermissionsHeaderPolicyProvider { Options = this },
                new ReferrerHeaderPolicyProvider { Options = this }
            };
        }

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string FrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;

        public string[] PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public IList<IHeaderPolicyProvider> HeaderPolicyProviders { get; set; }

        public SecurityHeadersOptions AddContentTypeOptions()
        {
            ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

            return this;
        }

        public SecurityHeadersOptions AddFrameOptions(Action<FrameOptionsOptionsBuilder> optionsAction)
        {
            var options = new FrameOptionsOptions();
            var builder = new FrameOptionsOptionsBuilder(options);
            optionsAction.Invoke(builder);

            return AddFrameOptions(options.Option);
        }

        public SecurityHeadersOptions AddFrameOptions(string options)
        {
            FrameOptions = options;

            return this;
        }

        public SecurityHeadersOptions AddPermissionsPolicy(string policies)
            => AddPermissionsPolicy(policies.Split(PermissionsPolicyOptions.Separator));

        public SecurityHeadersOptions AddPermissionsPolicy(params string[] policies)
        {
            PermissionsPolicy = policies;

            return this;
        }

        public SecurityHeadersOptions AddPermissionsPolicy(Action<PermissionsPolicyOptionsBuilder> optionsAction)
        {
            var options = new PermissionsPolicyOptions();
            var builder = new PermissionsPolicyOptionsBuilder(options);
            optionsAction.Invoke(builder);

            return AddPermissionsPolicy(options.ToString());
        }

        public SecurityHeadersOptions AddReferrerPolicy(Action<ReferrerPolicyOptionsBuilder> optionsAction)
        {
            var options = new ReferrerPolicyOptions();
            var builder = new ReferrerPolicyOptionsBuilder(options);
            optionsAction.Invoke(builder);

            return AddReferrerPolicy(options.Policy);
        }

        public SecurityHeadersOptions AddReferrerPolicy(string policy)
        {
            ReferrerPolicy = policy;

            return this;
        }
    }
}
