namespace Microsoft.Extensions.DependencyInjection
{
    public class TenantServicesBuilder
    {
        public TenantServicesBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
