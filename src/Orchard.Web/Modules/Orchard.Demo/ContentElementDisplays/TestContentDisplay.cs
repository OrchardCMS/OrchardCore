using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Handlers;
using Orchard.ContentManagement.Display.Views;
using Orchard.ContentManagement;
using Orchard.Demo.Models;
using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Demo.ContentElementDisplays
{
    public class TestContentElementDisplay : ContentElementDisplay
    {
        private static int _creating;
        private static int _processing;

        public override DisplayResult BuildDisplay(BuildDisplayContext context)
        {
            var testContentPart = context.Content.As<TestContentPartA>();

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

        public override DisplayResult BuildEditor(BuildEditorContext context)
        {
            var testContentPart = context.Content.As<TestContentPartA>();

            if (testContentPart == null)
            {
                return null;
            }

            return Shape("TestContentPartA_Edit", testContentPart).Location("Content");
        }

        public override async Task<DisplayResult> UpdateEditorAsync(UpdateEditorContext context, IModelUpdater updater)
        {
            var testContentPart = context.Content.As<TestContentPartA>();

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
                    context.Content.ContentItem.Weld(testContentPart);
                }
            }
            
            return Shape("TestContentPartA_Edit", testContentPart).Location("Content");
        }
    }    
}
