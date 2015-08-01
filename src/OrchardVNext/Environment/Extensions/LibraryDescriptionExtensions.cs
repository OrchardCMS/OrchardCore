using Microsoft.Dnx.Runtime;
using System.Linq;
using System.Reflection;

namespace OrchardVNext.Environment.Extensions {
    public static class LibraryDescriptionExtensions
    {
        public static Library ToLibrary(this LibraryDescription description) {
            return new Library(
                description.Identity.Name,
                description.Identity.Version?.ToString(),
                description.Path,
                description.Type,
                description.Dependencies.Select(d => d.Name),
                description.LoadableAssemblies.Select(a => new AssemblyName(a)));
        }
    }
}
