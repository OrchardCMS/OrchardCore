using System.Threading.Tasks;

namespace OrchardCore.ResourceManagement.Core;

public interface IResourceSettingProvider
{
    public Task<ResourceSetting> GetAsync();
}


