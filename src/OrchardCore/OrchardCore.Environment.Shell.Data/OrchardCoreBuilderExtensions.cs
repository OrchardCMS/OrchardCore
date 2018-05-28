using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Data
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        ///  Adds services at the host level to load site settings from the file system
        ///  and tenant level services to store states and descriptors in the database.
        /// </summary>
        public static OrchardCoreBuilder AddDataStorage(this OrchardCoreBuilder builder)
        {
            builder.Services.AddSitesFolder();
            
            builder.Startup.ConfigureServices((collection, sp) =>
            {
                collection.AddShellDescriptorStorage();
            });

            return builder;
        }
    }
}