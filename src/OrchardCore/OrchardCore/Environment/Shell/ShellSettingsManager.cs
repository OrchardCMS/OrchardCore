using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IEnumerable<IShellSettingsConfigurationProvider> _configurationProviders;

        public ShellSettingsManager(IEnumerable<IShellSettingsConfigurationProvider> configurationProviders)
        {
            _configurationProviders = configurationProviders.OrderBy(x => x.Order);
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            var configurationBuilder = new ConfigurationBuilder();

            foreach (var provider in _configurationProviders)
            {
                provider.AddSource(configurationBuilder);
            }

            var configurationRoot = configurationBuilder.Build();

            // All Tenant configuration files.
            // All tenants are keyed such 
            /*
            {
                Default: {
                    Foo: blah    
                }
            }
            Default:
                Foo: blah

            Key: Default:Foo
             */
            foreach (var tenant in configurationRoot.GetChildren())
            {
                var values = tenant
                    .AsEnumerable()
                    .ToDictionary(k => k.Key.Replace((tenant.Key + ":"), string.Empty), v => v.Value);

                // More a replace.
                values.Remove(tenant.Key);
                values.Add("Name", tenant.Key);

                // What goes in to here is everything but with tenant name removed.
                yield return new ShellSettings(values);
            }
        }

        public bool TryLoadSettings(string name, out ShellSettings settings)
        {
            var configurationBuilder = new ConfigurationBuilder();

            foreach (var provider in _configurationProviders)
            {
                provider.AddSource(configurationBuilder, name);
            }

            var configurationRoot = configurationBuilder.Build();

            var tenant = configurationRoot.GetChildren().FirstOrDefault(t => t.Key == name);

            if (tenant == null)
            {
                settings = null;
                return false;
            }

            var values = tenant
                .AsEnumerable()
                .ToDictionary(k => k.Key.Replace((tenant.Key + ":"), string.Empty), v => v.Value);

            // More a replace.
            values.Remove(tenant.Key);
            values.Add("Name", tenant.Key);

            // What goes in to here is everything but with tenant name removed.
            settings = new ShellSettings(values);
            return true;
        }

        public void SaveSettings(ShellSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var configuration = new Dictionary<string, string> { { settings.Name, null } };

            var settingsconfiguration = settings.Configuration;

            foreach (var item in settingsconfiguration)
            {
                configuration.Add($"{settings.Name}:{item.Key}", item.Value);
            }

            configuration.Remove($"{settings.Name}:Name");

            foreach (var provider in _configurationProviders)
            {
                provider.SaveToSource(settings.Name, configuration);
            }
        }
    }
}