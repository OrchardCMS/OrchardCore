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

            var newConfigFileStream = new FileStream(Source.Path, FileMode.CreateNew);

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