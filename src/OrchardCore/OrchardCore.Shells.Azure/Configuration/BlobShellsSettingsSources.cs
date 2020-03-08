using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration
{
    public class BlobShellsSettingsSources : IShellsSettingsSources
    {
        private const string _tenantsFileName = "tenants.json";

        private readonly IShellsFileStore _shellsFileStore;

        public BlobShellsSettingsSources(IShellsFileStore shellsFileStore)
        {
            _shellsFileStore = shellsFileStore;
        }

        public async Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            var fileInfo = await _shellsFileStore.GetFileInfoAsync(_tenantsFileName);
            if (fileInfo != null)
            {
                var stream = await _shellsFileStore.GetFileStreamAsync(_tenantsFileName);
                builder.AddJsonStream(stream);
            }
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            JObject tenantsSettings;

            var fileInfo = await _shellsFileStore.GetFileInfoAsync(_tenantsFileName);

            if (fileInfo != null)
            {
                using (var stream = await _shellsFileStore.GetFileStreamAsync(_tenantsFileName))
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        using (var reader = new JsonTextReader(streamReader))
                        {
                            tenantsSettings = await JObject.LoadAsync(reader);
                        }
                    }
                }
            }
            else
            {
                tenantsSettings = new JObject();
            }

            var settings = tenantsSettings.GetValue(tenant) as JObject ?? new JObject();

            foreach (var key in data.Keys)
            {
                if (data[key] != null)
                {
                    settings[key] = data[key];
                }
                else
                {
                    settings.Remove(key);
                }
            }

            tenantsSettings[tenant] = settings;

            using (var memoryStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using (var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented })
                    {
                        await tenantsSettings.WriteToAsync(jsonWriter);
                        await jsonWriter.FlushAsync();
                        memoryStream.Position = 0;
                        await _shellsFileStore.CreateFileFromStreamAsync(_tenantsFileName, memoryStream);
                    }
                }
            }
        }
    }
}
