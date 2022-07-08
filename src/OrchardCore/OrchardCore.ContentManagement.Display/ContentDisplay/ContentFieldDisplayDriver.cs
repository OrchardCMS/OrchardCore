using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentFieldDisplayDriver<TField> : DisplayDriverBase, IContentFieldDisplayDriver where TField : ContentField, new()
    {
        private const string DisplayToken = "_Display";
        private const string DisplaySeparator = "_Display__";

        private ContentTypePartDefinition _typePartDefinition;
        private ContentPartFieldDefinition _partFieldDefinition;

        public override ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
        {
            // e.g., HtmlBodyPart.Summary, HtmlBodyPart-BlogPost, BagPart-LandingPage-Services
            // context.Shape is the ContentItem shape, we need to alter the part shape

            var result = base.Factory(shapeType, shapeBuilder, initializeAsync).Prefix(Prefix);

            if (_typePartDefinition != null && _partFieldDefinition != null)
            {
                var partType = _typePartDefinition.PartDefinition.Name;
                var partName = _typePartDefinition.Name;
                var fieldType = _partFieldDefinition.FieldDefinition.Name;
                var fieldName = _partFieldDefinition.Name;
                var contentType = _typePartDefinition.ContentTypeDefinition.Name;
                var displayMode = _partFieldDefinition.DisplayMode();
                var hasDisplayMode = !String.IsNullOrEmpty(displayMode);

                if (GetEditorShapeType(_partFieldDefinition) == shapeType)
                {
                    // HtmlBodyPart-Description, Services-Description
                    result.Differentiator($"{partName}-{fieldName}");

                    // We do not need to add alternates on edit as they are handled with field editor types so return before adding alternates
                    return result;
                }

                // If the shape type and the field type only differ by the display mode
                if (hasDisplayMode && shapeType == fieldType + DisplaySeparator + displayMode)
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

                result.Displaying(ctx =>
                {
                    var displayTypes = new[] { "", "_" + ctx.Shape.Metadata.DisplayType };

                    // [ShapeType]_[DisplayType], e.g. TextField.Summary
                    ctx.Shape.Metadata.Alternates.Add($"{shapeType}_{ctx.Shape.Metadata.DisplayType}");

                    // When the shape type is the same as the field, we can ignore one of them in the alternate name
                    // For instance TextField returns a unique TextField shape type.
                    if (shapeType == fieldType)
                    {
                        foreach (var displayType in displayTypes)
                        {
                            // [PartType]__[FieldName], e.g. HtmlBodyPart-Description
                            ctx.Shape.Metadata.Alternates.Add($"{partType}{displayType}__{fieldName}");

                            // [ContentType]__[FieldType], e.g. Blog-TextField, LandingPage-TextField
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{fieldType}");

                            // [ContentType]__[PartName]__[FieldName], e.g. Blog-HtmlBodyPart-Description, LandingPage-Services-Description
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{partType}__{fieldName}");
                        }
                    }
                    else
                    {
                        if (hasDisplayMode)
                        {
                            // [FieldType]_[DisplayType]__[DisplayMode]_Display, e.g. TextField-Header.Display.Summary
                            ctx.Shape.Metadata.Alternates.Add($"{fieldType}_{ctx.Shape.Metadata.DisplayType}__{displayMode}{DisplayToken}");
                        }

                        for (var i = 0; i < displayTypes.Length; i++)
                        {
                            var displayType = displayTypes[i];

                            if (hasDisplayMode)
                            {
                                shapeType = $"{fieldType}__{displayMode}";

                                if (displayType == "")
                                {
                                    displayType = DisplayToken;
                                }
                                else
                                {
                                    shapeType += DisplayToken;
                                }
                            }

                            // [FieldType]__[ShapeType], e.g. TextField-TextFieldSummary
                            ctx.Shape.Metadata.Alternates.Add($"{fieldType}{displayType}__{shapeType}");

                            // [PartType]__[FieldName]__[ShapeType], e.g. HtmlBodyPart-Description-TextFieldSummary
                            ctx.Shape.Metadata.Alternates.Add($"{partType}{displayType}__{fieldName}__{shapeType}");

                            // [ContentType]__[FieldType]__[ShapeType], e.g. Blog-TextField-TextFieldSummary, LandingPage-TextField-TextFieldSummary
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{fieldType}__{shapeType}");

                            // [ContentType]__[PartName]__[FieldName]__[ShapeType], e.g. Blog-HtmlBodyPart-Description-TextFieldSummary, LandingPage-Services-Description-TextFieldSummary
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{partName}__{fieldName}__{shapeType}");
                        }
                    }
                });
            }

            return result;
        }

        Task<IDisplayResult> IContentFieldDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
        {
            if (!String.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
               !String.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var field = contentPart.Get<TField>(partFieldDefinition.Name);
            if (field != null)
            {
                BuildPrefix(typePartDefinition, partFieldDefinition, context.HtmlFieldPrefix);

                var fieldDisplayContext = new BuildFieldDisplayContext(contentPart, typePartDefinition, partFieldDefinition, context);

                _typePartDefinition = typePartDefinition;
                _partFieldDefinition = partFieldDefinition;

                var result = DisplayAsync(field, fieldDisplayContext);

                _typePartDefinition = null;
                _partFieldDefinition = null;

                return result;
            }

            return Task.FromResult(default(IDisplayResult));
        }

        Task<IDisplayResult> IContentFieldDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
        {
            if (!String.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
                !String.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var field = contentPart.GetOrCreate<TField>(partFieldDefinition.Name);

            if (field != null)
            {
                BuildPrefix(typePartDefinition, partFieldDefinition, context.HtmlFieldPrefix);

                var fieldEditorContext = new BuildFieldEditorContext(contentPart, typePartDefinition, partFieldDefinition, context);

                _typePartDefinition = typePartDefinition;
                _partFieldDefinition = partFieldDefinition;

                var result = EditAsync(field, fieldEditorContext);

                _typePartDefinition = null;
                _partFieldDefinition = null;

                return result;
            }

            return Task.FromResult(default(IDisplayResult));
        }

        async Task<IDisplayResult> IContentFieldDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
        {
            if (!String.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
                !String.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return null;
            }

            var field = contentPart.GetOrCreate<TField>(partFieldDefinition.Name);

            BuildPrefix(typePartDefinition, partFieldDefinition, context.HtmlFieldPrefix);

            var updateFieldEditorContext = new UpdateFieldEditorContext(contentPart, typePartDefinition, partFieldDefinition, context);

            _typePartDefinition = typePartDefinition;
            _partFieldDefinition = partFieldDefinition;

            var result = await UpdateAsync(field, context.Updater, updateFieldEditorContext);

            _typePartDefinition = null;
            _partFieldDefinition = null;

            if (result == null)
            {
                return null;
            }

            contentPart.Apply(partFieldDefinition.Name, field);

            return result;
        }

        public virtual Task<IDisplayResult> DisplayAsync(TField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Task.FromResult(Display(field, fieldDisplayContext));
        }

        public virtual Task<IDisplayResult> EditAsync(TField field, BuildFieldEditorContext context)
        {
            return Task.FromResult(Edit(field, context));
        }

        public virtual Task<IDisplayResult> UpdateAsync(TField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            return Task.FromResult(Update(field, updater, context));
        }

        public virtual IDisplayResult Display(TField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return null;
        }

        public virtual IDisplayResult Edit(TField field, BuildFieldEditorContext context)
        {
            return null;
        }

        public virtual IDisplayResult Update(TField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            return null;
        }

        protected string GetEditorShapeType(string shapeType, ContentPartFieldDefinition partFieldDefinition)
        {
            var editor = partFieldDefinition.Editor();
            return !String.IsNullOrEmpty(editor)
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
            return !String.IsNullOrEmpty(displayMode)
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

            if (!String.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = htmlFieldPrefix + "." + Prefix;
            }
        }
    }
}
