using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.SpaServices.Settings;

namespace OrchardCore.SpaServices.Controllers
{
    public class HomeController : Controller
    {
        readonly ISiteService _siteService;
        readonly ISpaStaticFileProvider _spaStaticFileProvider;
        readonly IStringLocalizer T;

        public HomeController(ISiteService siteService, ISpaStaticFileProvider spaStaticFileProvider, IStringLocalizer<HomeController> stringLocalizer)
        {
            _siteService = siteService;
            _spaStaticFileProvider = spaStaticFileProvider;
            T = stringLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            var site = await _siteService.GetSiteSettingsAsync();
            var settings = site.As<SpaServicesSettings>();
            if (settings.UseStaticFile)
            {
                var file = string.IsNullOrWhiteSpace(settings.StaticFile) ? "index.html" : settings.StaticFile;
                var fileInfo = _spaStaticFileProvider.FileProvider.GetFileInfo(file);
                if (fileInfo.Exists)
                {
                    using (var sr = new StreamReader(fileInfo.CreateReadStream()))
                    {
                        return Content(await sr.ReadToEndAsync(), "text/html; charset=utf-8");
                    }
                }
                else
                {
                    HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Content(T["File `{0}` not found. Please check the settings and/or upload your SPA", file]);
                }
            }
            else
                return View();
        }
    }
}