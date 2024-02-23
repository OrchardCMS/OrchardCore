using System.Text.Json;
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
            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "test");

            var json = JConvert.SerializeObject(contentItem);

            var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

            Assert.NotNull(contentItem2.Content.MyPart);
            Assert.Equal("test", (string)contentItem2.Content.MyPart.Text);
        }

        [Fact]
        public void ShouldAlterPart()
        {
            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "test");

            var json = JConvert.SerializeObject(contentItem);

            var contentItem2 = JConvert.DeserializeObject<ContentItem>(json);

            Assert.NotNull(contentItem2.Content.MyPart);
            Assert.Equal("test", (string)contentItem2.Content.MyPart.Text);
        }

        [Fact]
        public void ContentShouldOnlyContainParts()
        {
            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "test");

            var json = JConvert.SerializeObject(contentItem);

            Assert.Contains(@"""MyPart"":{""Text"":""test""}", json);
        }

        [Fact]
        public void ContentShouldStoreFields()
        {
            var contentItem = new ContentItem();
            contentItem.GetOrCreate<MyPart>();
            contentItem.Alter<MyPart>(x => x.Text = "test");
            contentItem.Alter<MyPart>(x =>
            {
                x.GetOrCreate<MyField>("myField");
                x.Alter<MyField>("myField", f => f.Value = 123);
            });

            var json = JConvert.SerializeObject(contentItem);

            Assert.Contains(@"""MyPart"":{""Text"":""test"",""myField"":{""Value"":123}}", json);
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
}
