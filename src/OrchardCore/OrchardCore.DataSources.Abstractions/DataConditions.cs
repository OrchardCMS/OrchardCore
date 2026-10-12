namespace OrchardCore.DataSources;

/// <summary>
/// Reads the conditions of a <see cref="DataSourceQuery"/> that a data source can apply while it loads its records,
/// such as in a database query. A data source may apply the conditions it understands and ignore the others: the
/// consumer applies every condition again to the rows it reads.
/// </summary>
public static class DataConditions
{
    /// <summary>
    /// Gets the values a consumer limits a field to with <see cref="DataFilterOperator.In"/> or
    /// <see cref="DataFilterOperator.Equals"/> conditions, such as the keys of the rows a join needs. When several
    /// conditions limit the field, only the values all of them allow are kept.
    /// </summary>
    /// <param name="conditions">The conditions of the query.</param>
    /// <param name="field">The name of the field.</param>
    /// <returns>The values, or <see langword="null"/> when no condition limits the field to a list of values.</returns>
    public static IReadOnlyList<object> Keys(IEnumerable<DataCondition> conditions, string field)
    {
        List<object> keys = null;

        foreach (var condition in Of(conditions, field))
        {
            if (condition.Operator is not (DataFilterOperator.In or DataFilterOperator.Equals))
            {
                continue;
            }

            var values = (condition.Values ?? []).Where(value => value is not null).Distinct().ToList();
            keys = keys is null ? values : keys.Where(values.Contains).ToList();
        }

        return keys;
    }

    /// <summary>
    /// Gets the range of dates the comparison conditions on a field allow, with inclusive bounds:
    /// <see cref="DataFilterOperator.GreaterThan"/>, <see cref="DataFilterOperator.GreaterThanOrEqual"/>,
    /// <see cref="DataFilterOperator.LessThan"/>, <see cref="DataFilterOperator.LessThanOrEqual"/> and
    /// <see cref="DataFilterOperator.Between"/>, whose open bound is <see langword="null"/>. A strict comparison gives
    /// an inclusive bound, which keeps a few more records that the consumer filters out.
    /// </summary>
    /// <param name="conditions">The conditions of the query.</param>
    /// <param name="field">The name of the field.</param>
    /// <returns>The narrowest range all the conditions allow; a missing bound is unlimited.</returns>
    public static (DateTime? From, DateTime? To) DateRange(IEnumerable<DataCondition> conditions, string field)
    {
        DateTime? from = null;
        DateTime? to = null;

        foreach (var condition in Of(conditions, field))
        {
            var values = condition.Values ?? [];
            var first = values.Count > 0 ? AsDate(values[0]) : null;
            var second = values.Count > 1 ? AsDate(values[1]) : null;

            var (lower, upper) = condition.Operator switch
            {
                DataFilterOperator.GreaterThan or DataFilterOperator.GreaterThanOrEqual => (first, null),
                DataFilterOperator.LessThan or DataFilterOperator.LessThanOrEqual => (null, first),
                DataFilterOperator.Between => (first, second),
                _ => ((DateTime?)null, (DateTime?)null),
            };

            if (lower is { } newFrom && (from is null || newFrom > from))
            {
                from = newFrom;
            }

            if (upper is { } newTo && (to is null || newTo < to))
            {
                to = newTo;
            }
        }

        return (from, to);
    }

    private static IEnumerable<DataCondition> Of(IEnumerable<DataCondition> conditions, string field)
        => (conditions ?? []).Where(condition => condition is not null && string.Equals(condition.Field, field, StringComparison.Ordinal));

    private static DateTime? AsDate(object value)
        => value switch
        {
            DateTime date => date,
            DateTimeOffset date => date.UtcDateTime,
            _ => null,
        };
}
