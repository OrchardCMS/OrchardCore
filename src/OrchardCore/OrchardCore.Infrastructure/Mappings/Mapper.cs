using Mapster;

namespace OrchardCore.Mappings;

/// <summary>
/// Represents a service for mapping objects.
/// </summary>
public class Mapper : IMapper
{
    /// <inheritdoc/>
    public TDestination Map<TDestination>(object source) => TypeAdapter.Adapt<TDestination>(source);

    /// <inheritdoc/>
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => TypeAdapter<TSource, TDestination>.Map(source);
}
