using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.Shapes;
using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Title;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Orchard.DisplayManagement.Razor
{
    public abstract class RazorPage<TModel> : Microsoft.AspNetCore.Mvc.Razor.RazorPage<TModel>
    {
        private dynamic _displayHelper;
        private IShapeFactory _shapeFactory;

        private void EnsureDisplayHelper()
        {
            if (_displayHelper == null)
            {
                IDisplayHelperFactory _factory = ViewContext.HttpContext.RequestServices.GetService<IDisplayHelperFactory>();
                _displayHelper = _factory.CreateHelper(ViewContext);
            }
        }

        private void EnsureShapeFactory()
        {
            if (_shapeFactory == null)
            {
                _shapeFactory = ViewContext.HttpContext.RequestServices.GetService<IShapeFactory>();
            }
        }

        public dynamic New
        {
            get
            {
                EnsureShapeFactory();
                return _shapeFactory;
            }
        }

        public IShapeFactory Factory
        {
            get
            {
                EnsureShapeFactory();
                return _shapeFactory;
            }
        }

        public IHtmlContent Display(dynamic shape)
        {
            EnsureDisplayHelper();
            return (IHtmlContent)_displayHelper(shape);
        }

        private dynamic _themeLayout;
        public dynamic ThemeLayout
        {
            get
            {
                if (_themeLayout == null)
                {
                    var layoutAccessor = ViewContext.HttpContext.RequestServices.GetService<ILayoutAccessor>();

                    if (layoutAccessor == null)
                    {
                        throw new InvalidOperationException("Could not find a valid layout accessor");
                    }

                    _themeLayout = layoutAccessor.GetLayout();
                }

                return _themeLayout;
            }

            set
            {
                _themeLayout = value;
            }
        }

        private IPageTitleBuilder _pageTitleBuilder;
        public IPageTitleBuilder Title
        {
            get
            {
                if (_pageTitleBuilder == null)
                {
                    _pageTitleBuilder = ViewContext.HttpContext.RequestServices.GetRequiredService<IPageTitleBuilder>();
                }

                return _pageTitleBuilder;
            }
        }

        private IViewLocalizer _t;
        public IViewLocalizer T
        {
            get
            {
                if (_t == null)
                {
                    _t = ViewContext.HttpContext.RequestServices.GetRequiredService<IViewLocalizer>();
                    ((IViewContextAware)_t).Contextualize(this.ViewContext);
                }

                return _t;
            }
        }

        public string RenderTitleSegments(string segment = "", string position = "0")
        {
            if (!String.IsNullOrWhiteSpace(segment))
            {
                Title.AddSegment(segment, position);
            }

            return Title.GenerateTitle();
        }

        protected HelperResult RenderLayoutBody()
        {
            var result = base.RenderBody();
            return result;
        }

        protected TagBuilder Tag(dynamic shape)
        {
            return Shape.GetTagBuilder(shape);
        }

        protected TagBuilder Tag(dynamic shape, string tag)
        {
            return Shape.GetTagBuilder(shape, tag);
        }

        protected new IHtmlContent RenderBody()
        {
            return Display(ThemeLayout.Content);
        }

        public new Task<IHtmlContent> RenderSectionAsync(string name, bool required)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var zone = ThemeLayout[name];

            if (required && zone != null && !zone.Items.Any())
            {
                throw new InvalidOperationException("Zone not found: " + name);
            }

            IHtmlContent result = Display(zone);
            return Task.FromResult(result);
        }
    }

    public abstract class RazorPage : RazorPage<dynamic>
    {
    }
}
