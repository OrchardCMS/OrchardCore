
namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Computes the aggregates shared by aggregate steps, columns and aggregate formula functions. Missing values are ignored, so
/// the average of <c>2</c>, <c>4</c>, and a missing value is <c>3</c>.
/// </summary>
public static class DataAggregations
{
    /// <summary>
    /// Computes an aggregate over a set of values.
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="values">The values of the group.</param>
    /// <returns>The aggregated value, or <see langword="null"/> when nothing can be aggregated.</returns>
    public static object Compute(DataAggregate aggregate, IReadOnlyList<object> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count > 0 && values[0] is DataPartialAggregate)
        {
            return Merge(aggregate, values.OfType<DataPartialAggregate>().ToArray());
        }

        return aggregate switch
        {
            DataAggregate.Count => Count(values),
            DataAggregate.CountDistinct => CountDistinct(values),
            DataAggregate.Sum => Sum(values),
            DataAggregate.Average => Average(values),
            DataAggregate.Min => Min(values),
            DataAggregate.Max => Max(values),
            DataAggregate.Median => Median(values),
            _ => values.FirstOrDefault(value => value is not null),
        };
    }

    // Combines the aggregates a data source computed per group, as if the rows had been aggregated here.
    private static object Merge(DataAggregate aggregate, DataPartialAggregate[] parts)
    {
        switch (aggregate)
        {
            case DataAggregate.Count:
                return parts.Sum(part => part.Count);

            case DataAggregate.Sum:
                return Sum(parts.Select(part => part.Sum).ToArray());

            case DataAggregate.Average:
                {
                    var count = parts.Sum(part => part.Count);
                    var total = Sum(parts.Select(part => part.Sum).ToArray());

                    return count == 0 || DataValues.Coerce(total, DataFieldType.Decimal) is not decimal sum
                        ? null
                        : sum / count;
                }

            case DataAggregate.Min:
                return Min(parts.Select(part => part.Min).ToArray());

            case DataAggregate.Max:
                return Max(parts.Select(part => part.Max).ToArray());

            default:
                return null;
        }
    }

    /// <summary>
    /// Infers the type of an aggregate's result.
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="fieldType">The type of the aggregated field.</param>
    /// <returns>The result type.</returns>
    public static DataFieldType GetResultType(DataAggregate aggregate, DataFieldType fieldType)
    {
        return aggregate switch
        {
            DataAggregate.Count or DataAggregate.CountDistinct => DataFieldType.Integer,
            DataAggregate.Sum => fieldType == DataFieldType.Integer ? DataFieldType.Integer : DataFieldType.Decimal,
            DataAggregate.Average => DataFieldType.Decimal,
            DataAggregate.Median => DataValues.IsTemporal(fieldType) ? fieldType : DataFieldType.Decimal,
            _ => fieldType,
        };
    }

    /// <summary>
    /// Determines whether an aggregate can be applied to a field type.
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="fieldType">The type of the field.</param>
    /// <returns><see langword="true"/> when the aggregate supports the type.</returns>
    public static bool Supports(DataAggregate aggregate, DataFieldType fieldType)
    {
        return aggregate switch
        {
            DataAggregate.Sum or DataAggregate.Average => DataValues.IsNumeric(fieldType),
            DataAggregate.Median => DataValues.IsNumeric(fieldType) || DataValues.IsTemporal(fieldType),
            DataAggregate.Min or DataAggregate.Max => fieldType != DataFieldType.Boolean,
            _ => true,
        };
    }

    /// <summary>
    /// Counts the values that are present.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The count.</returns>
    public static long Count(IReadOnlyList<object> values)
    {
        long count = 0;

        foreach (var value in values)
        {
            if (value is not null)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Counts the distinct values that are present.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The count.</returns>
    public static long CountDistinct(IReadOnlyList<object> values)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var value in values)
        {
            if (value is not null)
            {
                keys.Add(DataValues.ToKey(value));
            }
        }

        return keys.Count;
    }

    /// <summary>
    /// Sums the numeric values. The result is a <see cref="long"/> when every value is whole and the sum fits.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The sum, or <see langword="null"/> when there are no numbers.</returns>
    public static object Sum(IReadOnlyList<object> values)
    {
        decimal total = 0;
        var any = false;
        var allWhole = true;

        foreach (var value in values)
        {
            if (value is null)
            {
                continue;
            }

            if (value is not long && value is not int)
            {
                allWhole = false;
            }

            var number = DataValues.Coerce(value, DataFieldType.Decimal);

            if (number is decimal amount)
            {
                try
                {
                    total += amount;
                    any = true;
                }
                catch (OverflowException)
                {
                    return null;
                }
            }
        }

        if (!any)
        {
            return null;
        }

        if (allWhole && total >= long.MinValue && total <= long.MaxValue)
        {
            return (long)total;
        }

        return total;
    }

    /// <summary>
    /// Averages the numeric values.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The average, or <see langword="null"/> when there are no numbers.</returns>
    public static object Average(IReadOnlyList<object> values)
    {
        decimal total = 0;
        long count = 0;

        foreach (var value in values)
        {
            if (DataValues.Coerce(value, DataFieldType.Decimal) is decimal number)
            {
                try
                {
                    total += number;
                }
                catch (OverflowException)
                {
                    return null;
                }

                count++;
            }
        }

        return count == 0 ? null : total / count;
    }

    /// <summary>
    /// Finds the smallest value present.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The smallest value, or <see langword="null"/> when none is present.</returns>
    public static object Min(IReadOnlyList<object> values)
    {
        object result = null;

        foreach (var value in values)
        {
            if (value is not null && (result is null || DataValues.Compare(value, result) < 0))
            {
                result = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Finds the largest value present.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The largest value, or <see langword="null"/> when none is present.</returns>
    public static object Max(IReadOnlyList<object> values)
    {
        object result = null;

        foreach (var value in values)
        {
            if (value is not null && (result is null || DataValues.Compare(value, result) > 0))
            {
                result = value;
            }
        }

        return result;
    }

    /// <summary>
    /// Finds the middle value. For an even number of numbers, it is the average of the two middle numbers; for an even
    /// number of dates, it is the earlier middle date.
    /// </summary>
    /// <param name="values">The values.</param>
    /// <returns>The median, or <see langword="null"/> when none is present.</returns>
    public static object Median(IReadOnlyList<object> values)
    {
        var present = values.Where(value => value is not null).ToList();

        if (present.Count == 0)
        {
            return null;
        }

        if (present.All(value => value is DateTime))
        {
            var dates = present.Cast<DateTime>().Order().ToList();

            return dates[(dates.Count - 1) / 2];
        }

        var numbers = present
            .Select(value => DataValues.Coerce(value, DataFieldType.Decimal))
            .OfType<decimal>()
            .Order()
            .ToList();

        if (numbers.Count == 0)
        {
            return null;
        }

        var middle = numbers.Count / 2;

        return numbers.Count % 2 == 1
            ? numbers[middle]
            : (numbers[middle - 1] + numbers[middle]) / 2;
    }
}
