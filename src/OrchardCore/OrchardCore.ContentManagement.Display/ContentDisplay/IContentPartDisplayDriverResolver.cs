using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverResolver
    {
        IReadOnlyList<IContentPartDisplayDriver> GetDisplayDrivers(string partName);
    }
}
