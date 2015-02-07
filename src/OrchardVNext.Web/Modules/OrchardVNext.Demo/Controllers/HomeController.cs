using System.Linq;
using Microsoft.AspNet.Mvc;
using OrchardVNext.Data;
using OrchardVNext.Demo.Models;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IDataContextLocator _dataContextLocator;

        public HomeController(ITestDependency testDependency,
            IDataContextLocator dataContextLocator) {
            _testDependency = testDependency;
            _dataContextLocator = dataContextLocator;
        }

        public ActionResult Index() {
            var context = _dataContextLocator.For(typeof (TestRecord));

            var testRecord = new TestRecord { TestLine = "foo"};
            context.Set<TestRecord>().Add(testRecord);
            context.SaveChanges();

            // Always returning 0!?
            var p = context.Set<TestRecord>().Where(x => x.TestLine == "foo").ToList();
            Logger.Debug("Records returned {0}", p.Count());
            return View("Index", _testDependency.SayHi());
        }
    }
}