using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Orchard.Templates.Models;

namespace Orchard.Templates.Services
{
    /// <summary>
    /// And instance of this class provides custom templates to use while previewing a page.
    /// </summary>
    public class PreviewTemplatesProvider
    {
        private readonly Lazy<TemplatesDocument> _templatesDocument;

        public PreviewTemplatesProvider(IHttpContextAccessor httpContextAccessor)
        {
            _templatesDocument = new Lazy<TemplatesDocument>(() =>
            {

                var httpContext = httpContextAccessor.HttpContext;

                if (!httpContext.Request.Cookies.ContainsKey("orchard:templates"))
                {
                    return null;
                }

                var template = JsonConvert.DeserializeObject<Template>(httpContext.Request.Cookies["orchard:templates"]);
                var templatesDocument = new TemplatesDocument();
                templatesDocument.Templates.Add(template.Description, template);

                return templatesDocument;
            });

        }

        public TemplatesDocument GetTemplates()
        {
            return _templatesDocument.Value;
        }
    }
}