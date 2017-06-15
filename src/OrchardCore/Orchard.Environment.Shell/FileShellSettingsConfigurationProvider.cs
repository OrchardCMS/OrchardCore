using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Shell
{
    public class FileShellSettingsConfigurationProvider : IShellSettingsConfigurationProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public FileShellSettingsConfigurationProvider(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

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
            _features = shellSettings
                .Configuration
                .Where(xd => xd.Key.StartsWith("Features:")).Select(xa => xa.Value).ToArray();
        }

        private string[] _features = Array.Empty<string>();
        public string[] Features
        {
            get => _features;
            set
            {
                _features = value;
            }
        }
    }
}