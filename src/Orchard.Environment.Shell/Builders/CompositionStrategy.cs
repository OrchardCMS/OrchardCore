using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell.Descriptor.Models;

namespace Orchard.Environment.Shell.Builders
{
    public class CompositionStrategy : ICompositionStrategy
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private IList<Feature> _builtInFeatures;

        public CompositionStrategy(
            IHostingEnvironment environment,
            IExtensionManager extensionManager,
            ILogger<CompositionStrategy> logger)
        {
            _environment = environment;
            _extensionManager = extensionManager;
            _logger = logger;
        }

        public ShellBlueprint Compose(ShellSettings settings, ShellDescriptor descriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Composing blueprint");
            }

            var enabledFeatures = _extensionManager.EnabledFeatures(descriptor);
            var features = _extensionManager.LoadFeatures(enabledFeatures);

            // Requiring "Orchard.Hosting" is a shortcut for adding all referenced
            // assemblies as features
            if (descriptor.Features.Any(feature => feature.Name == "Orchard.Hosting"))
            {
                features = BuiltinFeatures().Concat(features);
            }

            var excludedTypes = GetExcludedTypes(features);

            var modules = BuildBlueprint(features, IsModule, BuildModule, excludedTypes);
            var dependencies = BuildBlueprint(features, IsDependency, (t, f) => BuildDependency(t, f, descriptor),
                excludedTypes);

            var result = new ShellBlueprint
            {
                Settings = settings,
                Descriptor = descriptor,
                Dependencies = dependencies.Concat(modules).ToArray()
            };

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Done composing blueprint");
            }
            return result;
        }

        private static IEnumerable<string> GetExcludedTypes(IEnumerable<Feature> features)
        {
            var excludedTypes = new HashSet<string>();

            // Identify replaced types
            foreach (Feature feature in features)
            {
                foreach (Type type in feature.ExportedTypes)
                {
                    foreach (
                        OrchardSuppressDependencyAttribute replacedType in
                            type.GetTypeInfo().GetCustomAttributes(typeof(OrchardSuppressDependencyAttribute), false))
                    {
                        excludedTypes.Add(replacedType.FullName);
                    }
                }
            }

            return excludedTypes;
        }

        private IEnumerable<Feature> BuiltinFeatures()
        {
            var projectContext = ProjectContext.CreateContextForEachFramework(_environment.ApplicationName).FirstOrDefault();

            var additionalLibraries = projectContext.LibraryManager
                .GetLibraries()
                .Where(x => x.Identity.Name.StartsWith("Orchard"))
                .Select(x => Assembly.Load(new AssemblyName(x.Identity.Name)));

            foreach (var additonalLib in additionalLibraries)
            {
                yield return new Feature
                {
                    Descriptor = new FeatureDescriptor
                    {
                        Id = additonalLib.GetName().Name,
                        Extension = new ExtensionDescriptor
                        {
                            Id = additonalLib.GetName().Name
                        }
                    },
                    ExportedTypes =
                        additonalLib.ExportedTypes
                            .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                            //.Except(new[] { typeof(DefaultOrchardHost) })
                            .ToArray()
                };
            }
        }

        private static IEnumerable<T> BuildBlueprint<T>(
            IEnumerable<Feature> features,
            Func<Type, bool> predicate,
            Func<Type, Feature, T> selector,
            IEnumerable<string> excludedTypes)
        {
            // Load types excluding the replaced types
            return features.SelectMany(
                feature => feature.ExportedTypes
                    .Where(predicate)
                    .Where(type => !excludedTypes.Contains(type.FullName))
                    .Select(type => selector(type, feature)))
                .ToArray();
        }

        private static bool IsModule(Type type)
        {
            return typeof(IModule).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildModule(Type type, Feature feature)
        {
            return new DependencyBlueprint
            {
                Type = type,
                Feature = feature,
                Parameters = Enumerable.Empty<ShellParameter>()
            };
        }

        private static bool IsDependency(Type type)
        {
            return
                typeof(IDependency).IsAssignableFrom(type) ||
                type.GetTypeInfo().GetCustomAttribute<ServiceScopeAttribute>() != null;
        }

        private static DependencyBlueprint BuildDependency(Type type, Feature feature, ShellDescriptor descriptor)
        {
            return new DependencyBlueprint
            {
                Type = type,
                Feature = feature,
                Parameters = descriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
            };
        }
    }
}