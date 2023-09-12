using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.ResourceManagement
{
    public class LinkEntry
    {
        private readonly TagBuilder _builder = new("link");

        public string Condition { get; set; }

        public LinkEntry()
        {
            _builder.TagRenderMode = TagRenderMode.SelfClosing;
        }

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

        public bool AppendVersion { get; set; }

        public IHtmlContent GetTag()
        {
            if (!String.IsNullOrEmpty(Condition))
            {
                var htmlBuilder = new HtmlContentBuilder();
                htmlBuilder.AppendHtml("<!--[if " + Condition + "]>");
                htmlBuilder.AppendHtml(_builder);
                htmlBuilder.AppendHtml("<![endif]-->");

                return htmlBuilder;
            }

            return _builder;
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
