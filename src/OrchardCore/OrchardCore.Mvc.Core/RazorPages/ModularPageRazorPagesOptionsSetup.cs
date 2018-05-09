using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ModularPageRazorPagesOptionsSetup(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(RazorPagesOptions options)
        {
            options.RootDirectory = "/";
            options.Conventions.Add(new DefaultModularPageRouteModelConvention(_httpContextAccessor));
        }
    }
}