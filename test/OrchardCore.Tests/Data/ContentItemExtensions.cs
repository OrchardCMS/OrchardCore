using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using Xunit;

namespace OrchardCore.Tests.Data
{
    public class ContentItemExtensions
    {
        [Fact]
        public void GetReflectsChangesMadeByApply()
        {
            // arrange
            var value = "changed value";
            var contentItem = new ContentItem();
            contentItem.Weld<CustomPart>();
            var part = new CustomPart { Value = value };

            // act
            contentItem.Apply(nameof(CustomPart), part);

            // assert
            var actual = contentItem.Get<CustomPart>(nameof(CustomPart));
            Assert.Equal(actual.Value, value);
        }

        [Fact]
        public void GetReflectsChangesToFieldMadeByApply()
        {
            // arrange
            var value = "changed value";
            var contentItem = new ContentItem();
            contentItem.Weld<CustomPart>();
            var part = contentItem.Get<CustomPart>(nameof(CustomPart));
            var field = part.GetOrCreate<TextField>(nameof(CustomPart.Field));
            field.Text = value;

            // act
            part.Apply(nameof(CustomPart.Field), field);

            // assert
            var actual = contentItem.Get<CustomPart>(nameof(CustomPart));
            Assert.Equal(actual.Field?.Text, value);
        }

        public class CustomPart : ContentPart
        {
            public string Value { get; set; }
            public TextField Field { get; set; }
        }
    }
}
