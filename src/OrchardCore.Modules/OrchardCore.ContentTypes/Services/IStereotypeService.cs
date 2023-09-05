using System.Collections.Generic;

namespace OrchardCore.ContentTypes.Services;

public interface IStereotypeService
{
    IEnumerable<StereotypeDescription> GetStereotypes();
}
