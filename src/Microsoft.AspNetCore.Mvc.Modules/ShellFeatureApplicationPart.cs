using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
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
		private static object _synLock = new object();

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
                return nameof(ShellFeatureApplicationPart);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TypeInfo> Types
        {
            get
            {
                return ShellBlueprint.Dependencies.Keys.Select(type => type.GetTypeInfo());
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

				var referenceAssemblies = new HashSet<Assembly>();
				var extensionManager = HttpContextAccessor.HttpContext.RequestServices.GetRequiredService<IExtensionManager>();
				foreach (var extension in extensionManager.GetExtensions())
				{
					var extensionAssembly = Assembly.Load(new AssemblyName(extension.Id));
					PopulateAssemblies(extensionAssembly, referenceAssemblies);
				}

				_referencePaths = referenceAssemblies.Select(x => x.Location).ToArray();
			}

			return _referencePaths;
		}

		private void PopulateAssemblies(Assembly assembly, HashSet<Assembly> assemblies)
		{
			assemblies.Add(assembly);
			var loadContext = AssemblyLoadContext.GetLoadContext(assembly);
			var referencedAssemblyNames = assembly.GetReferencedAssemblies();

			foreach (var referencedAssemblyName in referencedAssemblyNames)
			{
				var referencedAssembly = loadContext.LoadFromAssemblyName(referencedAssemblyName);

				if (assemblies.Contains(referencedAssembly))
				{
					continue;
				}

				PopulateAssemblies(referencedAssembly, assemblies);
			}
		}
    }
} 