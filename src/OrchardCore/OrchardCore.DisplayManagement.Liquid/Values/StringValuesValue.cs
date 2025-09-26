using System.Globalization;
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
            return ToStringValue() == other.ToStringValue();
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
                    if (item != otherItem.ToStringValue())
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

    public override bool Contains(FluidValue value)
    {
        if (value is null || value.IsNil())
        {
            return false;
        }

        if (_stringValues.Count == 0)
        {
            return false;
        }

        var str = value.ToStringValue();
        return _stringValues.Contains(str);
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
                    return StringValue.Create(_stringValues[0]);
                }
                break;

            case "last":
                if (_stringValues.Count > 0)
                {
                    return StringValue.Create(_stringValues[_stringValues.Count - 1]);
                }
                break;
        }

        return NilValue.Instance;
    }

    protected override FluidValue GetIndex(FluidValue index, TemplateContext context)
    {
        var i = (int)index.ToNumberValue();

        if (i >= 0 && i < _stringValues.Count)
        {
            return StringValue.Create(_stringValues[i]);
        }

        return NilValue.Instance;
    }

    public override bool ToBooleanValue()
        => true;

    public override decimal ToNumberValue()
        => _stringValues.Count;

    public override object ToObjectValue()
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

    public override string ToStringValue()
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
        => _stringValues.Select(s => new StringValue(s)).ToArray();

    public override IEnumerable<FluidValue> Enumerate(TemplateContext context)
    {
        return Values;
    }

    [Obsolete("WriteTo is obsolete, prefer the WriteToAsync method.")]
    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
    {
        AssertWriteToParameters(writer, encoder, cultureInfo);

        if (_stringValues.Count == 0)
        {
            return;
        }
        else if (_stringValues.Count == 1)
        {
            writer.Write(_stringValues[0]);
        }
        else
        {
            foreach (var v in _stringValues)
            {
                writer.Write(v);
            }
        }
    }

    public override async ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
    {
        AssertWriteToParameters(writer, encoder, cultureInfo);

        if (_stringValues.Count == 0)
        {
            return;
        }
        else if (_stringValues.Count == 1)
        {
            await writer.WriteAsync(_stringValues[0]);
        }
        else
        {
            foreach (var v in _stringValues)
            {
                await writer.WriteAsync(v);
            }
        }
    }
}
