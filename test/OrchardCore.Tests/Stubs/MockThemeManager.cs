using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Stubs.Tests
{
    public class MockThemeManager : IThemeManager
    {
        private IExtensionInfo _dec;
        public MockThemeManager(IExtensionInfo des)
        {
            _dec = des;
        }
        public Task<IExtensionInfo> GetThemeAsync()
        {
            return Task.FromResult(_dec);
        }
    }
}
