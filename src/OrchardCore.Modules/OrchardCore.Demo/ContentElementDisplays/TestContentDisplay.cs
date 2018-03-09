using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Demo.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Demo.ContentElementDisplays
{
    public class TestContentElementDisplay : ContentDisplayDriver
    {
        private static int _creating;
        private static int _processing;

        public override IDisplayResult Display(ContentItem contentItem, IUpdateModel updater)
        {
            var testContentPart = contentItem.As<TestContentPartA>();

            if (testContentPart == null)
            {
                return null;
            }

            return Combine(
                // A new shape is created and the properties of the object are bound to it when rendered
                Shape("TestContentPartA", testContentPart).Location("Detail", "Content"),
                // New shape, no initialization, custom location
                Shape("LowerDoll").Location("Detail", "Footer"),
                // New shape
                Shape("TestContentPartA",
                    async ctx => (await ctx.New.TestContentPartA()).Creating(_creating++),
                    shape =>
                    {
                        ((dynamic)shape).Processing = _processing++;
                        return Task.CompletedTask;
                    })
                    .Location("Detail", "Content")
                    .Cache("lowerdoll2", cache => cache.WithDuration(TimeSpan.FromSeconds(5))),
                // A strongly typed shape model is used and initialized when rendered
                Shape<TestContentPartAShape>(shape => { shape.Line = "Strongly typed shape"; return Task.CompletedTask; })
                    .Location("Detail", "Content:2"),
                // Cached shape
                Shape("LowerDoll")
                    .Location("Detail", "/Footer")
                    .Cache("lowerdoll", cache => cache.WithDuration(TimeSpan.FromSeconds(5)))
                );
        }

        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            var testContentPart = contentItem.As<TestContentPartA>();

            if (testContentPart == null)
            {
                return null;
            }

            return Shape("TestContentPartA_Edit", testContentPart).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var testContentPart = contentItem.As<TestContentPartA>();

            if (testContentPart == null)
            {
                return null;
            }

            if (await updater.TryUpdateModelAsync(testContentPart, ""))
            {
                if (testContentPart.Line.EndsWith(" "))
                {
                    updater.ModelState.AddModelError(nameof(testContentPart.Line), "Value cannot end with a space");
                }
                else
                {
                    contentItem.Apply(testContentPart);
                }
            }

            return Shape("TestContentPartA_Edit", testContentPart).Location("Content");
        }
    }
}
