using Fluid;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class TemplateOptionsFileProviderSetup : IConfigureOptions<TemplateOptions>
    {
        private readonly LiquidViewOptions _liquidViewOptions;

        public TemplateOptionsFileProviderSetup(IOptions<LiquidViewOptions> liquidViewOptions)
        {
            _liquidViewOptions = liquidViewOptions.Value;
        }

        public void Configure(TemplateOptions options)
        {
            var fileProviders = _liquidViewOptions.FileProviders;

            if (fileProviders.Count == 0)
            {
                options.FileProvider = new NullFileProvider();
            }
            else if (fileProviders.Count == 1)
            {
                options.FileProvider = fileProviders[0];
            }
            else
            {
                options.FileProvider = new CompositeFileProvider(fileProviders);
            }
        }
    }
}
