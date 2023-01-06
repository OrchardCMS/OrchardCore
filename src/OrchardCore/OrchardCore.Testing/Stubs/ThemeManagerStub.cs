using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Testing.Stubs;

public class ThemeManagerStub : IThemeManager
{
    private readonly IExtensionInfo _extensionInfo;

    public ThemeManagerStub(IExtensionInfo extensionInfo)
    {
        _extensionInfo = extensionInfo;
    }

    public Task<IExtensionInfo> GetThemeAsync() => Task.FromResult(_extensionInfo);
}
