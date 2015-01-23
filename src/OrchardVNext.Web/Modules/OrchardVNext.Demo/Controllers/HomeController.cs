using Microsoft.AspNet.Mvc;
using OrchardVNext.Data;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IRepository<TestRecord> testRepository;

        public HomeController(ITestDependency testDependency,
            IRepository<TestRecord> testRepository) {
            _testDependency = testDependency;
        }

        public ActionResult Index() {
            return View("Index", _testDependency.SayHi());
        }
    }

    public class TestRecord {
        public int Id { get; set; }
        public string TestLine { get; set; }
    }
}