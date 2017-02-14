using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell.Builders.Models;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    /// <summary>
    /// An <see cref="ApplicationPart"/> backed by an <see cref="Assembly"/>.
    /// </summary>
    public class SatelliteApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider,
        ICompilationReferencesProvider
    {
        internal static ISet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft.AspNetCore.Mvc",
            "Microsoft.AspNetCore.Mvc.Abstractions",
            "Microsoft.AspNetCore.Mvc.ApiExplorer",
            "Microsoft.AspNetCore.Mvc.Core",
            "Microsoft.AspNetCore.Mvc.Cors",
            "Microsoft.AspNetCore.Mvc.DataAnnotations",
            "Microsoft.AspNetCore.Mvc.Formatters.Json",
            "Microsoft.AspNetCore.Mvc.Formatters.Xml",
            "Microsoft.AspNetCore.Mvc.Localization",
            "Microsoft.AspNetCore.Mvc.Razor",
            "Microsoft.AspNetCore.Mvc.Razor.Host",
            "Microsoft.AspNetCore.Mvc.RazorPages",
            "Microsoft.AspNetCore.Mvc.TagHelpers",
            "Microsoft.AspNetCore.Mvc.ViewFeatures"
        };

        private bool isInitialized = false;

        private IList<TypeInfo> _cachedTypes;
        private IList<string> _cachedLocation;

        /// <summary>
        /// Initalizes a new <see cref="AssemblyPart"/> instance.
        /// </summary>
        /// <param name="assembly"></param>
        public SatelliteApplicationPart(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            HttpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the <see cref="IHttpContextAccessor"/> of the <see cref="ApplicationPart"/>.
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; }

        private ShellBlueprint ShellBlueprint =>
            HttpContextAccessor.HttpContext.RequestServices.GetRequiredService<ShellBlueprint>();

        public override string Name
        {
            get
            {
                return typeof(ShellFeatureApplicationPart).GetTypeInfo().Assembly.GetName().Name;
            }
        }

        /// <inheritdoc />
        public IEnumerable<TypeInfo> Types
        {
            get
            {
                EnsureInitialized();

                return _cachedTypes;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
            EnsureInitialized();

            return _cachedLocation;
        }

        private void EnsureInitialized() {
            if (isInitialized)
            {
                return;
            }

            var shellAssemblies = ShellBlueprint.Dependencies.Keys.Select(type => type.GetTypeInfo().Assembly).ToList();
            var discoveredAssemblies = DefaultAssemblyDiscoveryProvider
                .DiscoverAssemblies(
                    shellAssemblies,
                    ReferenceAssemblies);
            var candidateAssemblies = discoveredAssemblies.Except(shellAssemblies);

            _cachedTypes = candidateAssemblies
                .SelectMany(x => x.ExportedTypes)
                .Select(x => x.GetTypeInfo())
                .ToList();

            _cachedLocation = candidateAssemblies.Select(x => x.Location).ToList();
        }
    }
} 