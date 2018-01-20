using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nancy;
using OrchardCore.Environment.Shell.Builders.Models;

namespace OrchardCore.Nancy.AssemblyCatalogs
{
    public class AmbientAssemblyCatalog : IAssemblyCatalog
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmbientAssemblyCatalog"/> class.
        /// </summary>
        public AmbientAssemblyCatalog(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets all <see cref="Assembly"/> instances in the catalog.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Assembly"/> instances.</returns>
        public IReadOnlyCollection<Assembly> GetAssemblies()
        {
            var shellBluePrint = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ShellBlueprint>();
            return shellBluePrint.Dependencies.Keys
                .Select(type => type.Assembly)
                .Distinct()
                .ToArray();
        }
    }
}
