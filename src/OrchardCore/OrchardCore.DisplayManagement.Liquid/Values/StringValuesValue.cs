using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class StringValuesValue : FluidValue
{
    private readonly StringValues _stringValues;

    public StringValuesValue(StringValues stringValues)
    {
        _stringValues = stringValues;
    }

    public override FluidValues Type => FluidValues.Array;

    public override bool Equals(object obj)
    {
        return Equals(obj as FluidValue);
    }

    public override bool Equals(FluidValue other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.IsNil() || other.Type == FluidValues.Empty)
        {
            return _stringValues.Count == 0;
        }

        if (other is StringValuesValue svv)
        {
            return _stringValues.Equals(svv._stringValues);
        }
        else if (other.Type == FluidValues.String)
        {
            return ToStringValue(null) == other.ToStringValue(null);
        }
        else if (other is ArrayValue arrayValue)
        {
            if (_stringValues.Count != arrayValue.Values.Count)
            {
                return false;
            }

            for (var i = 0; i < _stringValues.Count; ++i)
            {
                var item = _stringValues[i];
                var otherItem = arrayValue.Values[i];

                if (otherItem.Type == FluidValues.String)
                {
                    if (item != otherItem.ToStringValue(null))
                    {
                        return false;
                    }
                }
                else if (otherItem.IsNil() || otherItem.Type == FluidValues.Empty)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override ValueTask<bool> ContainsAsync(FluidValue value, TemplateContext context)
    {
        if (value is null || value.IsNil())
        {
            return new ValueTask<bool>(false);
        }

        if (_stringValues.Count == 0)
        {
            return new ValueTask<bool>(false);
        }

        var str = value.ToStringValue(context);
        return new ValueTask<bool>(_stringValues.Contains(str));
    }

    public override int GetHashCode()
    {
        return _stringValues.GetHashCode();
    }

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        switch (name)
        {
            case "size":
                return NumberValue.Create(_stringValues.Count);

            case "first":
                if (_stringValues.Count > 0)
                {
                    return StringValue.Create(_stringValues[0] ?? string.Empty);
                }
                break;

            case "last":
                if (_stringValues.Count > 0)
                {
                    return StringValue.Create(_stringValues[_stringValues.Count - 1] ?? string.Empty);
                }
                break;
        }

        return NilValue.Instance;
    }

    public override ValueTask<FluidValue> GetIndexAsync(FluidValue index, TemplateContext context)
    {
        var i = (int)index.ToNumberValue(context);

        if (i >= 0 && i < _stringValues.Count)
        {
            return StringValue.Create(_stringValues[i] ?? string.Empty);
        }

        return NilValue.Instance;
    }

    public override bool ToBooleanValue(TemplateContext context)
        => true;

    public override decimal ToNumberValue(TemplateContext context)
        => _stringValues.Count;

    public override object ToObjectValue(TemplateContext context)
    {
        if (_stringValues.Count == 0)
        {
            return null;
        }
        else if (_stringValues.Count == 1)
        {
            return _stringValues[0];
        }

        return _stringValues.ToArray();
    }

    public override string ToStringValue(TemplateContext context)
    {
        if (_stringValues.Count == 0)
        {
            return string.Empty;
        }
        else if (_stringValues.Count == 1)
        {
            return _stringValues[0];
        }

        return string.Join("", _stringValues.ToArray());
    }

    public IReadOnlyList<FluidValue> Values
        => _stringValues.Select(s => StringValue.Create(s ?? string.Empty)).ToArray();

    public override async IAsyncEnumerable<FluidValue> EnumerateAsync(TemplateContext context)
    {
        await Task.CompletedTask;

        foreach (var value in _stringValues)
        {
            yield return StringValue.Create(value ?? string.Empty);
        }
    }

    public override ValueTask WriteToAsync(IFluidOutput output, TextEncoder encoder, TemplateContext context)
    {
        AssertWriteToParameters(output, encoder, context);

        foreach (var value in _stringValues)
        {
            output.Write(encoder.Encode(value));
        }

        return ValueTask.CompletedTask;
    }
}
