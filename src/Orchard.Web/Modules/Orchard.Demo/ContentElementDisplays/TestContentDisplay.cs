using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.Demo.Models;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace Orchard.Demo.ContentElementDisplays
{
    public class TestContentElementDisplay : ContentElementDisplay
    {
        private static int _creating;
        private static int _processing;

        public override DisplayResult<IContent> BuildDisplay(BuildDisplayContext<IContent> context)
        {
            var testContentPart = context.Model.As<TestContentPartA>();

            if (testContentPart == null)
            {
                return null;
            }

            return Combine(
                // A new shape is created and the properties of the object are bound to it when rendered
                Shape("TestContentPartA", testContentPart).Location("Content"),
                // New shape, no initialization, custom location
                Shape("LowerDoll").Location("Footer"),
                // New shape 
                Shape("TestContentPartA",
                    ctx => ctx.New.TestContentPartA().Creating(_creating++),
                    shape =>
                    {
                        shape.Processing = _processing++;
                        return Task.CompletedTask;
                    })
                    .Location("Content")
                    .Cache("lowerdoll2", cache => cache.During(TimeSpan.FromSeconds(5))),
                // A strongly typed shape model is used and initialized when rendered
                Shape<TestContentPartAShape>(shape => { shape.Line = "Strongly typed shape"; return Task.CompletedTask; })
                    .Location("Content:2"),
                // Cached shape
                Shape("LowerDoll")
                    .Location("/Footer")
                    .Cache("lowerdoll", cache => cache.During(TimeSpan.FromSeconds(5)))
                );
        }

        public override DisplayResult<IContent> BuildEditor(BuildEditorContext<IContent> context)
        {
            var testContentPart = context.Model.As<TestContentPartA>();

            if (testContentPart == null)
            {
                return null;
            }

            return Shape("TestContentPartA_Edit", testContentPart).Location("Content");
        }

        public override async Task<DisplayResult<IContent>> UpdateEditorAsync(UpdateEditorContext<IContent> context, IUpdateModel updater)
        {
            var testContentPart = context.Model.As<TestContentPartA>();

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
                    context.Model.ContentItem.Weld(testContentPart);
                }
            }
            
            return Shape("TestContentPartA_Edit", testContentPart).Location("Content");
        }
    }    
}
