namespace OrchardCore.Mappings;

/// <summary>
/// Represents a contract for mapping objects.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps the specified source object to a new object of the specified destination type.
    /// </summary>
    /// <typeparam name="TDestination">The type of object to map to.</typeparam>
    /// <param name="source">The object to be mapped.</param>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps the specified source object to the specified destination object.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDestination"></typeparam>
    /// <param name="source">The object to be mapped.</param>
    /// <param name="destination">The object to map to.</param>
    /// <returns></returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
