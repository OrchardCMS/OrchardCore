using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Controllers
{
    [Admin]
    public class PreviewController : Controller, IUpdateModel
    {
        private readonly IMemoryCache _memoryCache;

        public PreviewController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public void SetTemplate()
        {
            var name = Request.Form["Name"];
            var content = Request.Form["Content"];

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
            {
                _memoryCache.Set("OrchardCore.PreviewTemplate", new TemplateViewModel { Name = name, Content = content },
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(30)));
            }
        }
    }
}
