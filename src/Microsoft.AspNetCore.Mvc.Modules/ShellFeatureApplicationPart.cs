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
    public class ShellFeatureApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider,
        ICompilationReferencesProvider
    {
        internal static HashSet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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

        /// <summary>
        /// Initalizes a new <see cref="AssemblyPart"/> instance.
        /// </summary>
        /// <param name="assembly"></param>
        public ShellFeatureApplicationPart(IHttpContextAccessor httpContextAccessor)
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

        private IModularAssemblyProvider AssemblyProvider =>
            HttpContextAccessor.HttpContext.RequestServices.GetRequiredService<IModularAssemblyProvider>();

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
                return DefaultAssemblyDiscoveryProvider
                    .DiscoverAssemblies(
                        ShellBlueprint.Dependencies.Select(dep => dep.Type.GetTypeInfo().Assembly).ToList(),
                        ReferenceAssemblies)
                    .SelectMany(x => x.DefinedTypes);
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
            return AssemblyProvider.GetAssemblies().Select(x => x.Location);
        }
    }
} 