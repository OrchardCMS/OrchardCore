using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Sitemaps.Controllers
{
    public class TestController : Controller
    {

        public IActionResult Index()
        {
            return Ok();
        }
    }
}
