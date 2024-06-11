using System.Collections;
using System.Collections.Frozen;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Nodes;

namespace System.Text.Json.Dynamic;

#nullable enable

public class JsonDynamicValue : DynamicObject, IComparable, IComparable<JsonDynamicValue>, IConvertible, IEquatable<JsonDynamicValue>
{
    public JsonDynamicValue(JsonValue? jsonValue)
    {
        JsonValue = jsonValue;
    }

    public JsonValue? JsonValue { get; }

    public override DynamicMetaObject GetMetaObject(Expression parameter)
    {
        return new JsonDynamicMetaObject(parameter, this);
    }

    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        object? otherValue;
        JsonValueKind valueKind;

        if (obj is JsonDynamicValue value)
        {
            otherValue = value.JsonValue.GetObjectValue();
            valueKind = value.JsonValue?.GetValueKind() ?? JsonValue?.GetValueKind() ?? JsonValueKind.Undefined;
        }
        else
        {
            otherValue = obj;
            valueKind = JsonValue?.GetValueKind() ?? JsonValueKind.Undefined;
        }

        return Compare(JsonValue.GetObjectValue(), otherValue, valueKind);
    }

    public int CompareTo(JsonDynamicValue? other)
    {
        if (other is null)
            return 1;

        var valueKind = other.JsonValue?.GetValueKind() ?? JsonValue?.GetValueKind() ?? JsonValueKind.Undefined;

        return Compare(JsonValue.GetObjectValue(), other.JsonValue.GetObjectValue(), valueKind);
    }

    public override bool Equals(object? obj)
        => Equals(obj as JsonDynamicValue);

    public bool Equals(JsonDynamicValue? other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (ReferenceEquals(other, null))
            return false;

        var obj = JsonValue.GetObjectValue();
        var objOther = other.JsonValue.GetObjectValue();

        if (ReferenceEquals(obj, objOther))
            return true;

        if (ReferenceEquals(obj, null) ||
            ReferenceEquals(objOther, null))
            return false;

        return obj.Equals(objOther);
    }

    public override int GetHashCode()
    {
        return JsonValue.GetObjectValue()?.GetHashCode() ?? 0;
    }

    public T? ToObject<T>() => JsonValue.ToObject<T>();

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
        if (JsonValue == null)
        {
            return string.Empty;
        }

        var value = JsonValue.GetObjectValue();
        if (value is IFormattable formattable)
        {
            return formattable.ToString(format, formatProvider);
        }
        else
        {
            return value?.ToString() ?? string.Empty;
        }
    }

    TypeCode IConvertible.GetTypeCode()
    {
        if (JsonValue == null)
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
        return JsonValue?.ToObject(conversionType)
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

    public static bool operator !=(JsonDynamicValue left, JsonDynamicValue right)
    {
        return !(left == right);
    }

    public static bool operator <(JsonDynamicValue left, JsonDynamicValue right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(JsonDynamicValue left, JsonDynamicValue right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(JsonDynamicValue left, JsonDynamicValue right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(JsonDynamicValue left, JsonDynamicValue right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }

    public static explicit operator bool(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<bool>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Boolean");
    }

    public static explicit operator bool?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<bool?>();
    }

    public static explicit operator byte(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<byte>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Byte");
    }

    public static explicit operator byte?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<byte?>();
    }

    public static explicit operator char(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<char>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Char");
    }

    public static explicit operator char?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<char?>();
    }

    public static explicit operator DateTime(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<DateTime>()
            ?? throw new InvalidCastException($"Cannot convert {value} to DateTime");
    }

    public static explicit operator DateTime?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<DateTime?>();
    }

    public static explicit operator DateTimeOffset(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<DateTimeOffset>()
            ?? throw new InvalidCastException($"Cannot convert {value} to DateTimeOffset");
    }

    public static explicit operator DateTimeOffset?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<DateTimeOffset?>();
    }

    public static explicit operator decimal(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<decimal>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Decimal");
    }

    public static explicit operator decimal?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<decimal?>();
    }

    public static explicit operator double(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<double>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Double");
    }

    public static explicit operator double?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<double?>();
    }

    public static explicit operator Guid(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<Guid>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Guid");
    }

    public static explicit operator Guid?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<Guid?>();
    }

    public static explicit operator short(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<short>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Int16");
    }

    public static explicit operator short?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<short?>();
    }

    public static explicit operator int(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<int>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Int32");
    }

    public static explicit operator int?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<int?>();
    }

    public static explicit operator long(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<long>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Int64");
    }

    public static explicit operator long?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<long?>();
    }

    public static explicit operator sbyte(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<sbyte>()
            ?? throw new InvalidCastException($"Cannot convert {value} to SByte");
    }

    public static explicit operator sbyte?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<sbyte?>();
    }

    public static explicit operator float(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<float>()
            ?? throw new InvalidCastException($"Cannot convert {value} to Float");
    }

    public static explicit operator float?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<float?>();
    }

    public static explicit operator string?(JsonDynamicValue value)
    {
        return value?.ToString();
    }

    public static explicit operator ushort(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<ushort>()
            ?? throw new InvalidCastException($"Cannot convert {value} to UInt32");
    }

    public static explicit operator ushort?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<ushort?>();
    }

    public static explicit operator uint(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<uint>()
            ?? throw new InvalidCastException($"Cannot convert {value} to UInt32");
    }

    public static explicit operator uint?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<uint?>();
    }

    public static explicit operator ulong(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<ulong>()
            ?? throw new InvalidCastException($"Cannot convert {value} to UInt64");
    }

    public static explicit operator ulong?(JsonDynamicValue value)
    {
        return value?.JsonValue?.GetValue<ulong?>();
    }

    public static explicit operator byte[]?(JsonDynamicValue value)
    {
        if (value?.JsonValue.GetObjectValue() is string str)
        {
            return Convert.FromBase64String(str);
        }

        throw new InvalidCastException($"Cannot convert {value} to Byte array");
    }

    public static explicit operator TimeSpan(JsonDynamicValue value)
    {
        if (value?.JsonValue?.GetObjectValue() is string str)
        {
            return TimeSpan.Parse(str, CultureInfo.InvariantCulture);
        }

        throw new InvalidCastException($"Cannot convert {value} to TimeSpan");
    }

    public static explicit operator TimeSpan?(JsonDynamicValue value)
    {
        var str = value?.JsonValue?.GetObjectValue() as string;

        return str is not null
            ? TimeSpan.Parse(str, CultureInfo.InvariantCulture)
            : null;
    }

    public static explicit operator Uri?(JsonDynamicValue value)
    {
        return new(value?.JsonValue?.GetValue<string>() ?? string.Empty);
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
        int result = i1.CompareTo(ToBigInteger(i2));

        if (result != 0)
        {
            return result;
        }

        // converting a fractional number to a BigInteger will lose the fraction
        // check for fraction if result is two numbers are equal
        if (i2 is decimal d1)
        {
            return (0m).CompareTo(Math.Abs(d1 - Math.Truncate(d1)));
        }
        else if (i2 is double || i2 is float)
        {
            double d = Convert.ToDouble(i2, CultureInfo.InvariantCulture);
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

    private sealed class JsonDynamicMetaObject : DynamicMetaObject
    {
        private static readonly FrozenDictionary<Type, MethodInfo> _cachedReflectionInfo = typeof(JsonDynamicValue)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == "op_Explicit")
            .ToFrozenDictionary(method => method.ReturnType);

        public JsonDynamicMetaObject(Expression expression, JsonDynamicValue value)
            : base(expression, BindingRestrictions.Empty, value)
        {
        }

        // The 'BindConvert()' method is automatically invoked to handle type conversion when casting
        // dynamic types to static types in C#. 
        //
        // For example, when extracting a 'DateTime' value from a dynamically typed
        // content item's field:
        // 
        // dynamic contentItem = [...]; // Assume contentItem is initialized properly
        //
        // 'BindConvert()' is called implicitly to convert contentItem.Content.MyPart.MyField.Value 
        // to 'DateTime' with similar behavior to the following:
        // var dateTimeValue = (DateTime)contentItem.Content.MyPart.MyField.Value;
        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            var targetType = binder.Type;

            if (_cachedReflectionInfo.TryGetValue(targetType, out var castMethod))
            {
                var convertExpression = Expression.Convert(Expression.Convert(Expression, typeof(JsonDynamicValue)), targetType, castMethod);

                return new DynamicMetaObject(convertExpression, BindingRestrictions.GetTypeRestriction(Expression, typeof(JsonDynamicValue)));
            }

            // Fallback to default behavior.
            return base.BindConvert(binder);
        }
    }
}
