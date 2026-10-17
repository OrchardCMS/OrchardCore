namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Computes one aggregate as the rows arrive, without keeping them, except for the distinct values of a distinct count
/// and the values of a median.
/// </summary>
internal sealed class DataAccumulator
{
    private readonly DataAggregate _function;
    private readonly HashSet<string> _distinct;
    private readonly List<object> _values;
    private long _count;
    private decimal _sum;
    private bool _hasSum;
    private object _extreme;

    public DataAccumulator(DataAggregate function)
    {
        _function = function;

        if (function == DataAggregate.CountDistinct)
        {
            _distinct = new HashSet<string>(StringComparer.Ordinal);
        }
        else if (function == DataAggregate.Median)
        {
            _values = [];
        }
    }

    /// <summary>
    /// Adds the value of a row.
    /// </summary>
    /// <returns>The estimated memory the accumulator took to remember the value.</returns>
    public long Add(object value)
    {
        if (DataValues.IsEmpty(value))
        {
            return 0;
        }

        _count++;

        switch (_function)
        {
            case DataAggregate.CountDistinct:
                var key = DataKeys.Build([value], [0], ignoreCase: false);

                return _distinct.Add(key) ? 48 + (2L * key.Length) : 0;

            case DataAggregate.Median:
                _values.Add(value);

                return 8 + DataRowSize.Estimate(value);

            case DataAggregate.Sum or DataAggregate.Average:
                if (DataValues.Coerce(value, DataFieldType.Decimal) is decimal number)
                {
                    _sum += number;
                    _hasSum = true;
                }

                break;

            case DataAggregate.Min:
                if (_extreme is null || DataValues.Compare(value, _extreme) < 0)
                {
                    _extreme = value;
                }

                break;

            case DataAggregate.Max:
                if (_extreme is null || DataValues.Compare(value, _extreme) > 0)
                {
                    _extreme = value;
                }

                break;
        }

        return 0;
    }

    public object Result()
    {
        return _function switch
        {
            DataAggregate.Count => _count,
            DataAggregate.CountDistinct => (long)_distinct.Count,
            DataAggregate.Sum => _hasSum ? _sum : null,
            DataAggregate.Average => _hasSum && _count > 0 ? _sum / _count : null,
            DataAggregate.Min or DataAggregate.Max => _extreme,
            DataAggregate.Median => DataAggregations.Median(_values),
            _ => null,
        };
    }
}
