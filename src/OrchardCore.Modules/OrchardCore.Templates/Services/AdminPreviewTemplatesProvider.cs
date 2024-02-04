using System;
using Microsoft.AspNetCore.Http;
using OrchardCore.Templates.Models;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Services
{
    /// <summary>
    /// And instance of this class provides custom templates to use while previewing a page.
    /// </summary>
    public class AdminPreviewTemplatesProvider
    {
        private readonly Lazy<AdminTemplatesDocument> _templatesDocument;

        public AdminPreviewTemplatesProvider(IHttpContextAccessor httpContextAccessor)
        {
            _templatesDocument = new Lazy<AdminTemplatesDocument>(() =>
            {
                var httpContext = httpContextAccessor.HttpContext;

                var templatesDocument = new AdminTemplatesDocument();

                if (httpContext.Items.TryGetValue("OrchardCore.PreviewTemplate", out var model))
                {
                    if (model is not TemplateViewModel viewModel || viewModel.Name == null)
                    {
                        return templatesDocument;
                    }

                    var template = new Template { Content = viewModel.Content };
                    templatesDocument.Templates.Add(viewModel.Name, template);
                }

                return templatesDocument;
            });
        }

        public AdminTemplatesDocument GetTemplates()
        {
            return _templatesDocument.Value;
        }
    }
}
