using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;

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
            if(_builtInFeatures != null)
            {
                return _builtInFeatures;
            }

            var buildIntFeatures = new List<Feature>();

            if (_environment.ApplicationName != null)
            {
                var assemblyNames = new HashSet<string>();
                GetTransitiveAssemblyNames(_environment.ApplicationName, assemblyNames);
                var additionalAssemblies = assemblyNames.Select(x => Assembly.Load(new AssemblyName(x)));

                var extensionNames = _extensionManager.AvailableExtensions().Select(x => x.Id).ToArray();

                foreach (var additonalLib in additionalAssemblies)
                {
                    // Don't use an assembly that will be harvested as an extension
                    if(extensionNames.Contains(additonalLib.GetName().Name))
                    {
                        continue;
                    }

                    var feature = new Feature
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
                                .ToArray()
                    };

                    buildIntFeatures.Add(feature);
                }
            }

            _builtInFeatures = buildIntFeatures;
            return _builtInFeatures ;
        }
        private void GetTransitiveAssemblyNames(string assemblyName, HashSet<string> assemblyNames)
        {
            if(assemblyNames.Contains(assemblyName))
            {
                return;
            }

            assemblyNames.Add(assemblyName);
            var assembly = Assembly.Load(new AssemblyName(assemblyName));

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies().Where(x => x.Name.StartsWith("Orchard")))
            {
                GetTransitiveAssemblyNames(referencedAssembly.Name, assemblyNames);
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