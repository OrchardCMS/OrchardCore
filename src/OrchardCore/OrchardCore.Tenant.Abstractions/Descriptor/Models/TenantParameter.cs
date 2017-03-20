namespace OrchardCore.Tenant.Descriptor.Models
{
    /// <summary>
    /// A tenant parameter is a custom value that can be assigned to a specific component in a tenant.
    /// </summary>
    public class TenantParameter
    {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
