using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ResourceManagement.Core;

namespace OrchardCore.Resources;

public class LocalResourceConfigureOptions : IConfigureOptions<ResourceSetting>
{
    private readonly IConfiguration _configuration;

    public LocalResourceConfigureOptions(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(ResourceSetting options)
    {
        _configuration.GetSectionCompat("OrchardCore:Site_Settings").Bind(options);
    }

}
