using Microsoft.Extensions.Options;
using Orchard.Environment.Shell;

namespace Orchard.Tests.Stubs
{
    public class StubShellOptions : IOptions<ShellOptions>
    {
        private ShellOptions _option;
        public StubShellOptions(string root, string container)
        {
            _option = new ShellOptions { ShellsRootContainerName = root, ShellsContainerName = container };
        }

        public ShellOptions Value
        {
            get
            {
                return _option;
            }
        }
    }
}
