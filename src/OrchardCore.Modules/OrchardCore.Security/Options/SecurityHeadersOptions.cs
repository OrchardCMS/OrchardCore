namespace OrchardCore.Security.Options
{
    public class SecurityHeadersOptions
    {
        public SecurityHeadersOptions()
        {
            ContentTypeOptions = new ContentTypeOptionsOptions();
            FrameOptions = new FrameOptionsOptions { Value = FrameOptionsValue.SameOrigin };
            PermissionsPolicy = new PermissionsPolicyOptions();
            ReferrerPolicy = new ReferrerPolicyOptions { Value = ReferrerPolicyValue.NoReferrer };
        }

        public ContentTypeOptionsOptions ContentTypeOptions { get; set; }

        public FrameOptionsOptions FrameOptions { get; set; }

        public PermissionsPolicyOptions PermissionsPolicy { get; set; }

        public ReferrerPolicyOptions ReferrerPolicy { get; set; }
    }
}
