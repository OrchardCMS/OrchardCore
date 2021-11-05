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
            var manifest = new ResourceManifest();");

            const string minificationFileExtension = ".min";

            var assembliesFolderPath = new FileInfo(typeof(Resource).Assembly.Location).Directory.FullName;
            var resourcesFilePath = Path.Combine(assembliesFolderPath, "resources.json");
            using (var reader = new StreamReader(resourcesFilePath))
            {
                var count = 1;
                var content = reader.ReadToEnd();
                var resources = JsonSerializer.Deserialize<Resources>(content);

                for (var i = 0; i < resources.Scripts.Length; i++)
                {
                    var script = resources.Scripts.ElementAt(i);

                    sourceBuilder.AppendLine($@"
            var resource{count} = manifest
                .DefineScript(""{script.Name}"")
                .SetVersion(""{script.Version}"");");

                    if (script.Dependencies != null)
                    {
                        sourceBuilder.AppendLine($@"
            resource{count}.SetDependencies({String.Join(',', script.Dependencies.Select(d => "\"" + d + "\""))});");
                    }

                    if (!String.IsNullOrEmpty(script.Url))
                    {
                        if (script.Url.Contains(minificationFileExtension))
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{script.Url}"", ""{script.Url.Replace(minificationFileExtension, String.Empty)}"");");
                        }
                        else
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{script.Url}"");");
                        }
                    }

                    if (!String.IsNullOrEmpty(script.Cdn))
                    {
                        if (script.Cdn.Contains(minificationFileExtension))
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{script.Cdn}"", ""{script.Cdn.Replace(minificationFileExtension, String.Empty)}"");");
                        }
                        else
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{script.Cdn}"");");
                        }
                    }

                    if (script.CdnIntegrity != null)
                    {
                        sourceBuilder.AppendLine($@"
            resource{count}.SetCdnIntegrity({String.Join(',', script.CdnIntegrity.Select(c => "\"" + c + "\""))});");
                    }

                    ++count;
                }

                for (var i = 0; i < resources.Styles.Length; i++)
                {
                    var style = resources.Styles.ElementAt(i);

                    sourceBuilder.AppendLine($@"
            var resource{count} = manifest
                .DefineStyle(""{style.Name}"")
                .SetVersion(""{style.Version}"");");

                    if (style.Dependencies != null)
                    {
                        sourceBuilder.AppendLine($@"
            resource{count}.SetDependencies({String.Join(',', style.Dependencies.Select(d => "\"" + d + "\""))});");
                    }

                    if (!String.IsNullOrEmpty(style.Url))
                    {
                        if (style.Url.Contains(minificationFileExtension))
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{style.Url}"", ""{style.Url.Replace(minificationFileExtension, String.Empty)}"");");
                        }
                        else
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{style.Url}"");");
                        }
                    }

                    if (!String.IsNullOrEmpty(style.Cdn))
                    {
                        if (style.Cdn.Contains(minificationFileExtension))
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{style.Cdn}"", ""{style.Cdn.Replace(minificationFileExtension, String.Empty)}"");");
                        }
                        else
                        {
                            sourceBuilder.AppendLine($@"
            resource{count}.SetUrl(""{style.Cdn}"");");
                        }
                    }

                    if (style.CdnIntegrity != null)
                    {
                        sourceBuilder.AppendLine($@"
            resource{count}.SetCdnIntegrity({String.Join(',', style.CdnIntegrity.Select(c => "\"" + c + "\""))});");
                    }

                    ++count;
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
        }

        public void Initialize(GeneratorInitializationContext context)
        {

        }
    }
}
