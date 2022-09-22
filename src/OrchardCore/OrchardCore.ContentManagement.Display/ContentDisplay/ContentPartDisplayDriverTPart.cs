using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
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
                // The stereotype is used when not displaying for a specific content type. We don't use [Stereotype] and [ContentType] at
                // the same time in an alternate because a content type is always of one stereotype.

                var partName = _typePartDefinition.Name;
                var partType = _typePartDefinition.PartDefinition.Name;
                var contentType = _typePartDefinition.ContentTypeDefinition.Name;
                var editorPartType = GetEditorShapeType(_typePartDefinition);
                var displayMode = _typePartDefinition.DisplayMode();
                var hasDisplayMode = !String.IsNullOrEmpty(displayMode);
                var isDisplayModeShapeType = shapeType == partType + DisplaySeparator + displayMode;
                var stereotype = _typePartDefinition.ContentTypeDefinition.GetStereotype() ?? String.Empty;

                // If the shape type and the field type only differ by the display mode
                if (hasDisplayMode && isDisplayModeShapeType)
                {
                    // Preserve the shape name regardless its differentiator
                    result.Name(partName);
                }

                if (partType == shapeType || editorPartType == shapeType || isDisplayModeShapeType)
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
                    string[] displayTypes;

                    if (editorPartType == shapeType)
                    {
                        displayTypes = new[] { "_" + ctx.Shape.Metadata.DisplayType };
                    }
                    else
                    {
                        displayTypes = new[] { "", "_" + ctx.Shape.Metadata.DisplayType };

                        if (!isDisplayModeShapeType)
                        {
                            // Do not add  Display type suffix to display mode shapes since display modes are already existing custom shapes.
                            // [ShapeType]_[DisplayType], e.g. HtmlBodyPart.Summary, BagPart.Summary, ListPartFeed.Summary
                            ctx.Shape.Metadata.Alternates.Add($"{shapeType}_{ctx.Shape.Metadata.DisplayType}");
                        }
                    }

                    if (shapeType == partType || shapeType == editorPartType)
                    {
                        foreach (var displayType in displayTypes)
                        {
                            // [ContentType]_[DisplayType]__[PartType], e.g. Blog-HtmlBodyPart, LandingPage-BagPart
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{partType}");

                            if (!String.IsNullOrEmpty(stereotype))
                            {
                                // [Stereotype]__[DisplayType]__[PartType], e.g. Widget-ContentsMetadata
                                ctx.Shape.Metadata.Alternates.Add($"{stereotype}{displayType}__{partType}");
                            }
                        }

                        if (partType != partName)
                        {
                            foreach (var displayType in displayTypes)
                            {
                                // [ContentType]_[DisplayType]__[PartName], e.g. LandingPage-Services
                                ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayType}__{partName}");

                                if (!String.IsNullOrEmpty(stereotype))
                                {
                                    // [Stereotype]_[DisplayType]__[PartType]__[PartName], e.g. Widget-ServicePart-Services
                                    ctx.Shape.Metadata.Alternates.Add($"{stereotype}{displayType}__{partType}__{partName}");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (hasDisplayMode)
                        {
                            // [PartType]_[DisplayType]__[DisplayMode]_Display, e.g. HtmlBodyPart-MyDisplayMode.Display.Summary
                            ctx.Shape.Metadata.Alternates.Add($"{partType}_{ctx.Shape.Metadata.DisplayType}__{displayMode}{DisplayToken}");
                        }

                        var lastAlternatesOfNamedPart = new List<string>();

                        foreach (var displayType in displayTypes)
                        {
                            var shapeTypeSuffix = shapeType;
                            var displayTypeDisplayToken = displayType;

                            if (hasDisplayMode)
                            {
                                if (isDisplayModeShapeType)
                                {
                                    // In case of display mode, update shape type to only include DisplayMode and DisplayToken
                                    shapeTypeSuffix = displayMode;
                                }
                                else
                                {
                                    shapeTypeSuffix = $"{shapeTypeSuffix}__{displayMode}";
                                }

                                if (displayType == "")
                                {
                                    displayTypeDisplayToken = DisplayToken;
                                }
                                else
                                {
                                    shapeTypeSuffix = $"{shapeTypeSuffix}{DisplayToken}";
                                }
                            }

                            // [ContentType]_[DisplayType]__[PartType]__[ShapeType], e.g. Blog-ListPart-ListPartFeed
                            ctx.Shape.Metadata.Alternates.Add($"{contentType}{displayTypeDisplayToken}__{partType}__{shapeTypeSuffix}");

                            if (!String.IsNullOrEmpty(stereotype))
                            {
                                // [Stereotype]_[DisplayType]__[PartType]__[ShapeType], e.g. Blog-ListPart-ListPartFeed
                                ctx.Shape.Metadata.Alternates.Add($"{stereotype}{displayTypeDisplayToken}__{partType}__{shapeTypeSuffix}");
                            }

                            if (partType != partName)
                            {
                                // [ContentType]_[DisplayType]__[PartName]__[ShapeType], e.g. LandingPage-Services-BagPartSummary
                                lastAlternatesOfNamedPart.Add($"{contentType}{displayTypeDisplayToken}__{partName}__{shapeTypeSuffix}");

                                if (!String.IsNullOrEmpty(stereotype))
                                {
                                    // [Stereotype]_[DisplayType]__[PartType]__[PartName]__[ShapeType], e.g. Widget-ServicePart-Services-BagPartSummary
                                    lastAlternatesOfNamedPart.Add($"{stereotype}{displayTypeDisplayToken}__{partType}__{partName}__{shapeTypeSuffix}");
                                }
                            }
                        }

                        ctx.Shape.Metadata.Alternates.AddRange(lastAlternatesOfNamedPart);
                    }
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

                var result = await UpdateAsync(part, context.Updater, updateEditorContext);

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
            return Display(part);
        }

        public virtual IDisplayResult Display(TPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(TPart part, BuildPartEditorContext context)
        {
            return Task.FromResult(Edit(part, context));
        }

        public virtual IDisplayResult Edit(TPart part, BuildPartEditorContext context)
        {
            return Edit(part);
        }

        public virtual IDisplayResult Edit(TPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            return UpdateAsync(part, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, UpdatePartEditorContext context)
        {
            return UpdateAsync(part, context.Updater);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, IUpdateModel updater)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        protected string GetEditorShapeType(string shapeType, ContentTypePartDefinition typePartDefinition)
        {
            var editor = typePartDefinition.Editor();
            return !String.IsNullOrEmpty(editor)
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
            return !String.IsNullOrEmpty(displayMode)
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

            if (!String.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = htmlFieldPrefix + "." + Prefix;
            }

            return tempPrefix;
        }

        /// <summary>
        /// Restores the previous prefix automatically
        /// </summary>
        private class TempPrefix : IDisposable
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
}
