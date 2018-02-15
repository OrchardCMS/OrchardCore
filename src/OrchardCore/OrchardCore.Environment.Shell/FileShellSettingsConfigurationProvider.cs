using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public class FileShellSettingsConfigurationProvider : IShellSettingsConfigurationProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public FileShellSettingsConfigurationProvider(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public int Order => 1000;

        public void AddSource(IConfigurationBuilder builder)
        {
            builder.AddJsonFile(Path.Combine(_hostingEnvironment.ContentRootPath, "tenants.json"));
        }

        public void SaveToSource(string name, IDictionary<string, string> configuration)
        {
        }
    }

    public class ShellSettingsWithTenants : ShellSettings
    {
        public ShellSettingsWithTenants(ShellSettings shellSettings) : base(shellSettings.Configuration)
        {
            Features = shellSettings
                .Configuration
                .Where(x => x.Key.StartsWith("Features") && x.Value != null).Select(x => x.Value).ToArray();
        }

        public string[] Features { get; set; }
    }
}