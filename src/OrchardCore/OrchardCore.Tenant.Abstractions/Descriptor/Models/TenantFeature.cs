namespace OrchardCore.Tenant.Descriptor.Models
{
    public class TenantFeature
    {
        public TenantFeature()
        {
        }

        public TenantFeature(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }
}
