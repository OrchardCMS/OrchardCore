using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Orchard.Localization;
using Orchard.Environment.Extensions.Models;
using Microsoft.Extensions.Logging;
using Orchard.FileSystem;
using Orchard.Utility;

namespace Orchard.Environment.Extensions.Folders
{
    public class ExtensionHarvester : IExtensionHarvester
    {
        private const string NameSection = "name";
        private const string PathSection = "path";
        private const string DescriptionSection = "description";
        private const string VersionSection = "version";
        private const string OrchardVersionSection = "orchardversion";
        private const string AuthorSection = "author";
        private const string WebsiteSection = "website";
        private const string TagsSection = "tags";
        private const string AntiForgerySection = "antiforgery";
        private const string ZonesSection = "zones";
        private const string BaseThemeSection = "basetheme";
        private const string DependenciesSection = "dependencies";
        private const string CategorySection = "category";
        private const string FeatureDescriptionSection = "featuredescription";
        private const string FeatureNameSection = "featurename";
        private const string PrioritySection = "priority";
        private const string FeaturesSection = "features";
        private const string SessionStateSection = "sessionstate";

        private readonly IClientFolder _clientFolder;
        private readonly ILogger _logger;

        public ExtensionHarvester(IClientFolder clientFolder,
            ILoggerFactory loggerFactory)
        {
            _clientFolder = clientFolder;
            _logger = loggerFactory.CreateLogger<ExtensionHarvester>();

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional)
        {
            return paths
                .SelectMany(path => HarvestExtensions(path, extensionType, manifestName, manifestIsOptional))
                .ToList();
        }

        private IEnumerable<ExtensionDescriptor> HarvestExtensions(string path, string extensionType, string manifestName, bool manifestIsOptional)
        {
            return AvailableExtensionsInFolder(path, extensionType, manifestName, manifestIsOptional).ToReadOnlyCollection();
        }

        private List<ExtensionDescriptor> AvailableExtensionsInFolder(string path, string extensionType, string manifestName, bool manifestIsOptional)
        {
            _logger.LogInformation("Start looking for extensions in '{0}'...", path);
            var subfolderPaths = _clientFolder.ListDirectories(path);
            var localList = new List<ExtensionDescriptor>();
            foreach (var subfolderPath in subfolderPaths)
            {
                var extensionId = Path.GetFileName(subfolderPath);
                var manifestPath = Path.Combine(subfolderPath, manifestName);
                try
                {
                    var descriptor = GetExtensionDescriptor(path, extensionId, extensionType, manifestPath, manifestIsOptional);

                    if (descriptor == null)
                        continue;

                    if (descriptor.Path != null && !descriptor.Path.IsValidUrlSegment())
                    {
                        _logger.LogError("The module '{0}' could not be loaded because it has an invalid Path ({1}). It was ignored. The Path if specified must be a valid URL segment. The best bet is to stick with letters and numbers with no spaces.",
                                     extensionId,
                                     descriptor.Path);
                        continue;
                    }

                    if (descriptor.Path == null)
                    {
                        descriptor.Path = descriptor.Name.IsValidUrlSegment()
                                              ? descriptor.Name
                                              : descriptor.Id;
                    }

                    localList.Add(descriptor);
                }
                catch (Exception ex)
                {
                    // Ignore invalid module manifests
                    _logger.LogError(string.Format("The module '{0}' could not be loaded. It was ignored.", extensionId), ex);
                }
            }
            _logger.LogInformation("Done looking for extensions in '{0}': {1}", path, string.Join(", ", localList.Select(d => d.Id)));
            return localList;
        }

        public static ExtensionDescriptor GetDescriptorForExtension(string locationPath, string extensionId, string extensionType, string manifestText)
        {
            Dictionary<string, string> manifest = ParseManifest(manifestText);
            var extensionDescriptor = new ExtensionDescriptor
            {
                Location = locationPath,
                Id = extensionId,
                ExtensionType = extensionType,
                Name = GetValue(manifest, NameSection) ?? extensionId,
                Path = GetValue(manifest, PathSection),
                Description = GetValue(manifest, DescriptionSection),
                Version = GetValue(manifest, VersionSection),
                OrchardVersion = GetValue(manifest, OrchardVersionSection),
                Author = GetValue(manifest, AuthorSection),
                WebSite = GetValue(manifest, WebsiteSection),
                Tags = GetValue(manifest, TagsSection),
                AntiForgery = GetValue(manifest, AntiForgerySection),
                Zones = GetValue(manifest, ZonesSection),
                BaseTheme = GetValue(manifest, BaseThemeSection),
                SessionState = GetValue(manifest, SessionStateSection)
            };
            extensionDescriptor.Features = GetFeaturesForExtension(manifest, extensionDescriptor);

            return extensionDescriptor;
        }


        private ExtensionDescriptor GetExtensionDescriptor(string locationPath, string extensionId, string extensionType, string manifestPath, bool manifestIsOptional)
        {
            var manifestText = _clientFolder.ReadFile(manifestPath);
            if (manifestText == null)
            {
                if (manifestIsOptional)
                {
                    manifestText = string.Format("Id: {0}", extensionId);
                }
                else
                {
                    return null;
                }
            }

            return GetDescriptorForExtension(locationPath, extensionId, extensionType, manifestText);
        }

        private static Dictionary<string, string> ParseManifest(string manifestText)
        {
            var manifest = new Dictionary<string, string>();

            using (StringReader reader = new StringReader(manifestText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] field = line.Split(new[] { ":" }, 2, StringSplitOptions.None);
                    int fieldLength = field.Length;
                    if (fieldLength != 2)
                        continue;
                    for (int i = 0; i < fieldLength; i++)
                    {
                        field[i] = field[i].Trim();
                    }
                    switch (field[0].ToLowerInvariant())
                    {
                        case NameSection:
                            manifest.Add(NameSection, field[1]);
                            break;
                        case PathSection:
                            manifest.Add(PathSection, field[1]);
                            break;
                        case DescriptionSection:
                            manifest.Add(DescriptionSection, field[1]);
                            break;
                        case VersionSection:
                            manifest.Add(VersionSection, field[1]);
                            break;
                        case OrchardVersionSection:
                            manifest.Add(OrchardVersionSection, field[1]);
                            break;
                        case AuthorSection:
                            manifest.Add(AuthorSection, field[1]);
                            break;
                        case WebsiteSection:
                            manifest.Add(WebsiteSection, field[1]);
                            break;
                        case TagsSection:
                            manifest.Add(TagsSection, field[1]);
                            break;
                        case AntiForgerySection:
                            manifest.Add(AntiForgerySection, field[1]);
                            break;
                        case ZonesSection:
                            manifest.Add(ZonesSection, field[1]);
                            break;
                        case BaseThemeSection:
                            manifest.Add(BaseThemeSection, field[1]);
                            break;
                        case DependenciesSection:
                            manifest.Add(DependenciesSection, field[1]);
                            break;
                        case CategorySection:
                            manifest.Add(CategorySection, field[1]);
                            break;
                        case FeatureDescriptionSection:
                            manifest.Add(FeatureDescriptionSection, field[1]);
                            break;
                        case FeatureNameSection:
                            manifest.Add(FeatureNameSection, field[1]);
                            break;
                        case PrioritySection:
                            manifest.Add(PrioritySection, field[1]);
                            break;
                        case SessionStateSection:
                            manifest.Add(SessionStateSection, field[1]);
                            break;
                        case FeaturesSection:
                            manifest.Add(FeaturesSection, reader.ReadToEnd());
                            break;
                    }
                }
            }

            return manifest;
        }

        private static IEnumerable<FeatureDescriptor> GetFeaturesForExtension(IDictionary<string, string> manifest, ExtensionDescriptor extensionDescriptor)
        {
            var featureDescriptors = new List<FeatureDescriptor>();

            // Default feature
            FeatureDescriptor defaultFeature = new FeatureDescriptor
            {
                Id = extensionDescriptor.Id,
                Name = GetValue(manifest, FeatureNameSection) ?? extensionDescriptor.Name,
                Priority = GetValue(manifest, PrioritySection) != null ? int.Parse(GetValue(manifest, PrioritySection)) : 0,
                Description = GetValue(manifest, FeatureDescriptionSection) ?? GetValue(manifest, DescriptionSection) ?? string.Empty,
                Dependencies = ParseFeatureDependenciesEntry(GetValue(manifest, DependenciesSection)),
                Extension = extensionDescriptor,
                Category = GetValue(manifest, CategorySection)
            };

            featureDescriptors.Add(defaultFeature);

            // Remaining features
            string featuresText = GetValue(manifest, FeaturesSection);
            if (featuresText != null)
            {
                FeatureDescriptor featureDescriptor = null;
                using (StringReader reader = new StringReader(featuresText))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (IsFeatureDeclaration(line))
                        {
                            if (featureDescriptor != null)
                            {
                                if (!featureDescriptor.Equals(defaultFeature))
                                {
                                    featureDescriptors.Add(featureDescriptor);
                                }

                                featureDescriptor = null;
                            }

                            string[] featureDeclaration = line.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                            string featureDescriptorId = featureDeclaration[0].Trim();
                            if (string.Equals(featureDescriptorId, extensionDescriptor.Id, StringComparison.OrdinalIgnoreCase))
                            {
                                featureDescriptor = defaultFeature;
                                featureDescriptor.Name = extensionDescriptor.Name;
                            }
                            else
                            {
                                featureDescriptor = new FeatureDescriptor
                                {
                                    Id = featureDescriptorId,
                                    Extension = extensionDescriptor
                                };
                            }
                        }
                        else if (IsFeatureFieldDeclaration(line))
                        {
                            if (featureDescriptor != null)
                            {
                                string[] featureField = line.Split(new[] { ":" }, 2, StringSplitOptions.None);
                                int featureFieldLength = featureField.Length;
                                if (featureFieldLength != 2)
                                    continue;
                                for (int i = 0; i < featureFieldLength; i++)
                                {
                                    featureField[i] = featureField[i].Trim();
                                }

                                switch (featureField[0].ToLowerInvariant())
                                {
                                    case NameSection:
                                        featureDescriptor.Name = featureField[1];
                                        break;
                                    case DescriptionSection:
                                        featureDescriptor.Description = featureField[1];
                                        break;
                                    case CategorySection:
                                        featureDescriptor.Category = featureField[1];
                                        break;
                                    case PrioritySection:
                                        featureDescriptor.Priority = int.Parse(featureField[1]);
                                        break;
                                    case DependenciesSection:
                                        featureDescriptor.Dependencies = ParseFeatureDependenciesEntry(featureField[1]);
                                        break;
                                }
                            }
                            else
                            {
                                string message = string.Format("The line {0} in manifest for extension {1} was ignored", line, extensionDescriptor.Id);
                                throw new ArgumentException(message);
                            }
                        }
                        else
                        {
                            string message = string.Format("The line {0} in manifest for extension {1} was ignored", line, extensionDescriptor.Id);
                            throw new ArgumentException(message);
                        }
                    }

                    if (featureDescriptor != null && !featureDescriptor.Equals(defaultFeature))
                        featureDescriptors.Add(featureDescriptor);
                }
            }

            return featureDescriptors;
        }

        private static bool IsFeatureFieldDeclaration(string line)
        {
            if (line.StartsWith("\t\t") ||
                line.StartsWith("\t    ") ||
                line.StartsWith("    ") ||
                line.StartsWith("    \t"))
                return true;

            return false;
        }

        private static bool IsFeatureDeclaration(string line)
        {
            int lineLength = line.Length;
            if (line.StartsWith("\t") && lineLength >= 2)
            {
                return !Char.IsWhiteSpace(line[1]);
            }
            if (line.StartsWith("    ") && lineLength >= 5)
                return !Char.IsWhiteSpace(line[4]);

            return false;
        }

        private static IEnumerable<string> ParseFeatureDependenciesEntry(string dependenciesEntry)
        {
            if (string.IsNullOrEmpty(dependenciesEntry))
                return Enumerable.Empty<string>();

            var dependencies = new List<string>();
            foreach (var s in dependenciesEntry.Split(','))
            {
                dependencies.Add(s.Trim());
            }
            return dependencies;
        }

        private static string GetValue(IDictionary<string, string> fields, string key)
        {
            string value;
            return fields.TryGetValue(key, out value) ? value : null;
        }
    }
}