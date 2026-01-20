using System.Dynamic;
using System.Globalization;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace System.Text.Json.Dynamic;

#nullable enable

[JsonConverter(typeof(JsonDynamicJsonConverter<JsonDynamicValue>))]
public sealed class JsonDynamicValue : JsonDynamicBase, IComparable, IComparable<JsonDynamicValue>, IConvertible, IEquatable<JsonDynamicValue>
{
    private readonly JsonValue? _jsonValue;

    public JsonDynamicValue(JsonValue? jsonValue)
    {
        _jsonValue = jsonValue;
    }

    public override JsonNode? Node => _jsonValue;

    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        object? otherValue;
        JsonValueKind valueKind;
        if (obj is JsonDynamicValue value)
        {
            otherValue = value._jsonValue.GetObjectValue();
            valueKind = value._jsonValue?.GetValueKind() ?? _jsonValue?.GetValueKind() ?? JsonValueKind.Undefined;
        }
        else
        {
            otherValue = obj;
            valueKind = _jsonValue?.GetValueKind() ?? JsonValueKind.Undefined;
        }

        return Compare(_jsonValue.GetObjectValue(), otherValue, valueKind);
    }

    public int CompareTo(JsonDynamicValue? other)
    {
        if (other is null)
        {
            return 1;
        }

        var valueKind = other._jsonValue?.GetValueKind() ?? _jsonValue?.GetValueKind() ?? JsonValueKind.Undefined;

        return Compare(_jsonValue.GetObjectValue(), other._jsonValue.GetObjectValue(), valueKind);
    }

    public override bool Equals(object? obj)
        => Equals(obj as JsonDynamicValue);

    public bool Equals(JsonDynamicValue? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (ReferenceEquals(other, null))
        {
            return false;
        }

        var obj = _jsonValue.GetObjectValue();
        var objOther = other._jsonValue.GetObjectValue();

        if (ReferenceEquals(obj, objOther))
        {
            return true;
        }

        if (ReferenceEquals(obj, null) ||
            ReferenceEquals(objOther, null))
        {
            return false;
        }

        return obj.Equals(objOther);
    }

    public override int GetHashCode()
    {
        return _jsonValue.GetObjectValue()?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return ToString(null, null);
    }

    public string ToString(string format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public string ToString(IFormatProvider? formatProvider)
    {
        return ToString(null, formatProvider);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (_jsonValue == null)
        {
            return string.Empty;
        }

        var value = _jsonValue.GetObjectValue();
        if (value is IFormattable formattable)
        {
            return formattable.ToString(format, formatProvider);
        }
        else
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        if (!binder.Explicit)
        {
            try
            {
                if (_jsonValue is null || _jsonValue.GetValueKind() == JsonValueKind.Null)
                {
                    if (binder.Type.IsValueType &&
                        (!binder.Type.IsGenericType || binder.Type.GetGenericTypeDefinition() != typeof(Nullable<>)))
                    {
                        // Create default instance of the value type.
                        result = Activator.CreateInstance(binder.Type);
                    }
                    else
                    {
                        result = null;
                    }
                }
                else
                {
                    result = ((IConvertible)this).ToType(binder.Type, CultureInfo.InvariantCulture);
                }

                return true;
            }
            catch (Exception) { }
        }

        return base.TryConvert(binder, out result);
    }

    TypeCode IConvertible.GetTypeCode()
    {
        if (_jsonValue == null)
        {
            return TypeCode.Empty;
        }

        return TypeCode.Object;
    }

    bool IConvertible.ToBoolean(IFormatProvider? provider)
    {
        return (bool)this;
    }

    byte IConvertible.ToByte(IFormatProvider? provider)
    {
        return (byte)this;
    }

    char IConvertible.ToChar(IFormatProvider? provider)
    {
        return (char)this;
    }

    DateTime IConvertible.ToDateTime(IFormatProvider? provider)
    {
        return (DateTime)this;
    }

    decimal IConvertible.ToDecimal(IFormatProvider? provider)
    {
        return (decimal)this;
    }

    double IConvertible.ToDouble(IFormatProvider? provider)
    {
        return (double)this;
    }

    short IConvertible.ToInt16(IFormatProvider? provider)
    {
        return (short)this;
    }

    int IConvertible.ToInt32(IFormatProvider? provider)
    {
        return (int)this;
    }

    long IConvertible.ToInt64(IFormatProvider? provider)
    {
        return (long)this;
    }

    sbyte IConvertible.ToSByte(IFormatProvider? provider)
    {
        return (sbyte)this;
    }

    float IConvertible.ToSingle(IFormatProvider? provider)
    {
        return (float)this;
    }

    string IConvertible.ToString(IFormatProvider? provider)
    {
        return ToString(provider);
    }

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        return _jsonValue?.ToObject(conversionType)
            ?? throw new InvalidOperationException($"Cannot convert {this} to {conversionType}");
    }

    ushort IConvertible.ToUInt16(IFormatProvider? provider)
    {
        return (ushort)this;
    }

    uint IConvertible.ToUInt32(IFormatProvider? provider)
    {
        return (uint)this;
    }

    ulong IConvertible.ToUInt64(IFormatProvider? provider)
    {
        return (ulong)this;
    }

    public static bool operator ==(JsonDynamicValue left, JsonDynamicValue right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }

        return left.Equals(right);
    }

    public static bool operator !=(JsonDynamicValue left, JsonDynamicValue right) => !(left == right);

    public static bool operator <(JsonDynamicValue left, JsonDynamicValue right) => ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;

    public static bool operator <=(JsonDynamicValue left, JsonDynamicValue right) => ReferenceEquals(left, null) || left.CompareTo(right) <= 0;

    public static bool operator >(JsonDynamicValue left, JsonDynamicValue right) => !ReferenceEquals(left, null) && left.CompareTo(right) > 0;

    public static bool operator >=(JsonDynamicValue left, JsonDynamicValue right) => ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;

    public static explicit operator bool(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<bool>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Boolean");
    }

    public static explicit operator bool?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<bool?>();
    }

    public static explicit operator byte(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<byte>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Byte");
    }

    public static explicit operator byte?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<byte?>();
    }

    public static explicit operator char(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<char>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Char");
    }

    public static explicit operator char?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<char?>();
    }

    public static explicit operator DateTime(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<DateTime>()
            ?? throw new InvalidCastException($"Cannot convert {value} to DateTime");
    }

    public static explicit operator DateTime?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<DateTime?>();
    }

    public static explicit operator DateTimeOffset(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<DateTimeOffset>()
            ?? throw new InvalidCastException($"Cannot convert {value} to DateTimeOffset");
    }

    public static explicit operator DateTimeOffset?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<DateTimeOffset?>();
    }

    public static explicit operator decimal(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<decimal>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Decimal");
    }

    public static explicit operator decimal?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<decimal?>();
    }

    public static explicit operator double(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<double>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Double");
    }

    public static explicit operator double?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<double?>();
    }

    public static explicit operator Guid(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<Guid>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Guid");
    }

    public static explicit operator Guid?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<Guid?>();
    }

    public static explicit operator short(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<short>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Int16");
    }

    public static explicit operator short?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<short?>();
    }

    public static explicit operator int(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<int>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Int32");
    }

    public static explicit operator int?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<int?>();
    }

    public static explicit operator long(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<long>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Int64");
    }

    public static explicit operator long?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<long?>();
    }

    public static explicit operator sbyte(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<sbyte>()
            ?? throw new InvalidCastException($"Cannot convert {value} to SByte");
    }

    public static explicit operator sbyte?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<sbyte?>();
    }

    public static explicit operator float(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<float>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Float");
    }

    public static explicit operator float?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<float?>();
    }

    public static explicit operator string?(JsonDynamicValue value)
    {
        return value?.ToString();
    }

    public static explicit operator ushort(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<ushort>()
            ?? throw new InvalidCastException($"Cannot convert {value} to UInt32");
    }

    public static explicit operator ushort?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<ushort?>();
    }

    public static explicit operator uint(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<uint>()
            ?? throw new InvalidCastException($"Cannot convert {value} to UInt32");
    }

    public static explicit operator uint?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<uint?>();
    }

    public static explicit operator ulong(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<ulong>()
            ?? throw new InvalidCastException($"Cannot convert {value} to UInt64");
    }

    public static explicit operator ulong?(JsonDynamicValue value)
    {
        return value?._jsonValue?.GetValue<ulong?>();
    }

    public static explicit operator byte[]?(JsonDynamicValue value)
    {
        if (value?._jsonValue.GetObjectValue() is string str)
        {
            return Convert.FromBase64String(str);
        }

        throw new InvalidCastException($"Cannot convert {value} to Byte array");
    }

    public static explicit operator TimeSpan(JsonDynamicValue value)
    {
        if (value?._jsonValue?.GetObjectValue() is string str)
        {
            return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidCastException($"Cannot convert {value} to TimeSpan");
    }

    public static explicit operator TimeSpan?(JsonDynamicValue value)
    {
        var str = value?._jsonValue?.GetObjectValue() as string;

        return str is not null
            ? TimeSpan.Parse(str, CultureInfo.InvariantCulture)
            : null;
    }

    public static explicit operator Uri?(JsonDynamicValue value)
    {
        return new(value?._jsonValue?.GetValue<string>() ?? string.Empty);
    }

    public static implicit operator JsonDynamicValue(bool value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(bool? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(byte value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(byte? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(char value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(char? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(DateTime value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(DateTime? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(DateTimeOffset value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(DateTimeOffset? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(decimal value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(decimal? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(double value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(double? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(Guid value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(Guid? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(short value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(short? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(int value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(int? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(long value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(long? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(sbyte value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(sbyte? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(float value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(float? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(string value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(ushort value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(ushort? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(uint value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(uint? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(ulong value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(ulong? value)
    {
        return new JsonDynamicValue(JsonValue.Create(value, JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(byte[] value)
    {
        return new JsonDynamicValue(JsonValue.Create(Convert.ToBase64String(value), JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(TimeSpan value)
    {
        return new JsonDynamicValue(JsonValue.Create(value.ToString(), JOptions.Node));
    }

    public static implicit operator JsonDynamicValue(TimeSpan? value)
    {
        return new JsonDynamicValue(value.HasValue ? JsonValue.Create(value.ToString(), JOptions.Node) : null);
    }

    public static implicit operator JsonDynamicValue(Uri? value)
    {
        return new JsonDynamicValue(value != null ? JsonValue.Create(value.ToString(), JOptions.Node) : null);
    }

    public static implicit operator JsonValue?(JsonDynamicValue value) => value._jsonValue;

    public static implicit operator JsonDynamicValue(JsonValue value) => new(value);

    private static int Compare(object? objA, object? objB, JsonValueKind valueType)
    {
        if (objA == objB)
        {
            return 0;
        }

        if (objB == null)
        {
            return 1;
        }

        if (objA == null)
        {
            return -1;
        }

        switch (valueType)
        {
            case JsonValueKind.False:
            case JsonValueKind.True:

                var b1 = Convert.ToBoolean(objA, CultureInfo.InvariantCulture);
                var b2 = Convert.ToBoolean(objB, CultureInfo.InvariantCulture);

                return b1.CompareTo(b2);

            case JsonValueKind.Number:

                if (objA is BigInteger integerA)
                {
                    return CompareBigInteger(integerA, objB);
                }
                if (objB is BigInteger integerB)
                {
                    return -CompareBigInteger(integerB, objA);
                }

                if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                {
                    return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
                }
                else if (objA is float || objB is float || objA is double || objB is double)
                {
                    var d1 = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
                    var d2 = Convert.ToDouble(objB, CultureInfo.InvariantCulture);

                    return d1.CompareTo(d2);
                }
                else
                {
                    return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
                }

            case JsonValueKind.String:

                var s1 = Convert.ToString(objA, CultureInfo.InvariantCulture);
                var s2 = Convert.ToString(objB, CultureInfo.InvariantCulture);

                return string.CompareOrdinal(s1, s2);

            default:
                throw new NotImplementedException($"Comparing {objA?.GetType()} to {objB?.GetType()} is currently not implemented.");
        }
    }

    private static int CompareBigInteger(BigInteger i1, object i2)
    {
        var result = i1.CompareTo(ToBigInteger(i2));

        if (result != 0)
        {
            return result;
        }

        // Converting a fractional number to a BigInteger will lose the fraction
        // check for fraction if result is two numbers are equal
        if (i2 is decimal d1)
        {
            return (0m).CompareTo(Math.Abs(d1 - Math.Truncate(d1)));
        }
        else if (i2 is double || i2 is float)
        {
            var d = Convert.ToDouble(i2, CultureInfo.InvariantCulture);
            return (0d).CompareTo(Math.Abs(d - Math.Truncate(d)));
        }

        return result;

        static BigInteger ToBigInteger(object value)
        {
            if (value is BigInteger integer)
            {
                return integer;
            }

            if (value is string s)
            {
                return BigInteger.Parse(s, CultureInfo.InvariantCulture);
            }

            if (value is float f)
            {
                return new BigInteger(f);
            }
            if (value is double d)
            {
                return new BigInteger(d);
            }
            if (value is decimal @decimal)
            {
                return new BigInteger(@decimal);
            }
            if (value is int i)
            {
                return new BigInteger(i);
            }
            if (value is long l)
            {
                return new BigInteger(l);
            }
            if (value is uint u)
            {
                return new BigInteger(u);
            }
            if (value is ulong @ulong)
            {
                return new BigInteger(@ulong);
            }

            if (value is byte[] bytes)
            {
                return new BigInteger(bytes);
            }

            throw new InvalidCastException($"Cannot convert {value.GetType()} to BigInteger.");
        }
    }
}
