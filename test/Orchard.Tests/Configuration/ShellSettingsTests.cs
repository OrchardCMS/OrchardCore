using Microsoft.Extensions.Configuration;
using Orchard.Environment.Shell;
using Orchard.Parser.Yaml;
using System;
using System.IO;
using Xunit;

namespace Orchard.Tests.Configuration
{
    public class ShellSettingsTests : IDisposable
    {
        private string _tempFolderName;

        public ShellSettingsTests()
        {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
        }

        public void Dispose()
        {
            Directory.Delete(_tempFolderName, true);
        }

        [Fact(Skip = "Not Complete")]
        public void Foo()
        {
            var yaml = @"---
name: Default
dataproviders:
    entityframework-sql:
        connectionstring: Foo
        table-prefix: 12345
    entityframework-inmemory:
...";
            File.WriteAllText(_tempFolderName, yaml);

            var yamlConfigPrd = new YamlConfigurationProvider(_tempFolderName);

            var root = new ConfigurationBuilder().Add(yamlConfigPrd).Build();

            var settings = new ShellSettings(root);

            //Assert.Equal(, settings.DataProviders.First())
        }
    }
}