using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement;

public interface IStereotypeService
{
    Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();
}
