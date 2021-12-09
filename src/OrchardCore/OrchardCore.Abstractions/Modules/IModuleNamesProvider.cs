using System.Collections.Generic;

namespace OrchardCore.Modules
{
    public interface IModuleNamesProvider
    {
        IEnumerable<string> GetModuleNames();
    }
}
