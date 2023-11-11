using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes.Services;

public interface IStereotypeService
{
    [Obsolete($"Instead, utilize the {nameof(GetStereotypesAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<StereotypeDescription> GetStereotypes();

    Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();
}
