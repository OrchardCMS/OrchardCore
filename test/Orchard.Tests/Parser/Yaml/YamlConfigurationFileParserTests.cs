using Orchard.Parser.Yaml;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Orchard.Tests.Parser.Yaml {
    public class YamlConfigurationFileParserTests {

        [Fact]
        public void LoadKeyValuePairsFromValidYaml() {
            var yaml = @"
firstname: test
test.last.name: last.name
residential.address:
    street.name: Something street
    zipcode: 12345";
            var yamlConfigSrc = new YamlConfigurationSource(TestStreamHelpers.ArbitraryFilePath);

            yamlConfigSrc.Load(TestStreamHelpers.StringToStream(yaml.TrimStart()));

            Assert.Equal("test", yamlConfigSrc.Get("firstname"));
            Assert.Equal("last.name", yamlConfigSrc.Get("test.last.name"));
            Assert.Equal("Something street", yamlConfigSrc.Get("residential.address:STREET.name"));
            Assert.Equal("12345", yamlConfigSrc.Get("residential.address:zipcode"));
        }

        [Fact]
        public void LoadMethodCanHandleEmptyValue() {
            var yaml = @"name:";
            var yamlConfigSrc = new YamlConfigurationSource(TestStreamHelpers.ArbitraryFilePath);

            yamlConfigSrc.Load(TestStreamHelpers.StringToStream(yaml));

            Assert.Equal(string.Empty, yamlConfigSrc.Get("name"));
        }

        [Fact]
        public void NonObjectRootIsInvalid() {
            var yaml = @"test";
            var yamlConfigSrc = new YamlConfigurationSource("Foo");

            var exception = Assert.Throws<FormatException>(
                () => yamlConfigSrc.Load(TestStreamHelpers.StringToStream(yaml)));

            Assert.NotNull(exception.Message);
        }

        [Fact]
        public void SupportAndIgnoreComments() {
            var yaml = @"# Comments
                # Comments
                name: test #foo
                address:
                    street: Something street # Comments
                    zipcode: 12345";
            var yamlConfigSrc = new YamlConfigurationSource("Foo");

            yamlConfigSrc.Load(TestStreamHelpers.StringToStream(yaml));

            Assert.Equal("test", yamlConfigSrc.Get("name"));
            Assert.Equal("Something street", yamlConfigSrc.Get("address:street"));
            Assert.Equal("12345", yamlConfigSrc.Get("address:zipcode"));
        }

        [Fact]
        public void SupportForMultiple() {
            var yaml = @"---
name: test #foo
address:
    home:
        street: Some Home Address
        zipcode: 12345
    work:
        street: Some Work Address
        zipcode: 54321
...";
            var yamlConfigSrc = new YamlConfigurationSource(TestStreamHelpers.ArbitraryFilePath);

            yamlConfigSrc.Load(TestStreamHelpers.StringToStream(yaml));
            
            Assert.Equal("Some Home Address", yamlConfigSrc.Get("address:home:street"));
            Assert.Equal("12345", yamlConfigSrc.Get("address:home:zipcode"));
            Assert.Equal("Some Work Address", yamlConfigSrc.Get("address:work:street"));
            Assert.Equal("54321", yamlConfigSrc.Get("address:work:zipcode"));
        }
        //[Fact]
        //public void ThrowExceptionWhenPassingNullAsFilePath() {
        //    var expectedMsg = new ArgumentException("File not found: ", "path").Message;

        //    var exception = Assert.Throws<ArgumentException>(() => new YamlConfigurationSource(null));

        //    Assert.Equal(expectedMsg, exception.Message);
        //}

        //[Fact]
        //public void ThrowExceptionWhenPassingEmptyStringAsFilePath() {
        //    var expectedMsg = new ArgumentException(Resources.Error_InvalidFilePath, "path").Message;

        //    var exception = Assert.Throws<ArgumentException>(() => new YamlConfigurationSource(string.Empty));

        //    Assert.Equal(expectedMsg, exception.Message);
        //}

        //[Fact]
        //public void YamlConfiguration_Throws_On_Missing_Configuration_File() {
        //    var configSource = new YamlConfigurationSource("NotExistingConfig.yaml", optional: false);
        //    var exception = Assert.Throws<FileNotFoundException>(() => configSource.Load());

        //    // Assert
        //    Assert.Equal(Resources.FormatError_FileNotFound("NotExistingConfig.yaml"), exception.Message);
        //}

        //[Fact]
        //public void YamlConfiguration_Does_Not_Throw_On_Optional_Configuration() {
        //    var configSource = new YamlConfigurationSource("NotExistingConfig.yaml", optional: true);
        //    configSource.Load();
        //    Assert.Throws<InvalidOperationException>(() => configSource.Get("key"));
        //}
    }
}
