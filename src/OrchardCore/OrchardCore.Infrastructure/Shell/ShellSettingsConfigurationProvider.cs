using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Yaml;

namespace OrchardCore.Environment.Shell.Data
{
    public class ShellSettingsConfigurationProvider : IShellSettingsConfigurationProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ShellOptions> _optionsAccessor;
        
        public ShellSettingsConfigurationProvider(
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> optionsAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _optionsAccessor = optionsAccessor;
        }

        public int Order => 1000;

        public void AddSource(IConfigurationBuilder builder)
        {
            var tenantSettingsPath = Path.Combine(
                        _optionsAccessor.Value.ShellsApplicationDataPath,
                        _optionsAccessor.Value.ShellsContainerName);

            if (!Directory.Exists(tenantSettingsPath))
            {
                return;
            }

            var tenants = Directory.GetDirectories(tenantSettingsPath);

            foreach (var tenant in tenants)
            {
                var filePath = GetSettingsFilePath(tenant);

                if (File.Exists(filePath))
                {
                    builder.AddYamlFile(filePath);
                }
            }
        }

        public void SaveToSource(string name, IDictionary<string, string> configuration)
        {
            var settingsFile = GetSettingsFilePath(Path.Combine(
                        _optionsAccessor.Value.ShellsApplicationDataPath,
                        _optionsAccessor.Value.ShellsContainerName,
                        name));

            var configurationProvider = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = settingsFile,
                Optional = false
            });

            configurationProvider.Set(name, null);
            configurationProvider.Set($"{name}:RequestUrlHost", ObtainValue(configuration,$"{name}:RequestUrlHost"));
            configurationProvider.Set($"{name}:RequestUrlPrefix", ObtainValue(configuration,$"{name}:RequestUrlPrefix"));
            configurationProvider.Set($"{name}:DatabaseProvider", ObtainValue(configuration,$"{name}:DatabaseProvider"));
            configurationProvider.Set($"{name}:TablePrefix", ObtainValue(configuration,$"{name}:TablePrefix"));
            configurationProvider.Set($"{name}:ConnectionString", ObtainValue(configuration,$"{name}:ConnectionString"));
            configurationProvider.Set($"{name}:State", ObtainValue(configuration,$"{name}:State"));
            configurationProvider.Set($"{name}:Secret", ObtainValue(configuration, $"{name}:Secret"));
            configurationProvider.Set($"{name}:RecipeName", ObtainValue(configuration, $"{name}:RecipeName"));

            configurationProvider.Commit();
        }

        private string ObtainValue(IDictionary<string, string> configuration, string key)
        {
            configuration.TryGetValue(key, out string value);
            return (value ?? "~");
        }

        private string GetSettingsFilePath(string tenantFolderPath) => Path.Combine(tenantFolderPath, "Settings.txt");
    }
}
