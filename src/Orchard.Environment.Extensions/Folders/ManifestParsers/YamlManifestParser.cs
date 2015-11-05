using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders.ManifestParsers
{
    public class YamlManifestParser : IManifestParser
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

        public string Extension => ".txt";

        public ExtensionDescriptor GetDescriptorForExtension(string path, string extensionId, string extensionType, string manifestText)
        {
            var manifest = ParseManifest(manifestText);
            var extensionDescriptor = new ExtensionDescriptor
            {
                Location = path,
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

        private static Dictionary<string, string> ParseManifest(string manifestText)
        {
            var manifest = new Dictionary<string, string>();

            using (var reader = new StringReader(manifestText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var field = line.Split(new[] {":"}, 2, StringSplitOptions.None);
                    var fieldLength = field.Length;
                    if (fieldLength != 2)
                    {
                        continue;
                    }
                    for (var i = 0; i < fieldLength; i++)
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
            var defaultFeature = new FeatureDescriptor
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
            var featuresText = GetValue(manifest, FeaturesSection);
            if (featuresText != null)
            {
                FeatureDescriptor featureDescriptor = null;
                using (var reader = new StringReader(featuresText))
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

                            var featureDeclaration = line.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);
                            var featureDescriptorId = featureDeclaration[0].Trim();
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
                                var featureField = line.Split(new[] {":"}, 2, StringSplitOptions.None);
                                var featureFieldLength = featureField.Length;
                                if (featureFieldLength != 2)
                                {
                                    continue;
                                }
                                for (var i = 0; i < featureFieldLength; i++)
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
                                var message = string.Format("The line {0} in manifest for extension {1} was ignored", line, extensionDescriptor.Id);
                                throw new ArgumentException(message);
                            }
                        }
                        else
                        {
                            var message = string.Format("The line {0} in manifest for extension {1} was ignored", line, extensionDescriptor.Id);
                            throw new ArgumentException(message);
                        }
                    }

                    if (featureDescriptor != null && !featureDescriptor.Equals(defaultFeature))
                    {
                        featureDescriptors.Add(featureDescriptor);
                    }
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
            {
                return true;
            }

            return false;
        }

        private static bool IsFeatureDeclaration(string line)
        {
            var lineLength = line.Length;
            if (line.StartsWith("\t") && lineLength >= 2)
            {
                return !char.IsWhiteSpace(line[1]);
            }
            if (line.StartsWith("    ") && lineLength >= 5)
            {
                return !char.IsWhiteSpace(line[4]);
            }

            return false;
        }

        private static IEnumerable<string> ParseFeatureDependenciesEntry(string dependenciesEntry)
        {
            if (string.IsNullOrEmpty(dependenciesEntry))
            {
                return Enumerable.Empty<string>();
            }

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