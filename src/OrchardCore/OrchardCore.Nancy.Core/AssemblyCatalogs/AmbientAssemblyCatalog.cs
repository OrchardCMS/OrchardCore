using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Nancy.AssemblyCatalogs
{
    public class AmbientAssemblyCatalog : IAssemblyCatalog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientAssemblyCatalog"/> class.
        /// </summary>
        public AmbientAssemblyCatalog()
        {
        }

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public IReadOnlyCollection<Assembly> GetAssemblies()
        {
            var shellBluePrint = ShellScope.Services.GetRequiredService<ShellBlueprint>();
            return shellBluePrint.Dependencies.Keys
                .Select(type => type.Assembly)
                .Distinct()
                .ToArray();
        }
    }
}
