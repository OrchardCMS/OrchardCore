using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        private readonly IApplicationContext _applicationContext;

        public ModularPageRazorPagesOptionsSetup(IApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Configure(RazorPagesOptions options)
        {
            options.RootDirectory = "/";
            options.Conventions.Add(new DefaultModularPageRouteModelConvention(_applicationContext));
        }
    }
}