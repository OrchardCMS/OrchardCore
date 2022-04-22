using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.ResourceManagement.Core;

namespace OrchardCore.Resources;

public class LocalResourceSettingProvider : IResourceSettingProvider
{
    private readonly IConfiguration _configuration;

    public LocalResourceSettingProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<ResourceSetting> GetAsync()
    {
        var setting = _configuration.GetSectionCompat("OrchardCore:Site_Settings").Get<ResourceSetting>();

        return Task.FromResult(setting);
    }
}
