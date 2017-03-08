using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
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
                return ShellBlueprint.Dependencies.Keys.Select(type => type.GetTypeInfo());
            }
        }
    }
}