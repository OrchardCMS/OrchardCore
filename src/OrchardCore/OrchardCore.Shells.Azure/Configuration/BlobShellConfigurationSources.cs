using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration
{
    public class BlobShellConfigurationSources : IShellConfigurationSources
    {
        private readonly IShellsFileStore _shellsFileStore;
        private readonly string _container;

        public BlobShellConfigurationSources(
            IOptions<ShellOptions> shellOptions,
            IShellsFileStore shellsFileStore)
        {
            // e.g., Sites.
            _container = shellOptions.Value.ShellsContainerName;
            _shellsFileStore = shellsFileStore;
        }

        public void AddSources(string tenant, IConfigurationBuilder builder)
        {
            var appsettings = IFileStoreExtensions.Combine(null, _container, tenant, "appsettings.json");
            var fileInfo = _shellsFileStore.GetFileInfoAsync(appsettings).GetAwaiter().GetResult();
            if (fileInfo != null)
            {
                var stream = _shellsFileStore.GetFileStreamAsync(appsettings).GetAwaiter().GetResult();
                builder.AddJsonStream(stream);
            }
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            var appsettings = IFileStoreExtensions.Combine(null, _container, tenant, "appsettings.json");

            JObject config;
            var fileInfo = await _shellsFileStore.GetFileInfoAsync(appsettings);

            if (fileInfo != null)
            {
                using (var stream = await _shellsFileStore.GetFileStreamAsync(appsettings))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        using (var reader = new JsonTextReader(streamReader))
                        {
                            config = await JObject.LoadAsync(reader);
                        }
                    }
                }
            }
            else
            {
                config = new JObject();
            }

            foreach (var key in data.Keys)
            {
                if (data[key] != null)
                {
                    config[key] = data[key];
                }
                else
                {
                    config.Remove(key);
                }
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using (var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
                    {
                        await config.WriteToAsync(jsonWriter);
                        await jsonWriter.FlushAsync();
                        memoryStream.Position = 0;
                        await _shellsFileStore.CreateFileFromStreamAsync(appsettings, memoryStream);
                    }
                }
            }

        }
    }
}
