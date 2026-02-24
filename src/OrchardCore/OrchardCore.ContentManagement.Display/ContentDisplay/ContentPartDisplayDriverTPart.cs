using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Any concrete implementation of this class can provide shapes for any content item which has a specific Part.
/// </summary>
/// <typeparam name="TPart"></typeparam>
public abstract class ContentPartDisplayDriver<TPart> : DisplayDriverBase, IContentPartDisplayDriver where TPart : ContentPart, new()
{
    private const string DisplayToken = "_Display";
    private const string DisplaySeparator = "_Display__";

    private ContentTypePartDefinition _typePartDefinition;

    public override ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
    {
        // e.g., HtmlBodyPart.Summary, HtmlBodyPart-BlogPost, BagPart-LandingPage-Services
        // context.Shape is the ContentItem shape, we need to alter the part shape

        var result = base.Factory(shapeType, shapeBuilder, initializeAsync).Prefix(Prefix);

        if (_typePartDefinition != null)
        {
            // Get or create cached alternates for this part configuration
            var alternatesCollection = ContentPartAlternatesFactory.GetOrCreate(_typePartDefinition, shapeType);

            var partName = alternatesCollection.PartName;
            var partType = alternatesCollection.PartType;

            // If the shape type and the field type only differ by the display mode
            if (alternatesCollection.HasDisplayMode && alternatesCollection.IsDisplayModeShapeType)
            {
                // Preserve the shape name regardless its differentiator
                result.Name(partName);
            }

            if (partType == shapeType || alternatesCollection.EditorPartType == shapeType || alternatesCollection.IsDisplayModeShapeType)
            {
                // HtmlBodyPart, Services
                result.Differentiator(partName);
            }
            else
            {
                // ListPart-ListPartFeed
                result.Differentiator($"{partName}-{shapeType}");
            }

            result.Displaying(ctx =>
            {
                var displayType = ctx.Shape.Metadata.DisplayType;

                // Get cached alternates for this display type and add them efficiently
                var cachedAlternates = alternatesCollection.GetAlternates(displayType);
                ctx.Shape.Metadata.Alternates.AddRange(cachedAlternates);
            });
        }

        return result;
    }

    async Task<IDisplayResult> IContentPartDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
    {
        var part = contentPart as TPart;

        if (part == null)
        {
            return null;
        }

        using (BuildPrefix(typePartDefinition, context.HtmlFieldPrefix))
        {
            _typePartDefinition = typePartDefinition;

            var buildDisplayContext = new BuildPartDisplayContext(typePartDefinition, context);

            var result = await DisplayAsync(part, buildDisplayContext);

            _typePartDefinition = null;

            return result;
        }
    }

    async Task<IDisplayResult> IContentPartDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
    {
        var part = contentPart as TPart;

        if (part == null)
        {
            return null;
        }

        using (BuildPrefix(typePartDefinition, context.HtmlFieldPrefix))
        {
            _typePartDefinition = typePartDefinition;

            var buildEditorContext = new BuildPartEditorContext(typePartDefinition, context);

            var result = await EditAsync(part, buildEditorContext);

            _typePartDefinition = null;

            return result;
        }
    }

    async Task<IDisplayResult> IContentPartDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
    {
        var part = contentPart as TPart;

        if (part == null)
        {
            return null;
        }

        using (BuildPrefix(typePartDefinition, context.HtmlFieldPrefix))
        {
            var updateEditorContext = new UpdatePartEditorContext(typePartDefinition, context);

            _typePartDefinition = typePartDefinition;

            var result = await UpdateAsync(part, updateEditorContext);

            part.ContentItem.Apply(typePartDefinition.Name, part);

            _typePartDefinition = null;

            return result;
        }
    }

    public virtual Task<IDisplayResult> DisplayAsync(TPart part, BuildPartDisplayContext context)
    {
        return Task.FromResult(Display(part, context));
    }

    public virtual IDisplayResult Display(TPart part, BuildPartDisplayContext context)
    {
        return null;
    }

    public virtual Task<IDisplayResult> EditAsync(TPart part, BuildPartEditorContext context)
    {
        return Task.FromResult(Edit(part, context));
    }

    public virtual IDisplayResult Edit(TPart part, BuildPartEditorContext context)
    {
        return null;
    }

    public virtual Task<IDisplayResult> UpdateAsync(TPart part, UpdatePartEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(null);
    }

    protected string GetEditorShapeType(string shapeType, ContentTypePartDefinition typePartDefinition)
    {
        var editor = typePartDefinition.Editor();
        return !string.IsNullOrEmpty(editor)
            ? shapeType + "__" + editor
            : shapeType;
    }

    protected string GetEditorShapeType(string shapeType, BuildPartEditorContext context)
    {
        return GetEditorShapeType(shapeType, context.TypePartDefinition);
    }

    protected string GetEditorShapeType(ContentTypePartDefinition typePartDefinition)
    {
        return GetEditorShapeType(typeof(TPart).Name + "_Edit", typePartDefinition);
    }

    protected string GetEditorShapeType(BuildPartEditorContext context)
    {
        return GetEditorShapeType(context.TypePartDefinition);
    }

    protected string GetDisplayShapeType(string shapeType, BuildPartDisplayContext context)
    {
        var displayMode = context.TypePartDefinition.DisplayMode();
        return !string.IsNullOrEmpty(displayMode)
            ? shapeType + DisplaySeparator + displayMode
            : shapeType;
    }

    protected string GetDisplayShapeType(BuildPartDisplayContext context)
    {
        return GetDisplayShapeType(typeof(TPart).Name, context);
    }

    private TempPrefix BuildPrefix(ContentTypePartDefinition typePartDefinition, string htmlFieldPrefix)
    {
        var tempPrefix = new TempPrefix(this, Prefix);

        Prefix = typePartDefinition.Name;

        if (!string.IsNullOrEmpty(htmlFieldPrefix))
        {
            Prefix = htmlFieldPrefix + "." + Prefix;
        }

        return tempPrefix;
    }

    /// <summary>
    /// Restores the previous prefix automatically.
    /// </summary>
    private sealed class TempPrefix : IDisposable
    {
        private readonly ContentPartDisplayDriver<TPart> _driver;
        private readonly string _originalPrefix;

        public TempPrefix(ContentPartDisplayDriver<TPart> driver, string originalPrefix)
        {
            _driver = driver;
            _originalPrefix = originalPrefix;
        }

        public void Dispose()
        {
            _driver.Prefix = _originalPrefix;
        }
    }
}
