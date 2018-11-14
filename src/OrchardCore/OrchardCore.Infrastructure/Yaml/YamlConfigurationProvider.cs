using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;

namespace OrchardCore.Yaml
{
    public class YamlConfigurationProvider : FileConfigurationProvider
    {
        public const char Separator = ':';
        public const string EmptyValue = "null";
        public const char ThemesSeparator = ';';

        public YamlConfigurationProvider(FileConfigurationSource source) : base(source)
        {
        }

        public override void Load(Stream stream)
        {
            var parser = new YamlConfigurationFileParser();
            try
            {
                Data = parser.Parse(stream);
            }
            catch (InvalidCastException e)
            {
                throw new FormatException("FormatError_YAMLparseError", e);
            }
        }

        public virtual void Commit()
        {
            FileStream newConfigFileStream = null;
            try
            {
                if (File.Exists(Source.Path))
                {
                    File.Delete(Source.Path);
                }

                var fileInfo = new FileInfo(Source.Path);

                if (!fileInfo.Directory.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                }

                // TODO: Revisit to make fully atomic.
                // https://github.com/aspnet/Configuration/pull/147/files

                newConfigFileStream = new FileStream(Source.Path, FileMode.CreateNew);
            }

            catch (IOException)
            {
                // The settings file may be own by another process or already exists when trying to create
                // a new one. So, nothing more that we can do. Note: Other exceptions are normally thrown.

                return;

                // We could think about a retry logic to honour each "transaction" and preserve some order.
                // But here we create a new settings file, so, anyway, only one concurrent writer will win.

                // Another potential issue is when one instance is reading the settings file while another
                // instance is re-creating it. But currently this can only occur when an instance startups.
            }

            try
            {
                // Generate contents and write it to the newly created config file
                GenerateNewConfig(newConfigFileStream);
            }
            catch
            {
                newConfigFileStream.Dispose();

                // The operation should be atomic because we don't want a corrupted config file
                // So we roll back if the operation fails
                if (File.Exists(Source.Path))
                {
                    File.Delete(Source.Path);
                }

                // Rethrow the exception
                throw;
            }
            finally
            {
                newConfigFileStream.Dispose();
            }

            return;
        }


        internal void GenerateNewConfig(Stream outputStream)
        {
            var outputWriter = new StreamWriter(outputStream);

            var rootNodeName = Data.ElementAt(0).Key;

            var docMapping = new YamlMappingNode();
            foreach (var item in Data.Skip(1))
            {
                docMapping.Add(item.Key.Replace((rootNodeName + ":"), string.Empty), (item.Value ?? EmptyValue));
                // TODO: If contains ":" then Mapping node etc
            }

            var yamlStream = new YamlStream(
                new YamlDocument(
                    new YamlMappingNode(
                        new YamlScalarNode(rootNodeName),
                        docMapping
                    )
                )
            );

            yamlStream.Save(outputWriter);
            outputWriter.Flush();
        }
    }
}