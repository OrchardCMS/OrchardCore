using System.Text.Json;
using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Data
{
    public class ContentItemTests
    {
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
        public void ShouldUpdateContent()
        {
            var contentItem = CreateContentItemWithMyPart();

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
        public void ShouldDeserializeListContentPart()
        {
            var contentItem = CreateContentItemWithMyPart();
            contentItem.Alter<MyPart>(x => x.Text = "test");
            contentItem.Alter<MyPart>(x =>
            {
                x.GetOrCreate<MyField>("myField");
                x.Alter<MyField>("myField", f => f.Value = 123);
            });

            var json = JConvert.SerializeObject(contentItem);

            Assert.Contains(@"""MyPart"":{""Text"":""test"",""myField"":{""Value"":123}}", json);
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

    public class MyPart : ContentPart
    {
        public string Text { get; set; }
    }

    public class MyField : ContentField
    {
        public int Value { get; set; }
    }

    public class GetOnlyListPart : ContentPart
    {
        public IList<string> Texts { get; } = new List<string>();
    }
}
