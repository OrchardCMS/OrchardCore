using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Data
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        ///  Adds services at the host level to load site settings from the file system
        ///  and tenant level services to store states and descriptors in the database.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static ModularServicesBuilder AddDataStorage(this ModularServicesBuilder builder)
        {
            builder.Services.AddSitesFolder();

            builder.Services.ConfigureTenantServices(collection =>
            {
                collection.AddShellDescriptorStorage();
            });

            return builder;
        }
    }
}