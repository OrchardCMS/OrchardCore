using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell.Builders.Models;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    /// <summary>
    /// An <see cref="ApplicationPart"/> implementing <see cref="IApplicationPartTypeProvider"/>.
    /// </summary>
    public class ShellFeatureApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider
    {
        private static IEnumerable<TypeInfo> _applicationTypes;
        private static object _syncLock = new object();

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initalizes a new <see cref="ShellFeatureApplicationPart"/> instance.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public ShellFeatureApplicationPart(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ShellBlueprint ShellBlueprint =>
            _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ShellBlueprint>();

        private IExtensionManager ExtensionManager =>
            _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IExtensionManager>();

        private IHostingEnvironment HostingEnvironment =>
            _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IHostingEnvironment>();

        /// <inheritdoc />
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
                return GetApplicationTypes().Union(ShellBlueprint.Types);
            }
        }

        /// <inheritdoc />
        private IEnumerable<TypeInfo> GetApplicationTypes()
        {
            if (_applicationTypes != null)
            {
                return _applicationTypes;
            }

            lock (_syncLock)
            {
                if (_applicationTypes == null)
                {
                    _applicationTypes = DefaultAssemblyPartDiscoveryProvider
                        .DiscoverAssemblyParts(HostingEnvironment.ApplicationName)
                        .SelectMany(p => (p as AssemblyPart).Assembly.ExportedTypes)
                        .Select(type => type.GetTypeInfo())
                        .Where(type => type.IsClass && !type.IsAbstract)
                        .Except(ExtensionManager.GetTypes());
                }
            }

            return _applicationTypes;
        }
    }
}