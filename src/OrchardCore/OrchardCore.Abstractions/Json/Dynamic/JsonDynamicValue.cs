using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Nodes;

namespace System.Text.Json.Dynamic;

#nullable enable

public class JsonDynamicValue : DynamicObject, IConvertible
{
    public JsonDynamicValue(JsonValue? jsonValue) => JsonValue = jsonValue;

    public JsonValue? JsonValue { get; }

    public override DynamicMetaObject GetMetaObject(Expression parameter)
        => new JsonDynamicMetaObject(parameter, this);
    
    public override string ToString() 
        => JsonValue?.ToString() ?? string.Empty;

    TypeCode IConvertible.GetTypeCode()
    {
        if (JsonValue == null)
        {
            return TypeCode.Empty;
        }

        return TypeCode.Object;
    }

    bool IConvertible.ToBoolean(IFormatProvider? provider) 
        => (bool)this;

    byte IConvertible.ToByte(IFormatProvider? provider) 
        => (byte)this;

    char IConvertible.ToChar(IFormatProvider? provider) 
        => (char)this;

    DateTime IConvertible.ToDateTime(IFormatProvider? provider) 
        => (DateTime)this;

    decimal IConvertible.ToDecimal(IFormatProvider? provider) 
        => (decimal)this;

    double IConvertible.ToDouble(IFormatProvider? provider) 
        => (double)this;

    short IConvertible.ToInt16(IFormatProvider? provider) 
        => (short)this;

    int IConvertible.ToInt32(IFormatProvider? provider) 
        => (int)this;

    long IConvertible.ToInt64(IFormatProvider? provider) 
        => (long)this;

    sbyte IConvertible.ToSByte(IFormatProvider? provider) 
        => (sbyte)this;

    float IConvertible.ToSingle(IFormatProvider? provider) 
        => (float)this;

    string IConvertible.ToString(IFormatProvider? provider) 
        => (string?)this ?? string.Empty;

    object IConvertible.ToType(Type conversionType, IFormatProvider? provider) 
        => JsonValue?.ToObject(conversionType) 
        ?? throw new InvalidOperationException($"Cannot convert {this} to {conversionType}");

    ushort IConvertible.ToUInt16(IFormatProvider? provider) 
        => (ushort)this;

    uint IConvertible.ToUInt32(IFormatProvider? provider) 
        => (uint)this;

    ulong IConvertible.ToUInt64(IFormatProvider? provider) 
        => (ulong)this;

    public static explicit operator bool(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<bool>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Boolean");

    public static explicit operator bool?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<bool?>();

    public static explicit operator byte(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<byte>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Byte");
    
    public static explicit operator byte?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<byte?>();

    public static explicit operator char(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<char>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Char");

    public static explicit operator char?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<char?>();

    public static explicit operator DateTime(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<DateTime>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to DateTime");
    
    public static explicit operator DateTime?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<DateTime?>();

    public static explicit operator DateTimeOffset(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<DateTimeOffset>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to DateTimeOffset");

    public static explicit operator DateTimeOffset?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<DateTimeOffset?>();

    public static explicit operator decimal(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<decimal>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Decimal");

    public static explicit operator decimal?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<decimal?>();

    public static explicit operator double(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<double>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Double");

    public static explicit operator double?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<double?>();

    public static explicit operator Guid(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<Guid>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Guid");
    
    public static explicit operator Guid?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<Guid?>();

    public static explicit operator short(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<short>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Int16");

    public static explicit operator short?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<short?>();

    public static explicit operator int(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<int>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Int32");

    public static explicit operator int?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<int?>();

    public static explicit operator long(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<long>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Int64");

    public static explicit operator long?(JsonDynamicValue value)
        => value?.JsonValue?.GetValue<long?>();

    public static explicit operator sbyte(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<sbyte>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to SByte");

    public static explicit operator sbyte?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<sbyte?>();

    public static explicit operator float(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<float>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to Float");

    public static explicit operator float?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<float?>();

    public static explicit operator string?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<string>();

    public static explicit operator ushort(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<ushort>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to UInt32");

    public static explicit operator ushort?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<ushort?>();

    public static explicit operator uint(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<uint>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to UInt32");

    public static explicit operator uint?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<uint?>();

    public static explicit operator ulong(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<ulong>() 
        ?? throw new InvalidCastException($"Cannot convert {value} to UInt64");

    public static explicit operator ulong?(JsonDynamicValue value) 
        => value?.JsonValue?.GetValue<ulong?>();

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
        => new Uri(value?.JsonValue?.GetValue<string>() ?? string.Empty);

    private sealed class JsonDynamicMetaObject : DynamicMetaObject
    {
        public JsonDynamicMetaObject(Expression expression, JsonDynamicValue value)
            : base(expression, BindingRestrictions.Empty, value)
        {
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            var targetType = binder.Type;

            var castMethod = typeof(JsonDynamicValue).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "op_Explicit" && m.ReturnType == targetType)
                .FirstOrDefault();

            if (castMethod != null)
            {
                var convertExpression = Expression.Convert(Expression.Convert(Expression, typeof(JsonDynamicValue)), targetType, castMethod);
                return new DynamicMetaObject(convertExpression, BindingRestrictions.GetTypeRestriction(Expression, typeof(JsonDynamicValue)));
            }

            // Fallback to default behavior.
            return base.BindConvert(binder);
        }
    }
}
