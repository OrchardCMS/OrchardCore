using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Search.Azure.CognitiveSearch.Controllers;
public class AdminController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
