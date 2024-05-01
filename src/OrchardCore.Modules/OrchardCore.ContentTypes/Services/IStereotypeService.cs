using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes.Services;

public interface IStereotypeService
{
    Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();
}
