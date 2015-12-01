using Orchard.DisplayManagement.Layout;
using System;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.AspNet.Html.Abstractions;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using System.IO;

namespace Orchard.DisplayManagement.Razor
{
    public abstract class RazorPage<TModel> : Microsoft.AspNet.Mvc.Razor.RazorPage<TModel>
    {
        private dynamic _display;
        public dynamic Display
        {
            get
            {
                if (_display == null)
                {
                    IDisplayHelperFactory _factory = ViewContext.HttpContext.RequestServices.GetService(typeof(IDisplayHelperFactory)) as IDisplayHelperFactory;
                    _display = _factory.CreateHelper(ViewContext); 
                }

                return _display;
            }

            set
            {
                _display = value;
            }
        }

        private dynamic _themeLayout;
        public dynamic ThemeLayout
        {
            get
            {
                if (_themeLayout == null)
                {
                    var layoutAccessor = ViewContext.HttpContext.RequestServices.GetService(typeof(ILayoutAccessor)) as ILayoutAccessor;

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

        protected HelperResult RenderLayoutBody()
        {
            var result = base.RenderBody();
            return result;
        }

        protected override HelperResult RenderBody()
        {
            return new HelperResult(tw =>
            {
                // TODO: Implement async
                var content = (IHtmlContent)Display(ThemeLayout.Content);
                content.WriteTo(tw, HtmlEncoder);
                return Task.CompletedTask;
            });
        }

        public new Task<HtmlString> RenderSectionAsync(string name, bool required)
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

            var content = (IHtmlContent)Display(zone);
            using (var sw = new StringWriter())
            {
                content.WriteTo(sw, HtmlEncoder);
                return Task.FromResult(new HtmlString(sw.ToString()));
            }
        }
    }

    public abstract class RazorPage : RazorPage<dynamic>
    {
    }
}
