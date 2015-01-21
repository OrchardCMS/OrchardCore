using Microsoft.AspNet.Mvc;
using OrchardVNext.Data;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;

        public HomeController(ITestDependency testDependency) {
            _testDependency = testDependency;
        }

        public ActionResult Index() {
            return View("Index", _testDependency.SayHi());
        }
    }
}