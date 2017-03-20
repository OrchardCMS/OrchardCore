namespace OrchardCore.Tenant
{
    public class TenantOptions
    {
        /// <summary>
        /// The root container
        /// </summary>
        public string TenantsRootContainerName { get; set; }

        /// <summary>
        /// The container for tenants
        /// </summary>
        public string TenantsContainerName { get; set; }
    }
}