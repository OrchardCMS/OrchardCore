using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

public abstract class ContentFieldDisplayDriver<TField> : DisplayDriverBase, IContentFieldDisplayDriver where TField : ContentField, new()
{
    private const string DisplaySeparator = "_Display__";

    private ContentTypePartDefinition _typePartDefinition;
    private ContentPartFieldDefinition _partFieldDefinition;
    private bool _isDisplay;

    public override ShapeResult Factory<TBuilderState, TInitState>(string shapeType, Func<IBuildShapeContext, TBuilderState, ValueTask<IShape>> shapeBuilder, TBuilderState shapeBuilderState, Func<IShape, TInitState, ValueTask> initializingAsync, TInitState initializingState)
    {
        // e.g., HtmlBodyPart.Summary, HtmlBodyPart-BlogPost, BagPart-LandingPage-Services
        // context.Shape is the ContentItem shape, we need to alter the part shape

        var result = base.Factory(shapeType, shapeBuilder, shapeBuilderState, initializingAsync, initializingState);

        if (_typePartDefinition != null && _partFieldDefinition != null)
        {
            var partName = _typePartDefinition.Name;
            var fieldType = _partFieldDefinition.FieldDefinition.Name;
            var fieldName = _partFieldDefinition.Name;
            var displayMode = _partFieldDefinition.DisplayMode();
            var editorShapeType = GetEditorShapeType(_partFieldDefinition);

            if (editorShapeType == shapeType)
            {
                // HtmlBodyPart-Description, Services-Description
                result.Differentiator($"{partName}-{fieldName}");

                // We do not need to add alternates on edit as they are handled with field editor types so return before adding alternates
                return result;
            }

            // If the shape type and the field type only differ by the display mode
            if (!string.IsNullOrEmpty(displayMode) && shapeType == fieldType + DisplaySeparator + displayMode)
            {
                // Preserve the shape name regardless its differentiator
                result.Name($"{partName}-{fieldName}");
            }

            if (fieldType == shapeType)
            {
                // HtmlBodyPart-Description, Services-Description
                result.Differentiator($"{partName}-{fieldName}");
            }
            else
            {
                // HtmlBodyPart-Description-TextField, Services-Description-TextField
                result.Differentiator($"{partName}-{fieldName}-{shapeType}");
            }

            // The definitions will be cleared before the displaying event is raised, so
            // we need to cache it here for use in the displaying event to add alternates.
            var typePartDefinition = _typePartDefinition;
            var partFieldDefinition = _partFieldDefinition;
            var displayFieldName = _isDisplay && partFieldDefinition.GetSettings<ContentPartFieldSettings>().DisplayFieldName;
            result.Displaying(ctx =>
            {
                var displayType = ctx.Shape.Metadata.DisplayType;

                // Get cached alternates for this display type and add them efficiently
                var cachedAlternates = ContentFieldAlternatesFactory.GetAlternates(typePartDefinition, partFieldDefinition, shapeType, displayType);
                ctx.Shape.Metadata.Alternates.AddRange(cachedAlternates);

                // The wrapper renders the name of the field, and decides for which display types.
                if (displayFieldName)
                {
                    ctx.Shape.Properties[ContentFieldDisplayConstants.PartFieldDefinitionProperty] = partFieldDefinition;
                    ctx.Shape.Metadata.Wrappers.Add(ContentFieldDisplayConstants.WrapperShapeType);
                }
            });
        }

        return result;
    }

    Task<IDisplayResult> IContentFieldDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
    {
        if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal) &&
           !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
        {
            return Task.FromResult(default(IDisplayResult));
        }

        var field = contentPart.Get<TField>(partFieldDefinition.Name);
        if (field != null)
        {
            if (IsEmpty(field) && !partFieldDefinition.GetSettings<ContentPartFieldSettings>().AlwaysDisplay)
            {
                return Task.FromResult(default(IDisplayResult));
            }

            BuildPrefix(typePartDefinition, partFieldDefinition, context.HtmlFieldPrefix);

            var fieldDisplayContext = new BuildFieldDisplayContext(contentPart, typePartDefinition, partFieldDefinition, context);

            _typePartDefinition = typePartDefinition;
            _partFieldDefinition = partFieldDefinition;
            _isDisplay = true;

            var result = DisplayAsync(field, fieldDisplayContext);

            _typePartDefinition = null;
            _partFieldDefinition = null;
            _isDisplay = false;

            return result;
        }

        return Task.FromResult(default(IDisplayResult));
    }

    Task<IDisplayResult> IContentFieldDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
    {
        if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal) &&
            !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
        {
            return Task.FromResult(default(IDisplayResult));
        }

        var field = contentPart.GetOrCreate<TField>(partFieldDefinition.Name);

        BuildPrefix(typePartDefinition, partFieldDefinition, context.HtmlFieldPrefix);

        var fieldEditorContext = new BuildFieldEditorContext(contentPart, typePartDefinition, partFieldDefinition, context);

        _typePartDefinition = typePartDefinition;
        _partFieldDefinition = partFieldDefinition;

        var result = EditAsync(field, fieldEditorContext);

        _typePartDefinition = null;
        _partFieldDefinition = null;

        return result;
    }

    async Task<IDisplayResult> IContentFieldDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
    {
        if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal) &&
            !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name, StringComparison.Ordinal))
        {
            return null;
        }

        var field = contentPart.GetOrCreate<TField>(partFieldDefinition.Name);

        BuildPrefix(typePartDefinition, partFieldDefinition, context.HtmlFieldPrefix);

        var updateFieldEditorContext = new UpdateFieldEditorContext(contentPart, typePartDefinition, partFieldDefinition, context);

        _typePartDefinition = typePartDefinition;
        _partFieldDefinition = partFieldDefinition;

        var result = await UpdateAsync(field, updateFieldEditorContext);

        _typePartDefinition = null;
        _partFieldDefinition = null;

        if (result == null)
        {
            return null;
        }

        contentPart.Apply(partFieldDefinition.Name, field);

        return result;
    }

    /// <summary>
    /// Returns whether the field has no value. A field without a value is only displayed when its
    /// <see cref="ContentPartFieldSettings.AlwaysDisplay"/> setting is enabled.
    /// </summary>
    /// <param name="field">The field to check.</param>
    /// <returns><see langword="true"/> when the field has no value to display; by default <see langword="false"/>.</returns>
    protected virtual bool IsEmpty(TField field)
    {
        return false;
    }

    public virtual Task<IDisplayResult> DisplayAsync(TField field, BuildFieldDisplayContext fieldDisplayContext)
    {
        return Task.FromResult(Display(field, fieldDisplayContext));
    }

    public virtual IDisplayResult Display(TField field, BuildFieldDisplayContext fieldDisplayContext)
    {
        return null;
    }

    public virtual Task<IDisplayResult> EditAsync(TField field, BuildFieldEditorContext context)
    {
        return Task.FromResult(Edit(field, context));
    }

    public virtual IDisplayResult Edit(TField field, BuildFieldEditorContext context)
    {
        return null;
    }

    public virtual Task<IDisplayResult> UpdateAsync(TField field, UpdateFieldEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(null);
    }

    protected string GetEditorShapeType(string shapeType, ContentPartFieldDefinition partFieldDefinition)
    {
        var editor = partFieldDefinition.Editor();
        return !string.IsNullOrEmpty(editor)
            ? shapeType + "__" + editor
            : shapeType;
    }

    protected string GetEditorShapeType(string shapeType, BuildFieldEditorContext context)
    {
        return GetEditorShapeType(shapeType, context.PartFieldDefinition);
    }

    protected string GetEditorShapeType(ContentPartFieldDefinition partFieldDefinition)
    {
        return GetEditorShapeType(typeof(TField).Name + "_Edit", partFieldDefinition);
    }

    protected string GetEditorShapeType(BuildFieldEditorContext context)
    {
        return GetEditorShapeType(context.PartFieldDefinition);
    }

    protected string GetDisplayShapeType(string shapeType, BuildFieldDisplayContext context)
    {
        var displayMode = context.PartFieldDefinition.DisplayMode();
        return !string.IsNullOrEmpty(displayMode)
            ? shapeType + DisplaySeparator + displayMode
            : shapeType;
    }

    protected string GetDisplayShapeType(BuildFieldDisplayContext context)
    {
        return GetDisplayShapeType(typeof(TField).Name, context);
    }

    private void BuildPrefix(ContentTypePartDefinition typePartDefinition, ContentPartFieldDefinition partFieldDefinition, string htmlFieldPrefix)
    {
        Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;

        if (!string.IsNullOrEmpty(htmlFieldPrefix))
        {
            Prefix = htmlFieldPrefix + "." + Prefix;
        }
    }
}
