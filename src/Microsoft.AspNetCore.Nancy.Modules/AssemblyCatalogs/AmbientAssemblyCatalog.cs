using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using Orchard.Environment.Shell.Builders.Models;

namespace Microsoft.AspNetCore.Nancy.Modules.AssemblyCatalogs
{
    public class AmbientAssemblyCatalog : IAssemblyCatalog
    {
        internal static HashSet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Nancy"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientAssemblyCatalog"/> class.
        /// </summary>
        public AmbientAssemblyCatalog(IHttpContextAccessor httpContextAccessor)
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

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public IReadOnlyCollection<Assembly> GetAssemblies()
        {
            return DefaultAssemblyDiscoveryProvider
                    .DiscoverAssemblies(
                        ShellBlueprint.Dependencies.Select(dep => dep.Type.GetTypeInfo().Assembly).Distinct().ToList(),
                        ReferenceAssemblies)
                    .ToArray();
        }
    }
}
