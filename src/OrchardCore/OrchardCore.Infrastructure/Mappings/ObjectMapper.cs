using Riok.Mapperly.Abstractions;

namespace OrchardCore.Mappings;

/// <summary>
/// Represents a service for mapping objects.
/// </summary>
[Mapper]
public static partial class ObjectMapper
{
    public static partial TTarget Map<TSource, TTarget>(TSource source);
}
