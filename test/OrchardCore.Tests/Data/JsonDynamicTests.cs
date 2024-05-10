using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;

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
}
