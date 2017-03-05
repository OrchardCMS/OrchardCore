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
    /// An <see cref="ApplicationPart"/> which implements <see cref="IApplicationPartTypeProvider"/>.
    /// </summary>
    public class ShellFeatureApplicationPart :
        ApplicationPart,
        IApplicationPartTypeProvider
    {
        private static IEnumerable<TypeInfo> _applicationTypes;
        private static object _synLock = new object();

        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initalizes a new <see cref="ShellFeatureApplicationPart"/> instance.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public ShellFeatureApplicationPart(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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
                    .OfType<AssemblyPart>()
                    .SelectMany(p => p.Types);
            }

            return _applicationTypes;
        }
    }
}