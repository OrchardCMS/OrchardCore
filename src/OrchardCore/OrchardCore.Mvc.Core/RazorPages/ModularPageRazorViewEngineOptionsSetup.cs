using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Mvc.RazorPages
{
    public class ModularPageRazorViewEngineOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ExtensionExpanderOptions> _optionsAccessor;

        public ModularPageRazorViewEngineOptionsSetup(
            IHostingEnvironment hostingEnvironment,
            IOptions<ExtensionExpanderOptions> optionsAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _optionsAccessor = optionsAccessor;
        }

        public void Configure(RazorViewEngineOptions options)
        {
            if (_hostingEnvironment.ContentRootFileProvider != null)
            {
                for (var i = 0; i < options.FileProviders.Count; i++)
                {
                    if (options.FileProviders[i] == _hostingEnvironment.ContentRootFileProvider)
                    {
                        options.FileProviders[i] = new ModularPageRazorFileProvider(
                            _hostingEnvironment.ContentRootFileProvider, _optionsAccessor);
                    }
                }
            }
        }
    }
}
