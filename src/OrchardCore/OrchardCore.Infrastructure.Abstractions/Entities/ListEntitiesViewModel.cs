using OrchardCore.Entities;

namespace OrchardCore.Infrastructure.Entities;

public class ListEntitiesViewModel<TEntity, TOptions> : ListEntitiesWithOptionsViewModel<TOptions>
{
    public IList<TEntity> Models { get; set; }
}

public class ListEntitiesViewModel : ListEntitiesViewModel<Entity, ModelOptions>
{
}
