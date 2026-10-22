namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Computes one aggregate as the rows arrive, without keeping them, except for the distinct values of a distinct count
/// and the values of a median. Its result is the one <see cref="DataAggregations.Compute"/> computes over all the
/// values at once, including over the <see cref="DataPartialAggregate"/> a data source computed per group.
/// </summary>
internal sealed class DataAccumulator
{
    private readonly DataAggregate _function;
    private readonly HashSet<string> _distinct;
    private readonly List<object> _values;
    private long _count;
    private long _numbers;
    private decimal _sum;
    private bool _hasSum;
    private bool _wholeSum = true;
    private bool _overflow;
    private object _extreme;
    private object _first;
    private bool _partial;

    // Like DataAggregations.Sum, a sum is a whole number when every value is one and the sum fits.
    private bool IsWholeSum => _wholeSum && _sum >= long.MinValue && _sum <= long.MaxValue;

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
        if (value is null)
        {
            return 0;
        }

        if (value is DataPartialAggregate part)
        {
            AddPartial(part);

            return 0;
        }

        _count++;
        _first ??= value;

        switch (_function)
        {
            case DataAggregate.CountDistinct:
                var key = DataKeys.Build([value], [0], ignoreCase: false);

                return _distinct.Add(key) ? 48 + (2L * key.Length) : 0;

            case DataAggregate.Median:
                _values.Add(value);

                return 8 + DataRowSize.Estimate(value);

            case DataAggregate.Sum or DataAggregate.Average:
                AddNumber(value);

                break;

            case DataAggregate.Min or DataAggregate.Max:
                AddExtreme(value);

                break;
        }

        return 0;
    }

    public object Result()
    {
        if (_partial)
        {
            return _function switch
            {
                DataAggregate.Count => _count,
                DataAggregate.Sum when _overflow || !_hasSum => null,
                DataAggregate.Sum when IsWholeSum => (long)_sum,
                DataAggregate.Sum => _sum,
                DataAggregate.Average => _overflow || !_hasSum || _count == 0 ? null : _sum / _count,
                DataAggregate.Min or DataAggregate.Max => _extreme,
                _ => null,
            };
        }

        return _function switch
        {
            DataAggregate.Count => _count,
            DataAggregate.CountDistinct => (long)_distinct.Count,
            DataAggregate.Sum when _overflow || !_hasSum => null,
            DataAggregate.Sum when IsWholeSum => (long)_sum,
            DataAggregate.Sum => _sum,
            DataAggregate.Average => _overflow || _numbers == 0 ? null : _sum / _numbers,
            DataAggregate.Min or DataAggregate.Max => _extreme,
            DataAggregate.Median => DataAggregations.Median(_values),
            _ => _first,
        };
    }

    // A data source counted, summed and compared the rows of a group itself: combine them as if the rows had arrived.
    private void AddPartial(DataPartialAggregate part)
    {
        _partial = true;
        _count += part.Count;

        if (part.Sum is not null)
        {
            AddNumber(part.Sum);
        }

        var extreme = _function == DataAggregate.Max ? part.Max : part.Min;

        if (extreme is not null)
        {
            AddExtreme(extreme);
        }
    }

    private void AddNumber(object value)
    {
        // Like DataAggregations.Sum, a value that is not a whole number makes the sum a decimal, even one that is not a
        // number at all.
        if (value is not long and not int)
        {
            _wholeSum = false;
        }

        if (_overflow || DataValues.Coerce(value, DataFieldType.Decimal) is not decimal number)
        {
            return;
        }

        try
        {
            _sum += number;
            _hasSum = true;
            _numbers++;
        }
        catch (OverflowException)
        {
            _overflow = true;
        }
    }

    private void AddExtreme(object value)
    {
        var comparison = _extreme is null ? 0 : DataValues.Compare(value, _extreme);

        if (_extreme is null ||
            (_function == DataAggregate.Min && comparison < 0) ||
            (_function == DataAggregate.Max && comparison > 0))
        {
            _extreme = value;
        }
    }
}
