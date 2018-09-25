using System;
using OrchardCore.Yaml;
using Xunit;

namespace OrchardCore.Tests.Parser.Yaml
{
    public class YamlConfigurationFileParserTests
    {
        private YamlConfigurationProvider LoadProvider(string yaml)
        {
            var provider = new YamlConfigurationProvider(new YamlConfigurationSource { Optional = true });
            provider.Load(TestStreamHelpers.StringToStream(yaml));

            return provider;
        }

        [Fact]
        public void LoadKeyValuePairsFromValidYaml()
        {
            var yaml = @"
firstname: test
test.last.name: last.name
residential.address:
    street.name: Something street
    zipcode: 12345";

            var yamlConfigPrd = LoadProvider(yaml.TrimStart());

            Assert.Equal("test", yamlConfigPrd.Get("firstname"));
            Assert.Equal("last.name", yamlConfigPrd.Get("test.last.name"));
            Assert.Equal("Something street", yamlConfigPrd.Get("residential.address:STREET.name"));
            Assert.Equal("12345", yamlConfigPrd.Get("residential.address:zipcode"));
        }

        [Fact]
        public void LoadMethodCanHandleEmptyValue()
        {
            var yaml = @"name:";

            var yamlConfigPrd = LoadProvider(yaml);

            Assert.Equal(string.Empty, yamlConfigPrd.Get("name"));
        }

        [Fact]
        public void NonObjectRootIsInvalid()
        {
            var yaml = @"test";

            var yamlConfigPrd = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = "Foo",
                Optional = false
            });

            var exception = Assert.Throws<FormatException>(
                () => yamlConfigPrd.Load(TestStreamHelpers.StringToStream(yaml)));

            Assert.NotNull(exception.Message);
        }

        [Fact]
        public void SupportAndIgnoreComments()
        {
            var yaml = @"# Comments
                # Comments
                name: test #foo
                address:
                    street: Something street # Comments
                    zipcode: 12345";
            var yamlConfigPrd = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = "Foo",
                Optional = false
            });

            yamlConfigPrd.Load(TestStreamHelpers.StringToStream(yaml));

            Assert.Equal("test", yamlConfigPrd.Get("name"));
            Assert.Equal("Something street", yamlConfigPrd.Get("address:street"));
            Assert.Equal("12345", yamlConfigPrd.Get("address:zipcode"));
        }

        [Fact]
        public void SupportForMultiple()
        {
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

            var yamlConfigPrd = LoadProvider(yaml);

            Assert.Equal("Some Home Address", yamlConfigPrd.Get("address:home:street"));
            Assert.Equal("12345", yamlConfigPrd.Get("address:home:zipcode"));
            Assert.Equal("Some Work Address", yamlConfigPrd.Get("address:work:street"));
            Assert.Equal("54321", yamlConfigPrd.Get("address:work:zipcode"));
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