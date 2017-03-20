using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using OrchardCore.Tenant.Builders.Models;

namespace OrchardCore.Mvc.Modules
{
	/// <summary>
	/// An <see cref="ApplicationPart"/> backed by an <see cref="Assembly"/>.
	/// </summary>
	public class TenantFeatureApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider,
        ICompilationReferencesProvider
    {
		private static IEnumerable<string> _referencePaths;
		private static object _synLock = new object();

		private readonly IHttpContextAccessor _httpContextAccessor;

		/// <summary>
		/// Initalizes a new <see cref="AssemblyPart"/> instance.
		/// </summary>
		/// <param name="assembly"></param>
		public TenantFeatureApplicationPart(IHttpContextAccessor httpContextAccessor)
        {
			_httpContextAccessor = httpContextAccessor;
		}

		public override string Name
        {
            get
            {
                return nameof(TenantFeatureApplicationPart);
            }
        }

        /// <inheritdoc />
        public IEnumerable<TypeInfo> Types
        {
            get
            {
				var tenantBluePrint = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<TenantBlueprint>();
				return tenantBluePrint.Dependencies.Keys.Select(type => type.GetTypeInfo());
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
    }
}