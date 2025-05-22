using OrchardCore.Entities;

namespace OrchardCore.Search.Models;

public abstract class IndexSettingsBase : Entity
{
    public string IndexName { get; set; }
}
