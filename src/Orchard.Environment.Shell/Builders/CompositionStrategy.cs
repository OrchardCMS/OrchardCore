using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Loader;
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
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private bool _builtinFeatureRegistered;

        public CompositionStrategy(
            IHostingEnvironment environment,
            IExtensionManager extensionManager,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<CompositionStrategy> logger)
        {
            _typeFeatureProvider = typeFeatureProvider;
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

            var uniqueDependencies = new Dictionary<Type, DependencyBlueprint>();

            foreach(var dependency in dependencies)
            {
                if(!uniqueDependencies.ContainsKey(dependency.Type))
                {
                    uniqueDependencies.Add(dependency.Type, dependency);
                }
            }

            foreach (var dependency in modules)
            {
                if (!uniqueDependencies.ContainsKey(dependency.Type))
                {
                    uniqueDependencies.Add(dependency.Type, dependency);
                }
            }

            var result = new ShellBlueprint
            {
                Settings = settings,
                Descriptor = descriptor,
                Dependencies = uniqueDependencies.Values
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
            var projectContext = ProjectContext.CreateContextForEachFramework("").FirstOrDefault();

            var additionalLibraries = projectContext.LibraryManager
                .GetLibraries()
                .Where(x => x.Identity.Name.StartsWith("Orchard"));

            var features = new List<Feature>();

            foreach (var additonalLib in additionalLibraries)
            {
                var assembly = Assembly.Load(new AssemblyName(additonalLib.Identity.Name));

                var feature = new Feature
                {
                    Descriptor = new FeatureDescriptor
                    {
                        Id = additonalLib.Identity.Name,
                        Extension = new ExtensionDescriptor
                        {
                            Id = additonalLib.Identity.Name
                        }
                    },
                    ExportedTypes =
                        assembly.ExportedTypes
                            .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                            //.Except(new[] { typeof(DefaultOrchardHost) })
                            .ToArray()
                };

                features.Add(feature);

                // Register built-in features in the type provider

                // TODO: Prevent this code from adding the services from modules as it's already added
                // by the extension loader.

                if (!_builtinFeatureRegistered)
                {
                    foreach (var type in feature.ExportedTypes)
                    {
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }
            }

            _builtinFeatureRegistered = true;

            return features;
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