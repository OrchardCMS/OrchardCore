using Microsoft.AspNet.Mvc;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _foo;

        public HomeController(ITestDependency foo) {
            _foo = foo;
        }

        public ActionResult Index() {


            return View();

        }
    }
}