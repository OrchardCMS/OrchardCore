using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement;

public interface IStereotypesProvider
{
    Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync();
}
