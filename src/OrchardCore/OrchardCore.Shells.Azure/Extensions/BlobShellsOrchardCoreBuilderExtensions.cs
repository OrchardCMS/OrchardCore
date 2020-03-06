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
        /// Host services to load site and shell settings from Azure Blob Storage.
        /// </summary>
        public static OrchardCoreBuilder AddAzureShellsConfiguration(this OrchardCoreBuilder builder)
        {
            var services = builder.ApplicationServices;

            services.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            services.AddSingleton<IShellsFileStore>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var blobStorageOptions = configuration.GetSection("OrchardCore:OrchardCore.Shells.Azure").Get<BlobShellStorageOptions>();

                var clock = serviceProvider.GetRequiredService<IClock>();
                var contentTypeProvider = serviceProvider.GetRequiredService<IContentTypeProvider>();

                var fileStore = new BlobFileStore(blobStorageOptions, clock, contentTypeProvider);

                return new BlobShellsFileStore(fileStore);
            });

            services.Replace(ServiceDescriptor.Singleton<IShellsSettingsSources>(sp =>
            {
                var shellsFileStore = sp.GetRequiredService<IShellsFileStore>();

                return new BlobShellsSettingsSources(shellsFileStore);
            }));

            services.Replace(ServiceDescriptor.Singleton<IShellConfigurationSources>(sp =>
            {
                var shellOptions = sp.GetRequiredService<IOptions<ShellOptions>>();
                var shellsFileStore = sp.GetRequiredService<IShellsFileStore>();

                return new BlobShellConfigurationSources(shellOptions, shellsFileStore);
            }));

            services.Replace(ServiceDescriptor.Singleton<IShellsConfigurationSources>(sp =>
            {
                var shellsFileStore = sp.GetRequiredService<IShellsFileStore>();
                var environment = sp.GetRequiredService<IHostEnvironment>();

                return new BlobShellsConfigurationSources(shellsFileStore, environment);
            }));

            return builder;
        }
    }
}
