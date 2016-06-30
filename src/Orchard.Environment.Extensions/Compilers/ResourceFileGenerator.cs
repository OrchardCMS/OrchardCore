using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Orchard.Environment.Extensions.Compilers
{
    internal class ResourceFileGenerator
    {
        public static void Generate(ResourceFile sourceFile, Stream outputStream)
        {
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));
            using (var input = sourceFile.File.OpenRead())
            {
                var document = XDocument.Load(input);
                var data = document.Root.Elements("data");
                if (data.Any())
                {
                    var rw = new ResourceWriter(outputStream);

                    foreach (var e in data)
                    {
                        var name = e.Attribute("name").Value;
                        var value = e.Element("value").Value;
                        rw.AddResource(name, value);
                    }

                    rw.Generate();
                }
            }
        }
    }
}