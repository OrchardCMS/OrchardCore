using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Demo.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Demo.ContentElementDisplays;

public sealed class TestContentElementDisplayDriver : ContentDisplayDriver
{
    private static int _creating;
    private static int _processing;

    public override IDisplayResult Display(ContentItem contentItem, BuildDisplayContext context)
    {
        var testContentPart = contentItem.As<TestContentPartA>();

        if (testContentPart == null)
        {
            return null;
        }

        return Combine(
            // A new shape is created and the properties of the object are bound to it when rendered
            Copy("TestContentPartA", testContentPart).Location("Detail", "Content"),
            // New shape, no initialization, custom location
            Dynamic("LowerDoll").Location("Detail", "Footer"),
            // New shape
            Factory("TestContentPartA",
                async ctx => (await ctx.New.TestContentPartA()).Creating(_creating++),
                shape =>
                {
                    shape.Properties["Processing"] = _processing++;

                    return Task.CompletedTask;
                })
                .Location("Detail", "Content")
                .Cache("lowerdoll2", cache => cache.WithExpiryAfter(TimeSpan.FromSeconds(5))),
            // A strongly typed shape model is used and initialized when rendered
            Initialize<TestContentPartAShape>(shape => { shape.Line = "Strongly typed shape"; })
                .Location("Detail", "Content:2"),
            // Cached shape
            Dynamic("LowerDoll")
                .Location("Detail", "/Footer")
                .Cache("lowerdoll", cache => cache.WithExpiryAfter(TimeSpan.FromSeconds(5)))
            );
    }

    public override IDisplayResult Edit(ContentItem contentItem, BuildEditorContext context)
    {
        var testContentPart = contentItem.As<TestContentPartA>();

        if (testContentPart == null)
        {
            return null;
        }

        return Copy("TestContentPartA_Edit", testContentPart).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentItem contentItem, UpdateEditorContext context)
    {
        var testContentPart = contentItem.As<TestContentPartA>();

        if (testContentPart == null)
        {
            return null;
        }

        await context.Updater.TryUpdateModelAsync(testContentPart, "");

        if (testContentPart.Line.EndsWith(' '))
        {
            context.Updater.ModelState.AddModelError(nameof(testContentPart.Line), "Value cannot end with a space");
        }
        else
        {
            contentItem.Apply(testContentPart);
        }

        return Copy("TestContentPartA_Edit", testContentPart).Location("Content");
    }
}
