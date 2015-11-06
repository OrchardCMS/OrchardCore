using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;
using Orchard.Localization;

namespace Orchard.Environment.Extensions.Folders.ManifestParsers
{
    public class JsonManifestParser : IManifestParser
    {
        private readonly IClientFolder _clientFolder;

        public JsonManifestParser(IClientFolder clientFolder)
        {
            _clientFolder = clientFolder;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string Extension => ".json";

        public ExtensionDescriptor GetDescriptorForExtension(string path, string extensionId, string extensionType, string manifestText)
        {
            dynamic project = ReadJsonFile(path, extensionId, "project.json");
            dynamic manifest = ReadJsonFile(path, extensionId, "orchard.json");

            JsonExtensionDescriptor jsonDescriptor = MapToJsonExtensionDescriptor(project, manifest, path, extensionId);

            return MapJsonToExtensionDescriptor(jsonDescriptor);
        }

        private dynamic ReadJsonFile(string folderPath, string extensionId, string fileName)
        {
            var file = _clientFolder.ReadFile(Path.Combine(folderPath, extensionId, fileName));

            if (file == null)
            {
                throw new OrchardException(T($"{fileName} is null"));
            }

            return JObject.Parse(file);
        }

        private JsonExtensionDescriptor MapToJsonExtensionDescriptor(dynamic project, dynamic manifest, string path, string extensionId)
        {
            var descriptor = new JsonExtensionDescriptor
            {
                Location = path,
                Id = extensionId,
                Version = project.version,
                Title = project.title,
                Description = project.description,
                Authors = project.authors?.ToObject<IEnumerable<string>>(),
                ProjectUrl = project.projectUrl,
                LicenseUrl = project.licenceUrl,
                Tags = project.tags?.ToObject<IEnumerable<string>>(),
                MinOrchardVersion = manifest.minOrchardVersion
            };

            if (manifest.module != null)
            {
                dynamic module = manifest.module;

                var extension = new JsonExtensionModule
                {
                    AntiForgery = module.antiForgery,
                    Path = module.path
                };

                if (module.features != null)
                {
                    var features = new List<JsonExtensionModuleFeature>();
                    foreach (var feature in module.features)
                    {
                        features.Add(new JsonExtensionModuleFeature
                        {
                            Id = feature.id,
                            Name = feature.name,
                            Description = feature.description,
                            Category = feature.category,
                            Priority = feature.priority,
                            Dependencies = feature.dependencies?.ToObject<IEnumerable<string>>()
                        });
                    }
                    extension.Features = features;
                }

                descriptor.Extension = extension;
            }
            else if (manifest.theme == null)
            {
                dynamic theme = manifest.theme;

                var extension = new JsonExtensionTheme
                {
                    BaseTheme = theme.baseTheme,
                    Zones = theme.zones?.ToObject<IEnumerable<string>>()
                };

                descriptor.Extension = extension;
            }
            else
            {
                throw new OrchardException(T("No module or theme defined in orchard.json"));
            }

            return descriptor;
        }

        private ExtensionDescriptor MapJsonToExtensionDescriptor(JsonExtensionDescriptor jsonDescriptor)
        {
            var descriptor = new ExtensionDescriptor
            {
                Location = jsonDescriptor.Location,
                Id = jsonDescriptor.Id,
                Version = jsonDescriptor.Version,
                Name = jsonDescriptor.Title,
                Description = jsonDescriptor.Description,
                Author = string.Join(",", jsonDescriptor.Authors), // todo make this a list
                WebSite = jsonDescriptor.ProjectUrl,
                //LicenceUrl = jsonDescriptor.LicenseUrl, // todo add
                Tags = string.Join(",", jsonDescriptor.Tags), // todo make this a list

                OrchardVersion = jsonDescriptor.MinOrchardVersion
            };

            if (jsonDescriptor.IsModule)
            {
                var module = (JsonExtensionModule) jsonDescriptor.Extension;

                descriptor.ExtensionType = DefaultExtensionTypes.Module;
                descriptor.Path = module.Path;
                descriptor.AntiForgery = module.AntiForgery ? "enabled" : "disabled";
                descriptor.Features = module.Features?.Select(x => new FeatureDescriptor
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Category = x.Category,
                    Priority = x.Priority,
                    Dependencies = x.Dependencies,
                    Extension = descriptor
                }).ToList();
            }
            else if (jsonDescriptor.IsTheme)
            {
                var theme = (JsonExtensionTheme) jsonDescriptor.Extension;

                descriptor.ExtensionType = DefaultExtensionTypes.Theme;
                descriptor.BaseTheme = theme.BaseTheme;
                descriptor.Zones = string.Join(",", theme.Zones); // todo make this a list
            }

            return descriptor;
        }
    }

    public class JsonExtensionDescriptor
    {
        public string Location { get; set; }
        public string Id { get; set; }

        public string Version { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public string ProjectUrl { get; set; }
        public string LicenseUrl { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string MinOrchardVersion { get; set; }
        public IJsonExtension Extension { get; set; }

        public bool IsModule => Extension is JsonExtensionModule;
        public bool IsTheme => Extension is JsonExtensionTheme;
    }

    public interface IJsonExtension {}

    public class JsonExtensionModule : IJsonExtension
    {
        public string Path { get; set; }
        public bool AntiForgery { get; set; }
        public IEnumerable<JsonExtensionModuleFeature> Features { get; set; }
    }

    public class JsonExtensionModuleFeature
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
        public IEnumerable<string> Dependencies { get; set; }
    }

    public class JsonExtensionTheme : IJsonExtension
    {
        public string BaseTheme { get; set; }
        public IEnumerable<string> Zones { get; set; }
    }
}