using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorPagesOptionsSetup : IConfigureOptions<RazorPagesOptions>
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ModularPageRazorPagesOptionsSetup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void Configure(RazorPagesOptions options)
        {
            options.RootDirectory = "/";
            options.Conventions.Add(new DefaultModularPageRouteModelConvention(_hostingEnvironment));
        }
    }
}