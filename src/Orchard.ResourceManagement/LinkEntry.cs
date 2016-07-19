using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.ResourceManagement
{
    public class LinkEntry
    {
        private readonly TagBuilder _builder = new TagBuilder("link");

        public string Condition { get; set; }

        public string Rel
        {
            get
            {
                string value;
                _builder.Attributes.TryGetValue("rel", out value);
                return value;
            }
            set { SetAttribute("rel", value); }
        }

        public string Type
        {
            get
            {
                string value;
                _builder.Attributes.TryGetValue("type", out value);
                return value;
            }
            set { SetAttribute("type", value); }
        }

        public string Title
        {
            get
            {
                string value;
                _builder.Attributes.TryGetValue("title", out value);
                return value;
            }
            set { SetAttribute("title", value); }
        }

        public string Href
        {
            get
            {
                string value;
                _builder.Attributes.TryGetValue("href", out value);
                return value;
            }
            set { SetAttribute("href", value); }
        }

        public IHtmlContent GetTag()
        {
            _builder.TagRenderMode = TagRenderMode.SelfClosing;
            string tag = _builder.ToString();

            if (!string.IsNullOrEmpty(Condition))
            {
                return new HtmlString("<!--[if " + Condition + "]>" + tag + "<![endif]-->");
            }

            return new HtmlString(tag);
        }

        public LinkEntry AddAttribute(string name, string value)
        {
            _builder.MergeAttribute(name, value);
            return this;
        }

        public LinkEntry SetAttribute(string name, string value)
        {
            _builder.MergeAttribute(name, value, true);
            return this;
        }
    }
}