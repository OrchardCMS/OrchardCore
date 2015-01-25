using System.Linq;
using Microsoft.AspNet.Mvc;
using OrchardVNext.Data;
using OrchardVNext.Demo.Models;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IRepository<TestRecord> _testRepository;

        public HomeController(ITestDependency testDependency,
            IRepository<TestRecord> testRepository) {
            _testDependency = testDependency;
            _testRepository = testRepository;
        }

        public ActionResult Index() {
            var testRecord = new TestRecord { TestLine = "foo"};
            _testRepository.Create(testRecord);
            // Always returning 0!?
            var p = _testRepository.Table.Where(x => x.TestLine == "foo").ToList();
            Logger.Debug("Records returned {0}", p.Count());
            return View("Index", _testDependency.SayHi());
        }
    }
}