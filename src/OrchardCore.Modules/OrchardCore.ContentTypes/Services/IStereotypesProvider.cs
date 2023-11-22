using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes.Services;

public interface IStereotypesProvider
{
    Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();

    [Obsolete($"Instead, utilize the {nameof(GetStereotypesAsync)} method. This current method is slated for removal in upcoming releases.")]
    IEnumerable<StereotypeDescription> GetStereotypes()
    => GetStereotypesAsync().GetAwaiter().GetResult();
}
