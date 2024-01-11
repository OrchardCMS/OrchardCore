using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Mvc
{
    /// <summary>
    /// An <see cref="ApplicationPart"/> backed by an <see cref="Assembly"/>.
    /// </summary>
    public class ShellFeatureApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider,
        ICompilationReferencesProvider
    {
        private static IEnumerable<string> _referencePaths;
        private static readonly object _synLock = new();
        private ShellBlueprint _shellBlueprint;
        private IEnumerable<ITagHelpersProvider> _tagHelpers;

        /// <summary>
        /// Initalizes a new <see cref="AssemblyPart"/> instance.
        /// </summary>
        public ShellFeatureApplicationPart()
        {
        }

        public override string Name
        {
            get
            {
                return nameof(ShellFeatureApplicationPart);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TypeInfo> Types
        {
            get
            {
                var services = ShellScope.Services;

                // The scope is null when this code is called through a 'ChangeToken' callback, e.g to recompile razor pages.
                // So, here we resolve and cache tenant level singletons, application singletons can be resolved in the ctor.

                if (services != null && _tagHelpers == null)
                {
                    lock (this)
                    {
                        if (_tagHelpers == null)
                        {
                            _shellBlueprint = services.GetRequiredService<ShellBlueprint>();
                            _tagHelpers = services.GetServices<ITagHelpersProvider>();
                        }
                    }
                }

                return _shellBlueprint
                    .Dependencies.Keys
                    .Concat(_tagHelpers.SelectMany(p => p.GetTypes()))
                    .Select(x => x.GetTypeInfo());
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
            if (_referencePaths != null)
            {
                return _referencePaths;
            }

            lock (_synLock)
            {
                if (_referencePaths != null)
                {
                    return _referencePaths;
                }

                _referencePaths = DependencyContext.Default.CompileLibraries
                    .SelectMany(library => library.ResolveReferencePaths());
            }

            return _referencePaths;
        }
    }
}
