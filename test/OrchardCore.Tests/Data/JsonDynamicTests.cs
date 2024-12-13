using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Scripting;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Data;

public class JsonDynamicTests
{
    [Fact]
    public void JsonDynamicValueMustConvertToBool()
    {
        var expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (bool)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableBool()
    {
        bool? expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (bool?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToByte()
    {
        byte expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (byte)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableBye()
    {
        byte? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (byte?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToChar()
    {
        var expectedValue = 'A';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (char)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableChar()
    {
        char? expectedValue = 'B';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (char?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToDateTime()
    {
        var expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTime)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableDateTime()
    {
        DateTime? expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTime?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToDateTimeOffset()
    {
        DateTimeOffset expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTimeOffset)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullablDateTimeOffset()
    {
        DateTimeOffset? expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTimeOffset?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToDecimal()
    {
        decimal expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (decimal)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableDecimal()
    {
        decimal? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (decimal?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToDouble()
    {
        double expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (double)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableDouble()
    {
        double? expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (double?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToGuid()
    {
        Guid expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (Guid)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableGuid()
    {
        Guid? expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (Guid?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToInt16()
    {
        short expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (short)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableInt16()
    {
        short? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (short?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToInt32()
    {
        int expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (int)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableInt32()
    {
        int? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (int?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToInt64()
    {
        long expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (long)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableInt64()
    {
        long? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (long?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToSByte()
    {
        sbyte expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (sbyte)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableSByte()
    {
        sbyte? expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (sbyte?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToSingle()
    {
        float expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (float)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableSingle()
    {
        float? expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (float?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToString()
    {
        var expectedValue = "A test string value";
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (string)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToUInt16()
    {
        ushort expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ushort)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableUInt16()
    {
        ushort? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ushort?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToUInt32()
    {
        uint expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (uint)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableUInt32()
    {
        uint? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (uint?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToUInt64()
    {
        ulong expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ulong)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableUInt64()
    {
        ulong? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ulong?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToByteArray()
    {
        var expectedValue = Encoding.UTF8.GetBytes("A string in a byte array");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (byte[])myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToTimeSpan()
    {
        var expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Assert.Equal(expectedValue, (TimeSpan)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToNullableTimeSpan()
    {
        var expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Assert.Equal(expectedValue, (TimeSpan?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueMustConvertToUri()
    {
        var expectedValue = new Uri("https://www.orchardcore.net");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Assert.Equal(expectedValue, (Uri)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToBool()
    {
        var expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        bool value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableBool()
    {
        bool? expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        bool? value = myDynamic;

        Assert.Equal(expectedValue, value);

        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToByte()
    {
        byte expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        byte value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableByte()
    {
        byte? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        byte? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToChar()
    {
        var expectedValue = 'A';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        char value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableChar()
    {
        char? expectedValue = 'B';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        char? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDateTime()
    {
        var expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTime value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDateTime()
    {
        DateTime? expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTime? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDateTimeOffset()
    {
        DateTimeOffset expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTimeOffset value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullablDateTimeOffset()
    {
        DateTimeOffset? expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTimeOffset? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDecimal()
    {
        decimal expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        decimal value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDecimal()
    {
        decimal? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        decimal? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDouble()
    {
        double expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        double value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDouble()
    {
        double? expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        double? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToGuid()
    {
        Guid expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Guid value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableGuid()
    {
        Guid? expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Guid? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToInt16()
    {
        short expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        short value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableInt16()
    {
        short? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        short? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToInt32()
    {
        int expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        int value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableInt32()
    {
        int? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        int? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToInt64()
    {
        long expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        long value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableInt64()
    {
        long? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        long? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToSByte()
    {
        sbyte expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        sbyte value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableSByte()
    {
        sbyte? expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        sbyte? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToSingle()
    {
        float expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        float value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableSingle()
    {
        float? expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        float? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToString()
    {
        var expectedValue = "A test string value";
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        string value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUInt16()
    {
        ushort expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ushort value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableUInt16()
    {
        ushort? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ushort? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUInt32()
    {
        uint expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        uint value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableUInt32()
    {
        uint? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        uint? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUInt64()
    {
        ulong expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ulong value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableUInt64()
    {
        ulong? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ulong? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToByteArray()
    {
        var expectedValue = Encoding.UTF8.GetBytes("A string in a byte array");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        byte[] value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToTimeSpan()
    {
        var expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        TimeSpan value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableTimeSpan()
    {
        TimeSpan? expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        TimeSpan? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUri()
    {
        var expectedValue = new Uri("https://www.orchardcore.net");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Uri value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullable()
    {
        int? expectedValue = null;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create((int?)null));

        int? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    // Note: Direct comparison for additional types must be added later. Currently only
    // numbers, booleans and strings are supported.
    [Fact]
    public void JsonDynamicValueIsComparableToInt32()
    {
        int value = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(value));

        Assert.True(value >= myDynamic);
        Assert.True(value + 10 > myDynamic);

        Assert.False(value - 10 >= myDynamic);
        Assert.False(value > myDynamic);

        Assert.True(value <= myDynamic);
        Assert.True(value - 10 < myDynamic);

        Assert.False(value + 10 <= myDynamic);
        Assert.False(value < myDynamic);
    }

    [Fact]
    public void JsonDynamicValueIsComparableToInt64()
    {
        long value = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(value));

        Assert.True(value >= myDynamic);
        Assert.True(value + 10 > myDynamic);

        Assert.False(value - 10 >= myDynamic);
        Assert.False(value > myDynamic);

        Assert.True(value <= myDynamic);
        Assert.True(value - 10 < myDynamic);

        Assert.False(value + 10 <= myDynamic);
        Assert.False(value < myDynamic);
    }

    [Fact]
    public void JsonDynamicValueIsComparableToDouble()
    {
        double value = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(value));

        Assert.True(value >= myDynamic);
        Assert.True(value + 10 > myDynamic);

        Assert.False(value - 10 >= myDynamic);
        Assert.False(value > myDynamic);

        Assert.True(value <= myDynamic);
        Assert.True(value - 10 < myDynamic);

        Assert.False(value + 10 <= myDynamic);
        Assert.False(value < myDynamic);
    }

    [Fact]
    public void JsonDynamicValueIsComparableToString()
    {
        var value = "A string value";
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(value));

        Assert.True(value >= myDynamic);
        Assert.True("B " + value > myDynamic);

        Assert.False("0 " + value >= myDynamic);
        Assert.False(value > myDynamic);

        Assert.True(value <= myDynamic);
        Assert.True("0 " + value < myDynamic);

        Assert.False("B " + value <= myDynamic);
        Assert.False(value < myDynamic);
    }

    [Fact]
    public void SerializingJsonDynamicValueMustWriteValueOnly()
    {
        // Arrange
        var contentItem = GetContentTestData();
        dynamic contentExpando = new ExpandoObject();
        contentExpando.content = contentItem.Content;

        // Act
        var contentStr = JConvert.SerializeObject((ExpandoObject)contentExpando);

        // Assert
        Assert.Equal("{\"content\":{\"TestPart\":{\"TextFieldProp\":{\"Text\":\"test\"},\"NumericFieldProp\":{\"Value\":123},\"BooleanFieldProp\":{\"Value\":true}}}}", contentStr);
    }

    [Fact]
    public async Task SerializingJsonDynamicValueInScripting()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(scope =>
        {
            var getTestContent = new GlobalMethod
            {
                Name = "getTestContent",
                Method = sp => () => GetContentTestData()
            };
            var scriptingEngine = scope.ServiceProvider.GetRequiredService<IScriptingEngine>();
            var scriptingScope = scriptingEngine.CreateScope([getTestContent], scope.ServiceProvider, null, null);

            // Act
            var contentStr = (string)scriptingEngine.Evaluate(scriptingScope, "return JSON.stringify(getTestContent().Content)");

            // Assert
            Assert.Equal("{\"TestPart\":{\"TextFieldProp\":{\"Text\":\"test\"},\"NumericFieldProp\":{\"Value\":123},\"BooleanFieldProp\":{\"Value\":true}}}", contentStr);

            return Task.CompletedTask;
        });
    }

    private static ContentItem GetContentTestData()
    {
        var contentItem = new ContentItem();
        contentItem.Alter<TestPart>(part =>
        {
            part.TextFieldProp = new TextField { Text = "test" };
            part.NumericFieldProp = new NumericField { Value = 123 };
            part.BooleanFieldProp = new BooleanField { Value = true };
        });
        return contentItem;
    }

    public sealed class TestPart : ContentPart
    {
        public TextField TextFieldProp { get; set; }

        public NumericField NumericFieldProp { get; set; }

        public BooleanField BooleanFieldProp { get; set; }
    }
}
