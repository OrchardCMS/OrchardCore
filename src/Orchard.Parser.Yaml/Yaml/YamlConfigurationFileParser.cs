using Microsoft.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Orchard.Parser.Yaml {
    public class YamlConfigurationFileParser {
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;

        public IDictionary<string, string> Parse(Stream stream) {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var yamlConfig = new YamlStream();
            yamlConfig.Load(new StreamReader(stream));

            var mapping =
                (YamlMappingNode)yamlConfig.Documents[0].RootNode;

            VisitYamlMappingNode(mapping);

            return _data;
        }

        private void VisitYamlMappingNode(YamlMappingNode yamlNode) {
            foreach (var entry in yamlNode.Children) {
                VisitYamlNode(entry);
            }
        }

        private void VisitYamlNode(KeyValuePair<YamlNode, YamlNode> node) {
            if (node.Value is YamlScalarNode)
                VisitYamlScalarNode((YamlScalarNode)node.Key, (YamlScalarNode)node.Value);

            if (node.Value is YamlMappingNode)
                VisitYamlMappingNode((YamlScalarNode)node.Key, (YamlMappingNode)node.Value);
        }

        private void VisitYamlMappingNode(YamlScalarNode yamlNodeKey, YamlMappingNode yamlNodeValue) {
            EnterContext(yamlNodeKey.Value);

            VisitYamlMappingNode(yamlNodeValue);

            ExitContext();
        }

        private void VisitYamlScalarNode(YamlScalarNode yamlNodeKey, YamlScalarNode yamlNodeValue) {
            EnterContext(yamlNodeKey.Value);
            var key = _currentPath;

            if (_data.ContainsKey(key)) {
                throw new FormatException(string.Format("FormatError_KeyIsDuplicated({0})", key));
            }
            _data[key] = yamlNodeValue.Value;
            ExitContext();
        }



        private void EnterContext(string context) {
            _context.Push(context);
            _currentPath = string.Join(Constants.KeyDelimiter, _context.Reverse());
        }

        private void ExitContext() {
            _context.Pop();
            _currentPath = string.Join(Constants.KeyDelimiter, _context.Reverse());
        }
    }

                //var mapping =
                //    (YamlMappingNode)yaml.Documents[0].RootNode;

                //foreach (var entry in mapping.Children) {
                //    Console.WriteLine(((YamlScalarNode)entry.Key).Value);
                //}


                //    while (reader.Peek() != -1) {
                //        var rawLine = reader.ReadLine();
                //        var line = rawLine.Trim();

                //        // Ignore blank lines
                //        if (string.IsNullOrWhiteSpace(line)) {
                //            continue;
                //        }
                //        // Ignore comments
                //        if (line[0] == ';' || line[0] == '#' || line[0] == '/') {
                //            continue;
                //        }

                //        var separatorIndex = line.IndexOf(Separator);
                //        if (separatorIndex == -1) {
                //            continue;
                //        }
                //        string key = line.Substring(0, separatorIndex).Trim();
                //        string value = line.Substring(separatorIndex + 1).Trim();

                //        if (value.Equals(EmptyValue, StringComparison.OrdinalIgnoreCase)) {
                //            continue;
                //        }

                //        data[key] = value;
                //    }
        //    }
        //}
    //}
}
