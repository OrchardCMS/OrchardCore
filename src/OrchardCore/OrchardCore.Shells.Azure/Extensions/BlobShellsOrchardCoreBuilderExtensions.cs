using System;
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

            services.AddSingleton<IShellsFileStore>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                var blobOptions = configuration.GetSectionCompat("OrchardCore:OrchardCore_Shells_Azure")
                    .Get<BlobShellStorageOptions>()
                    ?? throw new ArgumentNullException(
                        nameof(BlobShellStorageOptions),
                        "The 'OrchardCore.Shells.Azure' configuration section must be defined");

                var clock = sp.GetRequiredService<IClock>();
                var contentTypeProvider = sp.GetRequiredService<IContentTypeProvider>();

                var fileStore = new BlobFileStore(blobOptions, clock, contentTypeProvider);

                return new BlobShellsFileStore(fileStore);
            });

            services.Replace(ServiceDescriptor.Singleton<IShellsSettingsSources>(sp =>
            {
                var shellsFileStore = sp.GetRequiredService<IShellsFileStore>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var blobOptions = configuration.GetSectionCompat("OrchardCore:OrchardCore_Shells_Azure").Get<BlobShellStorageOptions>();
                var shellOptions = sp.GetRequiredService<IOptions<ShellOptions>>();

                return new BlobShellsSettingsSources(shellsFileStore, blobOptions, shellOptions);
            }));

            services.Replace(ServiceDescriptor.Singleton<IShellConfigurationSources>(sp =>
            {
                var shellsFileStore = sp.GetRequiredService<IShellsFileStore>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var blobOptions = configuration.GetSectionCompat("OrchardCore:OrchardCore_Shells_Azure").Get<BlobShellStorageOptions>();
                var shellOptions = sp.GetRequiredService<IOptions<ShellOptions>>();

                return new BlobShellConfigurationSources(shellsFileStore, blobOptions, shellOptions);
            }));

            services.Replace(ServiceDescriptor.Singleton<IShellsConfigurationSources>(sp =>
            {
                var shellsFileStore = sp.GetRequiredService<IShellsFileStore>();
                var environment = sp.GetRequiredService<IHostEnvironment>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var blobOptions = configuration.GetSectionCompat("OrchardCore:OrchardCore_Shells_Azure").Get<BlobShellStorageOptions>();
                var shellOptions = sp.GetRequiredService<IOptions<ShellOptions>>();

                return new BlobShellsConfigurationSources(shellsFileStore, environment, blobOptions, shellOptions);
            }));

            return builder;
        }
    }
}
