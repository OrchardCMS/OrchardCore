using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;

namespace Orchard.Parser.Yaml
{
    internal class YamlConfigurationFileParser
    {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        public IDictionary<string, string> Parse(Stream stream)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var yamlConfig = new YamlStream();
            yamlConfig.Load(new StreamReader(stream));

            if (yamlConfig.Documents.Any())
            {
                var mapping = (YamlMappingNode)yamlConfig.Documents[0].RootNode;

                VisitYamlMappingNode(mapping);
            };

            return _data;
        }

        private void VisitYamlMappingNode(YamlMappingNode yamlNode)
        {
            foreach (var entry in yamlNode.Children)
            {
                VisitYamlNode(entry);
            }
        }

        private void VisitYamlSequenceNode(YamlSequenceNode yamlNode)
        {
            foreach (var entry in yamlNode.Children)
            {
                if (entry is YamlMappingNode)
                    VisitYamlMappingNode((YamlMappingNode)entry);
            }
        }

        private void VisitYamlNode(KeyValuePair<YamlNode, YamlNode> node)
        {
            if (node.Value is YamlScalarNode)
                VisitYamlScalarNode((YamlScalarNode)node.Key, (YamlScalarNode)node.Value);

            if (node.Value is YamlMappingNode)
                VisitYamlMappingNode((YamlScalarNode)node.Key, (YamlMappingNode)node.Value);

            if (node.Value is YamlSequenceNode)
                VisitYamlSequenceNode((YamlScalarNode)node.Key, (YamlSequenceNode)node.Value);
        }

        private void VisitYamlMappingNode(YamlScalarNode yamlNodeKey, YamlMappingNode yamlNodeValue)
        {
            EnterContext(yamlNodeKey.Value);

            VisitYamlMappingNode(yamlNodeValue);

            ExitContext();
        }

        private void VisitYamlSequenceNode(YamlScalarNode yamlNodeKey, YamlSequenceNode yamlNodeValue)
        {
            EnterContext(yamlNodeKey.Value);
            VisitYamlSequenceNode(yamlNodeValue);
            ExitContext();
        }

        private void VisitYamlScalarNode(YamlScalarNode yamlNodeKey, YamlScalarNode yamlNodeValue)
        {
            EnterContext(yamlNodeKey.Value);
            var key = _currentPath;

            if (_data.ContainsKey(key))
            {
                throw new FormatException(string.Format("FormatError_KeyIsDuplicated({0})", key));
            }
            _data[key] = yamlNodeValue.Value;
            ExitContext();
        }



        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = string.Join(ConfigurationPath.KeyDelimiter, _context.Reverse());
        }

        private void ExitContext()
        {
            _context.Pop();
            _currentPath = string.Join(ConfigurationPath.KeyDelimiter, _context.Reverse());
        }
    }
}