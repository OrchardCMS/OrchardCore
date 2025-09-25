using YesSql.Filters.Abstractions.Services;

namespace YesSql.Filters.Enumerable;

/// <summary>
/// Represents a filter parser for an <see cref="IEnumerable{T}"/>
/// </summary>
public interface IEnumerableParser<T> : IFilterParser<EnumerableFilterResult<T>>
    where T : class { }
