using System.Dynamic;
using System.Text.Json.Nodes;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class WorkflowVariableTypesTests
{
    private static readonly StringVariableType s_string = new StringVariableType(new PassThroughStringLocalizer<StringVariableType>());
    private static readonly NumberVariableType s_number = new NumberVariableType(new PassThroughStringLocalizer<NumberVariableType>());
    private static readonly BooleanVariableType s_boolean = new BooleanVariableType(new PassThroughStringLocalizer<BooleanVariableType>());
    private static readonly DateTimeVariableType s_dateTime = new DateTimeVariableType(new PassThroughStringLocalizer<DateTimeVariableType>());
    private static readonly ObjectVariableType s_object = new ObjectVariableType(new PassThroughStringLocalizer<ObjectVariableType>());
    private static readonly ArrayVariableType s_array = new ArrayVariableType(new PassThroughStringLocalizer<ArrayVariableType>());
    private static readonly AnyVariableType s_any = new AnyVariableType(new PassThroughStringLocalizer<AnyVariableType>());

    [Fact]
    public void TryCoerce_Null_IsValidForEveryType()
    {
        foreach (var type in new IWorkflowVariableType[] { s_string, s_number, s_boolean, s_dateTime, s_object, s_array, s_any })
        {
            Assert.True(type.TryCoerce(null, out var result), type.Name);
            Assert.Null(result);
        }
    }

    [Fact]
    public void TryCoerce_String_ConvertsEveryValue()
    {
        Assert.Equal("hello", Coerce(s_string, "hello"));
        Assert.Equal("42.5", Coerce(s_string, 42.5));
        Assert.Equal("true", Coerce(s_string, true));
        Assert.Equal("2026-10-06T09:00:00.0000000Z", Coerce(s_string, new DateTime(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc)));
        Assert.Equal("hello", Coerce(s_string, JsonValue.Create("hello")));
        Assert.Equal("{\"a\":1}", Coerce(s_string, new JsonObject { ["a"] = 1 }));
        Assert.Equal("[1,2]", Coerce(s_string, new List<object> { 1, 2 }));
    }

    [Theory]
    [InlineData(42, 42d)]
    [InlineData(42L, 42d)]
    [InlineData(4.5f, 4.5d)]
    [InlineData("4.5", 4.5d)]
    [InlineData("-1e3", -1000d)]
    public void TryCoerce_NumberFromNumbersAndText_ReturnsADouble(object value, double expected)
    {
        Assert.Equal(expected, Coerce(s_number, value));
    }

    [Fact]
    public void TryCoerce_NumberFromDecimalAndJson_ReturnsADouble()
    {
        Assert.Equal(2.5d, Coerce(s_number, 2.5m));
        Assert.Equal(7d, Coerce(s_number, JsonValue.Create(7)));
        Assert.Equal(7d, Coerce(s_number, JsonValue.Create("7")));
    }

    [Theory]
    [InlineData("seven")]
    [InlineData(true)]
    public void TryCoerce_NumberFromOtherValues_Fails(object value)
    {
        Assert.False(s_number.TryCoerce(value, out _));
    }

    [Fact]
    public void TryCoerce_Boolean_AcceptsBooleansAndTheirText()
    {
        Assert.Equal(true, Coerce(s_boolean, true));
        Assert.Equal(false, Coerce(s_boolean, " FALSE "));
        Assert.Equal(true, Coerce(s_boolean, JsonValue.Create(true)));
        Assert.False(s_boolean.TryCoerce("yes", out _));
        Assert.False(s_boolean.TryCoerce(1, out _));
    }

    [Fact]
    public void TryCoerce_DateTime_ReturnsUtc()
    {
        var expected = new DateTime(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);

        Assert.Equal(expected, Coerce(s_dateTime, "2026-10-06T09:00:00Z"));
        Assert.Equal(expected, Coerce(s_dateTime, "2026-10-06T11:00:00+02:00"));
        Assert.Equal(expected, Coerce(s_dateTime, "2026-10-06T09:00:00"));
        Assert.Equal(expected, Coerce(s_dateTime, new DateTimeOffset(2026, 10, 6, 11, 0, 0, TimeSpan.FromHours(2))));
        Assert.Equal(DateTimeKind.Utc, ((DateTime)Coerce(s_dateTime, new DateTime(2026, 10, 6, 9, 0, 0, DateTimeKind.Unspecified))).Kind);
        Assert.False(s_dateTime.TryCoerce("tomorrow", out _));
    }

    [Fact]
    public void TryCoerce_Object_ReturnsADictionary()
    {
        var fromJson = Assert.IsType<Dictionary<string, object>>(Coerce(s_object, new JsonObject { ["name"] = "Ada", ["age"] = 36 }));
        Assert.Equal("Ada", fromJson["name"]);

        var fromText = Assert.IsType<Dictionary<string, object>>(Coerce(s_object, "{\"name\":\"Ada\"}"));
        Assert.Equal("Ada", fromText["name"]);

        var fromAnonymous = Assert.IsType<Dictionary<string, object>>(Coerce(s_object, new { Name = "Ada" }));
        Assert.Equal("Ada", fromAnonymous["Name"]);

        IDictionary<string, object> expando = new ExpandoObject();
        expando["x"] = 1;
        Assert.Same(expando, Coerce(s_object, expando));

        Assert.False(s_object.TryCoerce("[1]", out _));
        Assert.False(s_object.TryCoerce(5, out _));
        Assert.False(s_object.TryCoerce(new List<object>(), out _));
    }

    [Fact]
    public void TryCoerce_Array_ReturnsAList()
    {
        Assert.Equal(new object[] { "a", "b" }, Assert.IsType<List<object>>(Coerce(s_array, new[] { "a", "b" })));
        Assert.Equal(2, Assert.IsType<List<object>>(Coerce(s_array, new JsonArray(1, 2))).Count);
        Assert.Single(Assert.IsType<List<object>>(Coerce(s_array, "[\"x\"]")));
        Assert.False(s_array.TryCoerce("{}", out _));
        Assert.False(s_array.TryCoerce("text", out _));
    }

    [Fact]
    public void TryCoerce_Any_KeepsTheValueAndConvertsJson()
    {
        var value = new { A = 1 };

        Assert.Same(value, Coerce(s_any, value));
        Assert.Equal("x", Coerce(s_any, JsonValue.Create("x")));
    }

    private static object Coerce(IWorkflowVariableType type, object value)
    {
        Assert.True(type.TryCoerce(value, out var result), $"{type.Name} should accept {value}.");

        return result;
    }
}
