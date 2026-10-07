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
    public void JsonDynamicValue_Default_ConvertsToBool()
    {
        var expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (bool)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableBool()
    {
        bool? expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (bool?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToByte()
    {
        byte expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (byte)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableByte()
    {
        byte? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (byte?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToChar()
    {
        var expectedValue = 'A';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (char)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableChar()
    {
        char? expectedValue = 'B';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (char?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToDateTime()
    {
        var expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTime)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableDateTime()
    {
        DateTime? expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTime?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToDateTimeOffset()
    {
        DateTimeOffset expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTimeOffset)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableDateTimeOffset()
    {
        DateTimeOffset? expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (DateTimeOffset?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToDecimal()
    {
        decimal expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (decimal)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableDecimal()
    {
        decimal? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (decimal?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToDouble()
    {
        double expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (double)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableDouble()
    {
        double? expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (double?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToGuid()
    {
        Guid expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (Guid)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableGuid()
    {
        Guid? expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (Guid?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToInt16()
    {
        short expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (short)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableInt16()
    {
        short? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (short?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToInt32()
    {
        int expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (int)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableInt32()
    {
        int? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (int?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToInt64()
    {
        long expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (long)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableInt64()
    {
        long? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (long?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToSByte()
    {
        sbyte expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (sbyte)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableSByte()
    {
        sbyte? expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (sbyte?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToSingle()
    {
        float expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (float)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableSingle()
    {
        float? expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (float?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToString()
    {
        var expectedValue = "A test string value";
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (string)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToUInt16()
    {
        ushort expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ushort)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableUInt16()
    {
        ushort? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ushort?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToUInt32()
    {
        uint expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (uint)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableUInt32()
    {
        uint? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (uint?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToUInt64()
    {
        ulong expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ulong)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableUInt64()
    {
        ulong? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (ulong?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToByteArray()
    {
        var expectedValue = Encoding.UTF8.GetBytes("A string in a byte array");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Assert.Equal(expectedValue, (byte[])myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToTimeSpan()
    {
        var expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Assert.Equal(expectedValue, (TimeSpan)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToNullableTimeSpan()
    {
        var expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Assert.Equal(expectedValue, (TimeSpan?)myDynamic);
    }

    [Fact]
    public void JsonDynamicValue_Default_ConvertsToUri()
    {
        var expectedValue = new Uri("https://www.orchardcore.net");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Assert.Equal(expectedValue, (Uri)myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToBool_Default_Succeeds()
    {
        var expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        bool value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableBool_Default_Succeeds()
    {
        bool? expectedValue = true;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        bool? value = myDynamic;

        Assert.Equal(expectedValue, value);

        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToByte_Default_Succeeds()
    {
        byte expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        byte value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableByte_Default_Succeeds()
    {
        byte? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        byte? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToChar_Default_Succeeds()
    {
        var expectedValue = 'A';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        char value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableChar_Default_Succeeds()
    {
        char? expectedValue = 'B';
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        char? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDateTime_Default_Succeeds()
    {
        var expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTime value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDateTime_Default_Succeeds()
    {
        DateTime? expectedValue = DateTime.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTime? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDateTimeOffset_Default_Succeeds()
    {
        DateTimeOffset expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTimeOffset value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDateTimeOffset_Default_Succeeds()
    {
        DateTimeOffset? expectedValue = DateTimeOffset.UtcNow;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        DateTimeOffset? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDecimal_Default_Succeeds()
    {
        decimal expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        decimal value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDecimal_Default_Succeeds()
    {
        decimal? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        decimal? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToDouble_Default_Succeeds()
    {
        double expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        double value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableDouble_Default_Succeeds()
    {
        double? expectedValue = 42.42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        double? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToGuid_Default_Succeeds()
    {
        Guid expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Guid value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableGuid_Default_Succeeds()
    {
        Guid? expectedValue = Guid.NewGuid();
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        Guid? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToInt16_Default_Succeeds()
    {
        short expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        short value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableInt16_Default_Succeeds()
    {
        short? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        short? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToInt32_Default_Succeeds()
    {
        int expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        int value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableInt32_Default_Succeeds()
    {
        int? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        int? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToInt64_Default_Succeeds()
    {
        long expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        long value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableInt64_Default_Succeeds()
    {
        long? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        long? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToSByte_Default_Succeeds()
    {
        sbyte expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        sbyte value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableSByte_Default_Succeeds()
    {
        sbyte? expectedValue = -42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        sbyte? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToSingle_Default_Succeeds()
    {
        float expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        float value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableSingle_Default_Succeeds()
    {
        float? expectedValue = 42.42F;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        float? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToString_Default_Succeeds()
    {
        var expectedValue = "A test string value";
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        string value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUInt16_Default_Succeeds()
    {
        ushort expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ushort value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableUInt16_Default_Succeeds()
    {
        ushort? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ushort? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUInt32_Default_Succeeds()
    {
        uint expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        uint value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableUInt32_Default_Succeeds()
    {
        uint? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        uint? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUInt64_Default_Succeeds()
    {
        ulong expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ulong value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableUInt64_Default_Succeeds()
    {
        ulong? expectedValue = 42;
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        ulong? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToByteArray_Default_Succeeds()
    {
        var expectedValue = Encoding.UTF8.GetBytes("A string in a byte array");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue));

        byte[] value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToTimeSpan_Default_Succeeds()
    {
        var expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        TimeSpan value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullableTimeSpan_Default_Succeeds()
    {
        TimeSpan? expectedValue = TimeSpan.FromSeconds(42);
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        TimeSpan? value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToUri_Default_Succeeds()
    {
        var expectedValue = new Uri("https://www.orchardcore.net");
        dynamic myDynamic = new JsonDynamicValue(JsonValue.Create(expectedValue.ToString()));

        Uri value = myDynamic;

        Assert.Equal(expectedValue, value);
        Assert.True(expectedValue == myDynamic);
        Assert.False(expectedValue != myDynamic);
    }

    [Fact]
    public void JsonDynamicValueCanImplicitlyConvertToNullable_Default_Succeeds()
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
    public void JsonDynamicValueIsComparableToInt32_Default_Succeeds()
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
    public void JsonDynamicValueIsComparableToInt64_Default_Succeeds()
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
    public void JsonDynamicValueIsComparableToDouble_Default_Succeeds()
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
    public void JsonDynamicValueIsComparableToString_Default_Succeeds()
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
    public void SerializingJsonDynamicValue_Default_WritesValueOnly()
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
    public async Task SerializingJsonDynamicValueInScripting_Default_Succeeds()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();
        await context.UsingTenantScopeAsync(scope =>
        {
            var getTestContent = new GlobalMethod
            {
                Name = "getTestContent",
                Method = sp => () => GetContentTestData(),
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
