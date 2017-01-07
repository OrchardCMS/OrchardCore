using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Builders.Models;
using Orchard.Environment.Shell.Descriptor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orchard.Environment.Shell.Builders
{
    public class CompositionStrategy : ICompositionStrategy
    {
        private readonly IExtensionManager _extensionManager;
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

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

        public async Task<ShellBlueprint> ComposeAsync(ShellSettings settings, ShellDescriptor descriptor)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Composing blueprint");
            }

            var features = await _extensionManager
                .LoadFeaturesAsync(descriptor.Features.Select(x => x.Id).ToArray());

            // Statup classes are the only types that are automatically added to the blueprint
            var dependencies = BuildBlueprint(features, IsStartup, BuildModule, Enumerable.Empty<string>());

            var uniqueDependencies = new Dictionary<Type, DependencyBlueprint>();

            foreach (var dependency in dependencies)
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

        private static IEnumerable<T> BuildBlueprint<T>(
            IEnumerable<FeatureEntry> features,
            Func<Type, bool> predicate,
            Func<Type, FeatureEntry, T> selector,
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

        private static bool IsStartup(Type type)
        {
            return typeof(Microsoft.AspNetCore.Modules.IStartup).IsAssignableFrom(type);
        }

        private static DependencyBlueprint BuildModule(Type type, FeatureEntry feature)
        {
            return new DependencyBlueprint
            {
                Type = type,
                Feature = feature.FeatureInfo,
                Parameters = Enumerable.Empty<ShellParameter>()
            };
        }

        private static DependencyBlueprint BuildDependency(Type type, FeatureEntry feature, ShellDescriptor descriptor)
        {
            return new DependencyBlueprint
            {
                Type = type,
                Feature = feature.FeatureInfo,
                Parameters = descriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
            };
        }
    }
}