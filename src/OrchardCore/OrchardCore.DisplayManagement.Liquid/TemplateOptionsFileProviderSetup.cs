using Fluid;
using Microsoft.Extensions.Options;

namespace OrchardCore.DisplayManagement.Liquid;

public sealed class TemplateOptionsFileProviderSetup : IConfigureOptions<TemplateOptionsBuilder>
{
    private readonly LiquidViewOptions _liquidViewOptions;

    public TemplateOptionsFileProviderSetup(IOptions<LiquidViewOptions> liquidViewOptions)
    {
        _liquidViewOptions = liquidViewOptions.Value;
    }

    public void Configure(TemplateOptionsBuilder options)
    {
        options.WithFileProvider(new FileProviderTemplateFileProvider(_liquidViewOptions.FileProvider));
    }
}
