
namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Builds the predicates that apply filters. Text comparisons ignore case. A date value written without a time
/// (<c>2026-01-31</c>) covers that whole day, so "on or before 2026-01-31" keeps the evening of the 31st.
/// </summary>
public static class DataFilterPredicates
{
    /// <summary>
    /// Builds the predicate of a filter.
    /// </summary>
    /// <param name="filterOperator">The comparison operator.</param>
    /// <param name="dataType">The type of the values compared.</param>
    /// <param name="rawValues">The filter values as invariant text.</param>
    /// <param name="today">The current date in the tenant time zone.</param>
    /// <returns>
    /// The predicate, or <see langword="null"/> when the filter is off because the operator needs values and none were
    /// given.
    /// </returns>
    public static Func<object, bool> Build(
        DataFilterOperator filterOperator,
        DataFieldType dataType,
        IList<string> rawValues,
        DateTime today)
    {
        var valueType = filterOperator is DataFilterOperator.InLastDays or DataFilterOperator.InNextDays
            ? DataFieldType.Integer
            : dataType;
        var values = (rawValues ?? [])
            .Select(raw => Parse(raw, valueType))
            .ToList();
        var present = values.Where(value => value.Value is not null).ToList();
        var first = present.FirstOrDefault();

        switch (filterOperator)
        {
            case DataFilterOperator.IsEmpty:
                return DataValues.IsEmpty;

            case DataFilterOperator.IsNotEmpty:
                return value => !DataValues.IsEmpty(value);

            case DataFilterOperator.Between:
                var lower = values.Count > 0 ? values[0] : default;
                var upper = values.Count > 1 ? values[1] : default;

                if (lower.Value is null && upper.Value is null)
                {
                    return null;
                }

                var lowerBound = LowerBound(lower);
                var upperBound = UpperBound(upper);

                return value => value is not null && lowerBound(value) && upperBound(value);
        }

        if (present.Count == 0)
        {
            return null;
        }

        switch (filterOperator)
        {
            case DataFilterOperator.Equals:
                return value => Matches(value, first);

            case DataFilterOperator.NotEquals:
                return value => !Matches(value, first);

            case DataFilterOperator.In:
                return value => present.Any(candidate => Matches(value, candidate));

            case DataFilterOperator.NotIn:
                return value => !present.Any(candidate => Matches(value, candidate));

            case DataFilterOperator.Contains:
                return value => Text(value)?.Contains(first.Text, StringComparison.OrdinalIgnoreCase) == true;

            case DataFilterOperator.NotContains:
                return value => Text(value)?.Contains(first.Text, StringComparison.OrdinalIgnoreCase) != true;

            case DataFilterOperator.StartsWith:
                return value => Text(value)?.StartsWith(first.Text, StringComparison.OrdinalIgnoreCase) == true;

            case DataFilterOperator.EndsWith:
                return value => Text(value)?.EndsWith(first.Text, StringComparison.OrdinalIgnoreCase) == true;

            case DataFilterOperator.GreaterThan:
                return first.IsDateOnly
                    ? value => value is not null && DataValues.Compare(value, NextDay(first.Value)) >= 0
                    : value => value is not null && DataValues.Compare(value, first.Value) > 0;

            case DataFilterOperator.GreaterThanOrEqual:
                return LowerBound(first);

            case DataFilterOperator.LessThan:
                return value => value is not null && DataValues.Compare(value, first.Value) < 0;

            case DataFilterOperator.LessThanOrEqual:
                return UpperBound(first);

            case DataFilterOperator.InLastDays:
            case DataFilterOperator.InNextDays:
                if (DataValues.Coerce(first.Text, DataFieldType.Integer) is not long days || days < 1 || days > 36_500)
                {
                    return null;
                }

                var start = filterOperator == DataFilterOperator.InLastDays ? today.Date.AddDays(1 - days) : today.Date;
                var end = filterOperator == DataFilterOperator.InLastDays ? today.Date.AddDays(1) : today.Date.AddDays(days);

                return value => value is DateTime date && date >= start && date < end;

            default:
                return null;
        }
    }

    /// <summary>
    /// Builds a condition a data source may apply while reading, or <see langword="null"/> when the filter cannot be
    /// passed down safely. A passed-down condition never removes a row the full filter would keep; the engine applies
    /// the full filter again afterwards.
    /// </summary>
    /// <param name="fieldName">The data set field name.</param>
    /// <param name="filterOperator">The comparison operator.</param>
    /// <param name="dataType">The type of the field.</param>
    /// <param name="rawValues">The filter values as invariant text.</param>
    /// <param name="toUtc">Converts a tenant-local date-time to UTC.</param>
    /// <param name="today">The tenant-local current date-time, which relative date filters count from.</param>
    /// <returns>The condition, or <see langword="null"/>.</returns>
    public static DataCondition BuildCondition(
        string fieldName,
        DataFilterOperator filterOperator,
        DataFieldType dataType,
        IList<string> rawValues,
        Func<DateTime, DateTime> toUtc,
        DateTime? today = null)
    {
        ArgumentNullException.ThrowIfNull(toUtc);

        if (filterOperator is DataFilterOperator.InLastDays or DataFilterOperator.InNextDays)
        {
            return RelativeCondition(fieldName, filterOperator, dataType, rawValues, toUtc, today);
        }

        var values = (rawValues ?? []).Select(raw => Parse(raw, dataType)).ToList();

        if (DataValues.IsTemporal(dataType))
        {
            object Convert(ParsedValue parsed, bool upper)
            {
                if (parsed.Value is not DateTime date)
                {
                    return null;
                }

                if (upper && parsed.IsDateOnly)
                {
                    date = date.AddDays(1);
                }

                return dataType == DataFieldType.DateTime ? toUtc(date) : date;
            }

            return filterOperator switch
            {
                DataFilterOperator.GreaterThan or DataFilterOperator.GreaterThanOrEqual when values.Count > 0 && values[0].Value is not null
                    => Condition(fieldName, DataFilterOperator.GreaterThanOrEqual, Convert(values[0], upper: false)),
                DataFilterOperator.LessThan or DataFilterOperator.LessThanOrEqual when values.Count > 0 && values[0].Value is not null
                    => Condition(fieldName, DataFilterOperator.LessThanOrEqual, Convert(values[0], upper: true)),
                DataFilterOperator.Between when values.Any(value => value.Value is not null)
                    => Condition(
                        fieldName,
                        DataFilterOperator.Between,
                        values.Count > 0 ? Convert(values[0], upper: false) : null,
                        values.Count > 1 ? Convert(values[1], upper: true) : null),
                _ => null,
            };
        }

        var present = values.Where(value => value.Value is not null).Select(value => value.Value).ToArray();

        return filterOperator switch
        {
            DataFilterOperator.Equals or DataFilterOperator.In when present.Length > 0
                => Condition(fieldName, DataFilterOperator.In, present),
            DataFilterOperator.Contains or DataFilterOperator.StartsWith or DataFilterOperator.EndsWith when dataType == DataFieldType.Text && present.Length > 0
                => Condition(fieldName, filterOperator, present[0]),
            DataFilterOperator.GreaterThan or
            DataFilterOperator.GreaterThanOrEqual or
            DataFilterOperator.LessThan or
            DataFilterOperator.LessThanOrEqual when DataValues.IsNumeric(dataType) && present.Length > 0
                => Condition(fieldName, filterOperator, present[0]),
            DataFilterOperator.Between when DataValues.IsNumeric(dataType) && present.Length > 0
                => Condition(fieldName, filterOperator, values.Count > 0 ? values[0].Value : null, values.Count > 1 ? values[1].Value : null),
            DataFilterOperator.IsNotEmpty => Condition(fieldName, filterOperator),
            _ => null,
        };
    }

    /// <summary>
    /// Builds the conditions that keep exactly the rows the filter keeps, for a data source that groups and aggregates
    /// itself that pushes grouping into its own store, where a loose condition would change the totals.
    /// </summary>
    /// <param name="fieldName">The data set field name.</param>
    /// <param name="filterOperator">The comparison operator.</param>
    /// <param name="dataType">The type of the field.</param>
    /// <param name="rawValues">The filter values as invariant text.</param>
    /// <param name="toUtc">Converts a tenant-local date-time to UTC.</param>
    /// <param name="today">The tenant-local current date-time.</param>
    /// <returns>
    /// The conditions, an empty list when the filter is off, or <see langword="null"/> when the filter cannot be
    /// expressed exactly (text matching such as Contains, or a date-only equality inside a list).
    /// </returns>
    public static IReadOnlyList<DataCondition> BuildExactConditions(
        string fieldName,
        DataFilterOperator filterOperator,
        DataFieldType dataType,
        IList<string> rawValues,
        Func<DateTime, DateTime> toUtc,
        DateTime today)
    {
        ArgumentNullException.ThrowIfNull(toUtc);

        if (Build(filterOperator, dataType, rawValues, today) is null)
        {
            return [];
        }

        var temporal = DataValues.IsTemporal(dataType);
        var values = (rawValues ?? []).Select(raw => Parse(raw, dataType)).ToList();
        var present = values.Where(value => value.Value is not null).ToList();
        var first = present.FirstOrDefault();

        object Bound(object value)
        {
            return dataType == DataFieldType.DateTime && value is DateTime date ? toUtc(date) : value;
        }

        DataCondition On(DataFilterOperator op, params object[] conditionValues)
        {
            return Condition(fieldName, op, conditionValues.Select(Bound).ToArray());
        }

        switch (filterOperator)
        {
            case DataFilterOperator.IsEmpty:
            case DataFilterOperator.IsNotEmpty:
                return [Condition(fieldName, filterOperator)];

            case DataFilterOperator.Between:
                {
                    var lower = values.Count > 0 ? values[0] : default;
                    var upper = values.Count > 1 ? values[1] : default;
                    var conditions = new List<DataCondition> { Condition(fieldName, DataFilterOperator.IsNotEmpty) };

                    if (lower.Value is not null)
                    {
                        conditions.Add(On(DataFilterOperator.GreaterThanOrEqual, lower.Value));
                    }

                    if (upper.Value is not null)
                    {
                        conditions.Add(upper.IsDateOnly
                            ? On(DataFilterOperator.LessThan, NextDay(upper.Value))
                            : On(DataFilterOperator.LessThanOrEqual, upper.Value));
                    }

                    return conditions;
                }

            case DataFilterOperator.Equals:
                return temporal && first.IsDateOnly
                    ? [On(DataFilterOperator.GreaterThanOrEqual, first.Value), On(DataFilterOperator.LessThan, NextDay(first.Value))]
                    : [On(DataFilterOperator.Equals, first.Value)];

            case DataFilterOperator.NotEquals:
            case DataFilterOperator.In:
            case DataFilterOperator.NotIn:
                if (present.Any(value => value.IsDateOnly))
                {
                    return null;
                }

                return filterOperator == DataFilterOperator.NotEquals
                    ? [On(DataFilterOperator.NotEquals, first.Value)]
                    : [On(filterOperator, present.Select(value => value.Value).ToArray())];

            case DataFilterOperator.GreaterThan:
                return first.IsDateOnly
                    ? [On(DataFilterOperator.GreaterThanOrEqual, NextDay(first.Value))]
                    : [On(DataFilterOperator.GreaterThan, first.Value)];

            case DataFilterOperator.GreaterThanOrEqual:
                return [On(DataFilterOperator.GreaterThanOrEqual, first.Value)];

            case DataFilterOperator.LessThan:
                return [On(DataFilterOperator.LessThan, first.Value)];

            case DataFilterOperator.LessThanOrEqual:
                return first.IsDateOnly
                    ? [On(DataFilterOperator.LessThan, NextDay(first.Value))]
                    : [On(DataFilterOperator.LessThanOrEqual, first.Value)];

            case DataFilterOperator.InLastDays:
            case DataFilterOperator.InNextDays:
                {
                    var days = (long)DataValues.Coerce(rawValues.First(value => !string.IsNullOrWhiteSpace(value)).Trim(), DataFieldType.Integer);
                    var start = filterOperator == DataFilterOperator.InLastDays ? today.Date.AddDays(1 - days) : today.Date;
                    var end = filterOperator == DataFilterOperator.InLastDays ? today.Date.AddDays(1) : today.Date.AddDays(days);

                    return [On(DataFilterOperator.GreaterThanOrEqual, start), On(DataFilterOperator.LessThan, end)];
                }

            default:
                return null;
        }
    }

    // The range of a relative date filter, as the same days the full filter keeps: from the start of the first day to the
    // start of the day after the last one, as an inclusive range, which keeps every row the filter keeps.
    private static DataCondition RelativeCondition(
        string fieldName,
        DataFilterOperator filterOperator,
        DataFieldType dataType,
        IList<string> rawValues,
        Func<DateTime, DateTime> toUtc,
        DateTime? today)
    {
        if (today is null ||
            !DataValues.IsTemporal(dataType) ||
            DataValues.Coerce(rawValues?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim(), DataFieldType.Integer) is not long days ||
            days < 1 ||
            days > 36_500)
        {
            return null;
        }

        var start = filterOperator == DataFilterOperator.InLastDays ? today.Value.Date.AddDays(1 - days) : today.Value.Date;
        var end = filterOperator == DataFilterOperator.InLastDays ? today.Value.Date.AddDays(1) : today.Value.Date.AddDays(days);

        return dataType == DataFieldType.DateTime
            ? Condition(fieldName, DataFilterOperator.Between, toUtc(start), toUtc(end))
            : Condition(fieldName, DataFilterOperator.Between, start, end);
    }

    private static DataCondition Condition(string fieldName, DataFilterOperator filterOperator, params object[] values)
    {
        return new DataCondition
        {
            Field = fieldName,
            Operator = filterOperator,
            Values = values,
        };
    }

    private static Func<object, bool> LowerBound(ParsedValue bound)
    {
        if (bound.Value is null)
        {
            return _ => true;
        }

        return value => value is not null && DataValues.Compare(value, bound.Value) >= 0;
    }

    private static Func<object, bool> UpperBound(ParsedValue bound)
    {
        if (bound.Value is null)
        {
            return _ => true;
        }

        if (bound.IsDateOnly)
        {
            var next = NextDay(bound.Value);

            return value => value is not null && DataValues.Compare(value, next) < 0;
        }

        return value => value is not null && DataValues.Compare(value, bound.Value) <= 0;
    }

    private static bool Matches(object value, ParsedValue candidate)
    {
        if (candidate.IsDateOnly && value is DateTime date && candidate.Value is DateTime day)
        {
            return date.Date == day.Date;
        }

        if (value is null)
        {
            return false;
        }

        return DataValues.AreEqual(value, candidate.Value);
    }

    private static object NextDay(object value)
    {
        return value is DateTime date ? date.Date.AddDays(1) : value;
    }

    private static string Text(object value)
    {
        return DataValues.ToText(value);
    }

    private static ParsedValue Parse(string raw, DataFieldType dataType)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return default;
        }

        var trimmed = raw.Trim();
        var value = DataValues.Coerce(trimmed, dataType);
        var isDateOnly = DataValues.IsTemporal(dataType) &&
            value is DateTime &&
            !trimmed.Contains(':', StringComparison.Ordinal);

        return new ParsedValue(value, trimmed, isDateOnly);
    }

    private readonly record struct ParsedValue(object Value, string Text, bool IsDateOnly);
}
