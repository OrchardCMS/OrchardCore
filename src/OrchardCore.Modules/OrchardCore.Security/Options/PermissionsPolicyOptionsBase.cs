namespace OrchardCore.Security.Options
{
    public abstract class PermissionsPolicyOptionsBase
    {
        public abstract string Name { get; }

        public PermissionsPolicyOriginValue Origin { get; set; } = PermissionsPolicyOriginValue.None;
    }
}
