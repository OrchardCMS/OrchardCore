using System.Threading.Tasks;

namespace OrchardCore.ResourceManagement.Abstractions;

public interface IResourceSettingProvider
{
    public Task<ResourceSetting> GetAsync();
}
