using Microsoft.AspNet.Mvc;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.Descriptor.Models;
using OrchardVNext.Environment.Extensions;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.Environment.ShellBuilders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OrchardVNext.Environment.ShellBuilders
{
    /// <summary>
    /// Service at the host level to transform the cachable descriptor into the loadable blueprint.
    /// </summary>
    public interface ICompositionStrategy
    {
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// blueprint model the shell builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        ShellBlueprint Compose(ShellSettings settings, ShellDescriptor descriptor);
    }

    public class CompositionStrategy : ICompositionStrategy {
        private readonly IExtensionManager _extensionManager;

        public CompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public ShellBlueprint Compose(ShellSettings settings, ShellDescriptor descriptor) {
            Logger.Debug("Composing blueprint");

            var enabledFeatures = _extensionManager.EnabledFeatures(descriptor);
            var features = _extensionManager.LoadFeatures(enabledFeatures);

            if (descriptor.Features.Any(feature => feature.Name == "OrchardVNext.Framework"))
                features = BuiltinFeatures().Concat(features);

            var excludedTypes = GetExcludedTypes(features);

            var modules = BuildBlueprint(features, IsModule, BuildModule, excludedTypes);
            var dependencies = BuildBlueprint(features, IsDependency, (t, f) => BuildDependency(t, f, descriptor),
                excludedTypes);
            var controllers = BuildBlueprint(features, IsController, BuildController, excludedTypes);

            var result = new ShellBlueprint {
                Settings = settings,
                Descriptor = descriptor,
                Dependencies = dependencies.Concat(modules).ToArray(),
                Controllers = controllers,
            };

            Logger.Debug("Done composing blueprint");
            return result;
        }

        private static IEnumerable<string> GetExcludedTypes(IEnumerable<Feature> features) {
            var excludedTypes = new HashSet<string>();

            // Identify replaced types
            foreach (Feature feature in features) {
                foreach (Type type in feature.ExportedTypes) {
                    foreach (
                        OrchardSuppressDependencyAttribute replacedType in
                            type.GetTypeInfo().GetCustomAttributes(typeof (OrchardSuppressDependencyAttribute), false)) {
                        excludedTypes.Add(replacedType.FullName);
                    }
                }
            }

            return excludedTypes;
        }

        private static IEnumerable<Feature> BuiltinFeatures() {
            yield return new Feature {
                Descriptor = new FeatureDescriptor {
                    Id = "OrchardVNext.Framework",
                    Extension = new ExtensionDescriptor {
                        Id = "OrchardVNext.Framework"
                    }
                },
                ExportedTypes =
                    typeof (OrchardStarter).GetTypeInfo().Assembly.ExportedTypes
                        .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract)
                        .Except(new[] {typeof (DefaultOrchardHost)})
                        .ToArray()
            };
        }

        private static IEnumerable<T> BuildBlueprint<T>(
            IEnumerable<Feature> features,
            Func<Type, bool> predicate,
            Func<Type, Feature, T> selector,
            IEnumerable<string> excludedTypes) {

            // Load types excluding the replaced types
            return features.SelectMany(
                feature => feature.ExportedTypes
                    .Where(predicate)
                    .Where(type => !excludedTypes.Contains(type.FullName))
                    .Select(type => selector(type, feature)))
                .ToArray();
        }
        
        private static bool IsModule(Type type) {
            return typeof (IModule).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildModule(Type type, Feature feature) {
            return new DependencyBlueprint {
                Type = type,
                Feature = feature,
                Parameters = Enumerable.Empty<ShellParameter>()
            };
        }
        
        private static bool IsDependency(Type type) {
            return typeof (IDependency).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildDependency(Type type, Feature feature, ShellDescriptor descriptor) {
            return new DependencyBlueprint {
                Type = type,
                Feature = feature,
                Parameters = descriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
            };
        }

        private static bool IsController(Type type) {
            return typeof(Controller).IsAssignableFrom(type);
        }

        private static ControllerBlueprint BuildController(Type type, Feature feature) {
            var areaName = feature.Descriptor.Extension.Id;

            var controllerName = type.Name;
            if (controllerName.EndsWith("Controller"))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            return new ControllerBlueprint {
                Type = type,
                Feature = feature,
                AreaName = areaName,
                ControllerName = controllerName,
            };
        }
    }
}