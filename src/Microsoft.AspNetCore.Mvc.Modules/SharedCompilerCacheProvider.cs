using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Microsoft.AspNetCore.Mvc.Modules
{
	/// <summary>
	/// This implementation of <see cref="ICompilerCacheProvider"/> shares the same <see cref="ICompilerCache"/>
	/// instance across all tenants of the same application in order for the compiled view. Otherwise each
	/// tenant would get its own compiled view.
	/// </summary>
	public class SharedCompilerCacheProvider : ICompilerCacheProvider
	{
		private static ICompilerCache _cache;
		private static object _synLock = new object();

		public SharedCompilerCacheProvider(
			ApplicationPartManager applicationPartManager,
			IRazorViewEngineFileProviderAccessor fileProviderAccessor,
			IHostingEnvironment env)
		{
			lock (_synLock)
			{
				if (_cache == null)
				{
					var feature = new ViewsFeature();
					applicationPartManager.PopulateFeature(feature);

					// Applying ViewsFeatureProvider to gather any precompiled view
					var viewInfoContainerTypeName = GetViewInfoContainerType(new AssemblyName(env.ApplicationName));

					if (viewInfoContainerTypeName != null)
					{
						var viewContainer = (ViewInfoContainer)Activator.CreateInstance(viewInfoContainerTypeName);

						foreach (var item in viewContainer.ViewInfos)
						{
							feature.Views[item.Path] = item.Type;
						}
					}

					_cache = new CompilerCache(fileProviderAccessor.FileProvider, feature.Views);
				}
			}
		}

		/// <inheritdoc />
		public ICompilerCache Cache
		{
			get
			{
				return _cache;
			}
		}

		private Type GetViewInfoContainerType(AssemblyName applicationAssemblyName)
		{

			var applicationAssembly = Assembly.Load(applicationAssemblyName);
			
#if NETSTANDARD1_6
			var precompiledAssemblyFileName = applicationAssemblyName.Name
				+ ViewsFeatureProvider.PrecompiledViewsAssemblySuffix
				+ ".dll";
			var precompiledAssemblyFilePath = Path.Combine(
				Path.GetDirectoryName(applicationAssembly.Location),
				precompiledAssemblyFileName);

			if (File.Exists(precompiledAssemblyFilePath))
			{
				try
				{
					System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(precompiledAssemblyFilePath);
				}
				catch (FileLoadException)
				{
					// Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
				}
			}
#endif
			var precompiledAssemblyName = new AssemblyName(applicationAssemblyName.Name + ViewsFeatureProvider.PrecompiledViewsAssemblySuffix);

			var typeName = $"{ViewsFeatureProvider.ViewInfoContainerNamespace}.{ViewsFeatureProvider.ViewInfoContainerTypeName},{precompiledAssemblyName}";
			return Type.GetType(typeName);
		}
	}
}
