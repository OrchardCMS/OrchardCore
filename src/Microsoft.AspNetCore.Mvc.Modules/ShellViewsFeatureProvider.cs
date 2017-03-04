using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    /// <summary>
    /// An <see cref="IApplicationFeatureProvider{TFeature}"/> for <see cref="ViewsFeature"/>.
    /// </summary>
    public class ShellViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

         public ShellViewsFeatureProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
		}

        /// <inheritdoc />
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            var hostingEnvironment = _httpContextAccessor.HttpContext
                .RequestServices.GetRequiredService<IHostingEnvironment>();

            new ViewsFeatureProvider().PopulateFeature(
                new AssemblyPart[]
                {
                    new AssemblyPart(Assembly.Load(new AssemblyName(hostingEnvironment.ApplicationName)))
                },
                feature);
        }
    }
}