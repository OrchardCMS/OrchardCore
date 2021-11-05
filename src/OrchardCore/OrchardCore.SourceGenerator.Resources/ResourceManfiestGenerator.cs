using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace OrchardCore.SourceGenerator.Resources
{
    [Generator]
    internal class ResourceManfiestGenerator : ISourceGenerator
    {
        private static readonly string _resourcesFile = "resources.json";

        public void Execute(GeneratorExecutionContext context)
        {
            var sourceBuilder = new StringBuilder(@"
using System;

namespace OrchardCore.ResourceManagement
{
    public static class ResourceManfiestGenerator
    {
        const string monacoEditorVersion = ""0.29.1"";

        public static ResourceManifest Build(string tenantPrefix)
        {
            var manifest = new ResourceManifest();
            ResourceDefinition resource = null;");

            const string minificationFileExtension = ".min";

            var assembliesFolderPath = new FileInfo(typeof(Resource).Assembly.Location).Directory.FullName;
            var resourcesFilePath = Path.Combine(assembliesFolderPath, _resourcesFile);
            using (var reader = new StreamReader(resourcesFilePath))
            {
                var content = reader.ReadToEnd();
                var resources = JsonSerializer.Deserialize<Resources>(content);

                foreach (var resource in resources.Scripts)
                {
                    resource.Type = ResourceType.Script;

                    CreateResource(resource);
                }

                foreach (var resource in resources.Styles)
                {
                    resource.Type = ResourceType.Style;

                    CreateResource(resource);
                }
            }

            sourceBuilder.Append(@"
            manifest
                .DefineScript(""monaco-loader"")
                .SetUrl(""~/OrchardCore.Resources/Scripts/monaco/vs/loader.js"")
                .SetPosition(ResourcePosition.Last)
                .SetVersion(monacoEditorVersion);
            
            manifest
                .DefineScript(""monaco"")
                .SetAttribute(""data-tenant-prefix"", tenantPrefix)
                .SetUrl(""~/OrchardCore.Resources/Scripts/monaco/ocmonaco.js"")
                .SetDependencies(""monaco-loader"")
                .SetVersion(monacoEditorVersion);

            return manifest;
        }
    }
}");

            context.AddSource(nameof(ResourceManfiestGenerator), SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

            void CreateResource(Resource resource)
            {
                if (resource.Type == ResourceType.Script)
                {
                    sourceBuilder.AppendLine($@"
            resource = manifest
                .DefineScript(""{resource.Name}"")
                .SetVersion(""{resource.Version}"");");
                }
                else
                {
                    sourceBuilder.AppendLine($@"
            resource = manifest
                .DefineStyle(""{resource.Name}"")
                .SetVersion(""{resource.Version}"");");
                }

                if (resource.Dependencies != null)
                {
                    sourceBuilder.AppendLine($@"
            resource.SetDependencies({String.Join(',', resource.Dependencies.Select(d => "\"" + d + "\""))});");
                }

                if (!String.IsNullOrEmpty(resource.Url))
                {
                    if (resource.Url.Contains(minificationFileExtension))
                    {
                        sourceBuilder.AppendLine($@"
            resource.SetUrl(""{resource.Url}"", ""{resource.Url.Replace(minificationFileExtension, String.Empty)}"");");
                    }
                    else
                    {
                        sourceBuilder.AppendLine($@"
            resource.SetUrl(""{resource.Url}"");");
                    }
                }

                if (!String.IsNullOrEmpty(resource.Cdn))
                {
                    if (resource.Cdn.Contains(minificationFileExtension))
                    {
                        sourceBuilder.AppendLine($@"
            resource.SetUrl(""{resource.Cdn}"", ""{resource.Cdn.Replace(minificationFileExtension, String.Empty)}"");");
                    }
                    else
                    {
                        sourceBuilder.AppendLine($@"
            resource.SetUrl(""{resource.Cdn}"");");
                    }
                }

                if (resource.CdnIntegrity != null)
                {
                    sourceBuilder.AppendLine($@"
            resource.SetCdnIntegrity({String.Join(',', resource.CdnIntegrity.Select(c => "\"" + c + "\""))});");
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
