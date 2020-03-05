using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;
using OrchardCore.Shells.Azure.Configuration;
using OrchardCore.Shells.Azure.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BlobShellsOrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Host services to load site settings from Azure Blob Storage.
        /// </summary>
        public static OrchardCoreBuilder AddAzureShellsConfiguration(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddSingleton<IShellsFileStore>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var blobStorageOptions = configuration.GetSection("OrchardCore:OrchardCore.Shells.Azure").Get<BlobShellStorageOptions>();

                //TODO we could append ShellOptions.ShellsApplicationDataPath to the base path to give 'App_Data' as a default base path.
                // to avoid confusion, but not essential.
                var clock = serviceProvider.GetRequiredService<IClock>();
                var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();

                var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);

                return new BlobShellsFileStore(fileStore);
            });


            // We cannot check configuration options here. 
            services.Replace(ServiceDescriptor.Singleton<IShellsSettingsSources>(sp =>
            {
                var shellFileStore = sp.GetRequiredService<IShellsFileStore>();
                return new BlobShellsSettingsSources(shellFileStore);
            }));

            services.Replace(ServiceDescriptor.Singleton<IShellConfigurationSources>(sp =>
            {
                var shellOptions = sp.GetRequiredService<IOptions<ShellOptions>>();
                var shellFileStore = sp.GetRequiredService<IShellsFileStore>();
                return new BlobShellConfigurationSources(shellOptions, shellFileStore);
            }));

            services.Replace(ServiceDescriptor.Singleton<IShellsConfigurationSources>(sp =>
            {
                var shellFileStore = sp.GetRequiredService<IShellsFileStore>();
                var environment = sp.GetRequiredService<IHostEnvironment>();
                return new BlobShellsConfigurationSources(shellFileStore, environment);
            }));

            //services.AddSingleton<IShellsConfigurationSources, ShellsConfigurationSources>();
            //services.AddSingleton<IShellConfigurationSources, ShellConfigurationSources>();
            //services.AddTransient<IConfigureOptions<ShellOptions>, ShellOptionsSetup>();
            //services.AddSingleton<IShellSettingsManager, ShellSettingsManager>();

            return builder;
        }
    }
}
