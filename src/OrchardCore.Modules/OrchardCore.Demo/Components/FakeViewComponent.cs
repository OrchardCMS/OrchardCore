using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Demo.Components
{
    public class FakeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string value)
        {
            return View("Default", value);
        }
    }
}
