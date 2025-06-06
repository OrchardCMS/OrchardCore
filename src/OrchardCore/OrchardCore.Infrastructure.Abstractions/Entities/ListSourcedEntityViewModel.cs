using OrchardCore.Entities;

namespace OrchardCore.Infrastructure.Entities;

public class ListSourcedEntityViewModel : ListEntitiesViewModel<ModelEntry<IEntity>, ModelOptions>
{
    public IEnumerable<string> Sources { get; set; }
}

public class ListSourcedEntityViewModel<TSource, TEntity, TOptions> : ListEntitiesViewModel<TEntity, TOptions>
{
    public IEnumerable<TSource> Sources { get; set; }
}

public class ListSourcedEntityViewModel<TSource, TEntity> : ListEntitiesViewModel<TEntity, ModelOptions>
{
    public IEnumerable<TSource> Sources { get; set; }
}

public class ListSourcedEntityViewModel<TSource> : ListEntitiesViewModel
{
    public IEnumerable<TSource> Sources { get; set; }
}

public class ListSourcedEntitiesViewModel<TSource, TEntity> : ListSourcedEntityViewModel<TSource>
{
}

public class ListSourcedEntitiesViewModel<TEntity> : ListSourcedEntityViewModel<string, ModelEntry<TEntity>>
{
}
