using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.RazorPages
{
    public abstract class Page : Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        private dynamic _displayHelper;
        private IShapeFactory _shapeFactory;
        private IOrchardDisplayHelper _orchardHelper;
        private ISite _site;

        public override ViewContext ViewContext
        {
            get => base.ViewContext;
            set
            {
                // We make the ViewContext available to other sub-systems that need it.
                var viewContextAccessor = value.HttpContext.RequestServices.GetService<ViewContextAccessor>();
                base.ViewContext = viewContextAccessor.ViewContext = value;
            }
        }

        private void EnsureDisplayHelper()
        {
            if (_displayHelper == null)
            {
                _displayHelper = HttpContext.RequestServices.GetService<IDisplayHelper>();
            }
        }

        private void EnsureShapeFactory()
        {
            if (_shapeFactory == null)
            {
                _shapeFactory = HttpContext.RequestServices.GetService<IShapeFactory>();
            }
        }

        /// <summary>
        /// Gets a dynamic shape factory to create new shapes.
        /// </summary>
        /// <example>
        /// Usage:
        /// <code>
        /// await New.MyShape()
        /// await New.MyShape(A: 1, B: "Some text")
        /// (await New.MyShape()).A(1).B("Some text")
        /// </code>
        /// </example>
        public dynamic New => Factory;

        /// <summary>
        /// Gets an <see cref="IShapeFactory"/> to create new shapes.
        /// </summary>
        public IShapeFactory Factory
        {
            get
            {
                EnsureShapeFactory();
                return _shapeFactory;
            }
        }

        /// <summary>
        /// Renders a shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        public Task<IHtmlContent> DisplayAsync(dynamic shape)
        {
            EnsureDisplayHelper();
            return (Task<IHtmlContent>)_displayHelper(shape);
        }

        public IOrchardDisplayHelper Orchard
        {
            get
            {
                if (_orchardHelper == null)
                {
                    EnsureDisplayHelper();
                    _orchardHelper = new OrchardDisplayHelper(HttpContext, _displayHelper);
                }

                return _orchardHelper;
            }
        }

        private dynamic _themeLayout;

        public dynamic ThemeLayout
        {
            get
            {
                if (_themeLayout == null)
                {
                    _themeLayout = HttpContext.Features.Get<RazorViewFeature>()?.ThemeLayout;
                }

                return _themeLayout;
            }

            set
            {
                _themeLayout = value;
            }
        }

        public string ViewLayout
        {
            get
            {
                if (ThemeLayout is IShape layout)
                {
                    if (layout.Metadata.Alternates.Count > 0)
                    {
                        return layout.Metadata.Alternates.Last;
                    }

                    return layout.Metadata.Type;
                }

                return String.Empty;
            }

            set
            {
                if (ThemeLayout is IShape layout)
                {
                    if (layout.Metadata.Alternates.Contains(value))
                    {
                        if (layout.Metadata.Alternates.Last == value)
                        {
                            return;
                        }

                        layout.Metadata.Alternates.Remove(value);
                    }

                    layout.Metadata.Alternates.Add(value);
                }
            }
        }

        private IPageTitleBuilder _pageTitleBuilder;

        public IPageTitleBuilder Title
        {
            get
            {
                if (_pageTitleBuilder == null)
                {
                    _pageTitleBuilder = HttpContext.RequestServices.GetRequiredService<IPageTitleBuilder>();
                }

                return _pageTitleBuilder;
            }
        }

        private IViewLocalizer _t;

        /// <summary>
        /// The <see cref="IViewLocalizer"/> instance for the current view.
        /// </summary>
        public IViewLocalizer T
        {
            get
            {
                if (_t == null)
                {
                    _t = HttpContext.RequestServices.GetRequiredService<IViewLocalizer>();
                    ((IViewContextAware)_t).Contextualize(ViewContext);
                }

                return _t;
            }
        }

        /// <summary>
        /// Adds a segment to the title and returns all segments.
        /// </summary>
        /// <param name="segment">The segment to add to the title.</param>
        /// <param name="position">Optional. The position of the segment in the title.</param>
        /// <param name="separator">The html string that should separate all segments.</param>
        /// <returns>And <see cref="IHtmlContent"/> instance representing the full title.</returns>
        public IHtmlContent RenderTitleSegments(IHtmlContent segment, string position = "0", IHtmlContent separator = null)
        {
            Title.AddSegment(segment, position);
            return Title.GenerateTitle(separator);
        }

        /// <summary>
        /// Adds a segment to the title and returns all segments.
        /// </summary>
        /// <param name="segment">The segment to add to the title.</param>
        /// <param name="position">Optional. The position of the segment in the title.</param>
        /// <param name="separator">The html string that should separate all segments.</param>
        /// <returns>And <see cref="IHtmlContent"/> instance representing the full title.</returns>
        public IHtmlContent RenderTitleSegments(string segment, string position = "0", IHtmlContent separator = null)
        {
            Title.AddSegment(new StringHtmlContent(segment), position);
            return Title.GenerateTitle(separator);
        }

        /// <summary>
        /// Creates a <see cref="TagBuilder"/> to render a shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns>A new <see cref="TagBuilder"/>.</returns>
        public TagBuilder Tag(dynamic shape)
        {
            return Shape.GetTagBuilder(shape);
        }

        public TagBuilder Tag(dynamic shape, string tag)
        {
            return Shape.GetTagBuilder(shape, tag);
        }

        public object OrDefault(object text, object other)
        {
            if (text == null || Convert.ToString(text) == "")
            {
                return other;
            }

            return text;
        }

        /// <summary>
        /// Returns the full escaped path of the current request.
        /// </summary>
        public string FullRequestPath => HttpContext.Request.PathBase + HttpContext.Request.Path + HttpContext.Request.QueryString;

        /// <summary>
        /// Gets the <see cref="ISite"/> instance.
        /// </summary>
        public ISite Site
        {
            get
            {
                if (_site == null)
                {
                    _site = HttpContext.Features.Get<RazorViewFeature>()?.Site;
                }

                return _site;
            }
        }
    }
}
