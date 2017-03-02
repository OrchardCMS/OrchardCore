using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Orchard.Environment.Extensions;
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
        private static IEnumerable<string> _referencePaths;
        private static IEnumerable<TypeInfo> _applicationTypes;
        private static object _synLock = new object();

        private readonly IHttpContextAccessor _httpContextAccessor;

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
                var shellBluePrint = _httpContextAccessor.HttpContext
                    .RequestServices.GetRequiredService<ShellBlueprint>();

                var extensionManager = _httpContextAccessor.HttpContext
                    .RequestServices.GetRequiredService<IExtensionManager>();

                var excludedTypes = extensionManager
                    .LoadFeaturesAsync().GetAwaiter().GetResult()
                    .Except(shellBluePrint.Dependencies.Values.Distinct())
                    .SelectMany(f => f.ExportedTypes)
                    .Select(type => type.GetTypeInfo());

                return GetApplicationTypes()
                    .Except(excludedTypes);
			}
        }

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
			if (_referencePaths != null)
			{
				return _referencePaths;
			}

			lock(_synLock)
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

        /// <inheritdoc />
        private IEnumerable<TypeInfo> GetApplicationTypes()
        {
            if (_applicationTypes != null)
            {
                return _applicationTypes;
            }

            lock (_synLock)
            {
                if (_applicationTypes != null)
                {
                    return _applicationTypes;
                }
                var hostingEnvironment = _httpContextAccessor.HttpContext
                    .RequestServices.GetRequiredService<IHostingEnvironment>();

                _applicationTypes = DefaultAssemblyPartDiscoveryProvider
                    .DiscoverAssemblyParts(hostingEnvironment.ApplicationName)
                    .Where(p => p is AssemblyPart)
                    .SelectMany(p => (p as AssemblyPart).Assembly.ExportedTypes)
                    .Select(type => type.GetTypeInfo());
            }

            return _applicationTypes;
        }
    }
}