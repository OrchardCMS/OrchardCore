using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using OrchardCore.DisplayManagement.TagHelpers;
using OrchardCore.Environment.Shell.Builders.Models;
using Microsoft.Extensions.DependencyModel.Resolution;

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
        private static object _synLock = new object();

        private readonly IHttpContextAccessor _httpContextAccessor;

        private ShellBlueprint _shellBlueprint;
        private IEnumerable<ITagHelpersProvider> _tagHelpers;

        /// <summary>
        /// Initalizes a new <see cref="AssemblyPart"/> instance.
        /// </summary>
        /// <param name="assembly"></param>
        public ShellFeatureApplicationPart(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
                var services = _httpContextAccessor.HttpContext?.RequestServices;

                // 'HttpContext' is null when this code is called through a 'ChangeToken' callback, e.g to recompile razor pages.
                // So, here we resolve and cache tenant level singletons, application singletons are resolved in the constructor.

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

                var test1 = "C:/Program Files/dotnet/packs/Microsoft.AspNetCore.App.Ref/3.0.0-preview4-19172-03/ref/netcoreapp3.0";
                var test2 = "C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/3.0.0-preview4-27522-09/ref/netcoreapp3.0";

                _referencePaths = DependencyContext.Default.CompileLibraries
                    .SelectMany(library =>
                    {
                        if (library.Type != "referenceassembly")
                        {
                            return library.ResolveReferencePaths();
                        }

                        var paths = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

                        foreach (var assembly in library.Assemblies)
                        {
                            var path = Path.Combine(test1, assembly);

                            if (File.Exists(path))
                            {
                                paths.Add(path);
                            }
                            else
                            {
                                path = Path.Combine(test2, assembly);

                                if (File.Exists(path))
                                {
                                    paths.Add(path);
                                }
                                else
                                {
                                    ;
                                }
                            }
                        }

                        return paths;
                    });
            }

            return _referencePaths;
        }
    }
}