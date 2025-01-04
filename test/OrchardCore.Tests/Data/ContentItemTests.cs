using System.Text.Json;
using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Tests.Data;

public class ContentItemTests
{
    [Fact]
    public void NullValueDateTimeFieldSerializationTest()
    {
        // Arrange
        var jsonStr = """
         {
            "NullValueDateTimeFieldTest": {
                "Value": null 
            }
        }
        """;

        var jObject = JsonNode.Parse(jsonStr);

        var nullValueDateTimeField = jObject.SelectNode("NullValueDateTimeFieldTest").ToObject<DateTimeField>();
        Assert.Null(nullValueDateTimeField.Value);
        Assert.Null(JObject.FromObject(nullValueDateTimeField).SelectNode("Value"));
    }

    /// <summary>
    /// To validate <see cref="DateTimeJsonConverter"/>
    /// and <seealso cref="TimeSpanJsonConverter"/>
    /// </summary>
    [Fact]
    public void JsonNode_WhenParseCalled_ConvertShortTimeFormatToTimeField()
    {
        // Arrange
        var jsonStr = """
         {
            "TimeFieldTest": {
                "Value": "13:05"
            },
            "DateTimeFieldTest": {
                "Value": "2024-5-31 13:05"
            },
            "TimezoneDateTimeFieldTest": {
                "Value": "2022-12-13T21:02:18.399-05:00"
            },
            "DateFieldTest": {
                "Value": "2024-5-31"
            }
        }
        """;

        // Act
        var jObject = JsonNode.Parse(jsonStr);
        var timeField = jObject.SelectNode("TimeFieldTest").ToObject<TimeField>();
        var dateField = jObject.SelectNode("DateFieldTest").ToObject<DateField>();
        var dateTimeField = jObject.SelectNode("DateTimeFieldTest").ToObject<DateTimeField>();
        var timeZoneDateTimeFieldTest = jObject.SelectNode("TimezoneDateTimeFieldTest").ToObject<DateTimeField>();

        // Assert
        Assert.Equal("13:05:00", timeField.Value.Value.ToString());
        Assert.Equal("2024-05-31", dateField.Value.Value.ToString("yyyy-MM-dd"));
        Assert.Equal("2024-05-31 13:05", dateTimeField.Value.Value.ToString("yyyy-MM-dd HH:mm"));
        Assert.Equal("13:05:00", JObject.FromObject(timeField).SelectNode("Value").ToString());
        Assert.Equal("2024-05-31T00:00:00Z", JObject.FromObject(dateField).SelectNode("Value").ToString());
        Assert.Equal("2024-05-31T13:05:00Z", JObject.FromObject(dateTimeField).SelectNode("Value").ToString());


        var utcTime = TimeZoneInfo.ConvertTimeToUtc(timeZoneDateTimeFieldTest.Value.Value);
        Assert.Equal("2022-12-14 02:02:18", utcTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public void JsonNode_WhenParseCalled_ThrowsJsonExceptionWithInvalidDateTime()
    {
        // Arrange
        var jsonStr = """
         {
            "EmptyValueDateTimeFieldTest": {
                "Value": ""
            },
            "ErrorFormatDateTimeFieldTest": {
                "Value": "ErrorFormatValue"
            }
        }
        """;

        // Act
        var jobject = JsonNode.Parse(jsonStr);

        // Assert
        _ = Assert.Throws<JsonException>(() => jobject.SelectNode("EmptyValueDateTimeFieldTest").ToObject<DateTimeField>());

        _ = Assert.Throws<JsonException>(() => jobject.SelectNode("ErrorFormatDateTimeFieldTest").ToObject<DateTimeField>());
    }

    [Fact]
    public void ShouldSerializeContent()
    {
        var contentItem = new ContentItem
        {
            Id = 1,
            ContentItemId = "2",
            ContentType = "Page",
            Latest = true,
            Published = true,
        };

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.Equal(0, contentItem2.Id); // Should be 0 as we dont serialize it.
        Assert.Equal(contentItem.ContentItemId, contentItem2.ContentItemId);
        Assert.Equal(contentItem.ContentType, contentItem2.ContentType);
        Assert.Equal(contentItem.Latest, contentItem2.Latest);
        Assert.Equal(contentItem.Published, contentItem2.Published);
    }

    [Fact]
    public void ShouldSerializeParts()
    {
        var contentItem = new ContentItem();
        var myPart = new MyPart { Text = "test" };
        contentItem.Weld(myPart);

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.NotNull(contentItem2.Content.MyPart);
        Assert.Equal("test", (string)contentItem2.Content.MyPart.Text);
    }

    [Fact]
    public void ShouldAlterPart()
    {
        var contentItem = CreateContentItemWithMyPart();

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.NotNull(contentItem2.Content.MyPart);
        Assert.Equal("test", (string)contentItem2.Content.MyPart.Text);
    }

    [Fact]
    public void ContentShouldOnlyContainParts()
    {
        var contentItem = CreateContentItemWithMyPart();

        var json = JConvert.SerializeObject(contentItem);

        Assert.Contains(@"""MyPart"":{""Text"":""test""}", json);
    }

    [Fact]
    public void ContentShouldStoreFields()
    {
        var contentItem = CreateContentItemWithMyPart();
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyField>("myField");
            x.Alter<MyField>("myField", f => f.Value = 123);
        });

        var json = JConvert.SerializeObject(contentItem);

        Assert.Contains(@"""MyPart"":{""Text"":""test"",""myField"":{""Value"":123}}", json);
    }

    [Fact]
    public void ContentShouldBeJsonPathQueryable()
    {
        var contentItem = CreateContentItemWithMyPart();
        JsonNode contentItemJson = contentItem.Content;
        JsonNode contentPartJson = contentItem.As<MyPart>().Content;

        // The content part should be selectable from the content item.
        var selectedItemNode = contentItemJson.SelectNode("MyPart");
        Assert.NotNull(selectedItemNode);
        Assert.Equal(selectedItemNode.ToJsonString(), contentPartJson.ToJsonString());

        // Verify that SelectNode queries the subtree of the node it's called on (not the document root).
        var textPropertyNode = selectedItemNode.SelectNode("Text");
        AssertJsonEqual(textPropertyNode, JObject.Parse(selectedItemNode.ToJsonString()).SelectNode("Text"));

        // Verify consistent results when targeting the same node in different ways.
        AssertJsonEqual(textPropertyNode, contentPartJson.SelectNode("Text"));
        AssertJsonEqual(textPropertyNode, contentItemJson.SelectNode("MyPart.Text"));
        AssertJsonEqual(textPropertyNode, contentItemJson.SelectNode("$..Text"));
    }

    [Fact]
    public void RemovingPropertyShouldWork()
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = "test");

        JsonDynamicObject content = contentItem.Content;
        Assert.Null(content.GetValue("not real property")); // Properties that don't exist return null.
        Assert.NotNull(content.GetValue(nameof(MyPart))); // Right now this property exists.

        content.Remove(nameof(MyPart));
        Assert.Null(content.GetValue(nameof(MyPart)));
    }

    [Fact]
    public void ContentShouldCanCallRemoveMethod()
    {
        var contentItem = CreateContentItemWithMyPart();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        Assert.Equal("test", contentItem.As<MyPart>().Text);
        Assert.True(contentItem.Content.Remove("MyPart"));
    }

    [Fact]
    public void ShouldDeserializeContentField()
    {
        var contentItem = CreateContentItemWithMyPart();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyField>("myField");
            x.Alter<MyField>("myField", f => f.Value = 123);
        });

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.NotNull(contentItem2.Content.MyPart);
        Assert.NotNull(contentItem2.Content.MyPart.myField);
        Assert.Equal(123, (int)contentItem2.Content.MyPart.myField.Value);
    }

    [Fact]
    public void ContentShouldStoreDateTimeFields()
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyDateTimeField>("myField");
            x.Alter<MyDateTimeField>("myField", f => f.Value = new DateTime(2024, 1, 1, 10, 42, 0));
        });

        var json = JConvert.SerializeObject(contentItem);

        Assert.Contains(@"""MyPart"":{""Text"":""test"",""myField"":{""Value"":""2024-01-01T10:42:00Z""}}", json);
    }

    [Fact]
    public void ShouldDeserializeDateTimeFields()
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyDateTimeField>("myField");
            x.Alter<MyDateTimeField>("myField", f => f.Value = new DateTime(2024, 1, 1, 10, 42, 0));
        });

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.NotNull(contentItem2.Content.MyPart);
        Assert.NotNull(contentItem2.Content.MyPart.myField);
        Assert.Equal(new DateTime(2024, 1, 1, 10, 42, 0), (DateTime?)contentItem2.Content.MyPart.myField.Value);
    }

    [Fact]
    public void ContentShouldStoreUtcDateTimeFields()
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyDateTimeField>("myField");
            x.Alter<MyDateTimeField>("myField", f => f.Value = new DateTime(2024, 1, 1, 10, 42, 0, DateTimeKind.Utc));
        });

        var json = JConvert.SerializeObject(contentItem);

        Assert.Contains(@"""MyPart"":{""Text"":""test"",""myField"":{""Value"":""2024-01-01T10:42:00Z""}}", json);
    }

    [Fact]
    public void ShouldDeserializeUtcDateTimeFields()
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyDateTimeField>("myField");
            x.Alter<MyDateTimeField>("myField", f => f.Value = new DateTime(2024, 1, 1, 10, 42, 0, DateTimeKind.Utc));
        });

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.NotNull(contentItem2.Content.MyPart);
        Assert.NotNull(contentItem2.Content.MyPart.myField);
        Assert.Equal(new DateTime(2024, 1, 1, 10, 42, 0, DateTimeKind.Utc), (DateTime?)contentItem2.Content.MyPart.myField.Value);
    }

    [Fact]
    public void ShouldDeserializeTextFields()
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = "test");
        contentItem.Alter<MyPart>(x =>
        {
            x.GetOrCreate<MyTextField>("myField");
            x.Alter<MyTextField>("myField", f => f.Text = "This is a test field entry");
        });

        var json = JConvert.SerializeObject(contentItem);

        var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

        Assert.NotNull(contentItem2.Content.MyPart);
        Assert.NotNull(contentItem2.Content.MyPart.myField);
        Assert.Equal("This is a test field entry", (string)contentItem2.Content.MyPart.myField.Text);
    }

    private static ContentItem CreateContentItemWithMyPart(string text = "test")
    {
        var contentItem = new ContentItem();
        contentItem.GetOrCreate<MyPart>();
        contentItem.Alter<MyPart>(x => x.Text = text);

        return contentItem;
    }

    private static void AssertJsonEqual(JsonNode expected, JsonNode actual)
    {
        Assert.NotNull(expected);
        Assert.NotNull(actual);
        Assert.Equal(expected.ToJsonString(), actual.ToJsonString());
    }
}

public sealed class MyPart : ContentPart
{
    public string Text { get; set; }
}

public sealed class MyField : ContentField
{
    public int Value { get; set; }
}

public sealed class MyDateTimeField : ContentField
{
    public DateTime? Value { get; set; }
}

public sealed class MyTextField : ContentField
{
    public string Text { get; set; }
}

public sealed class GetOnlyListPart : ContentPart
{
    public IList<string> Texts { get; } = [];
}
