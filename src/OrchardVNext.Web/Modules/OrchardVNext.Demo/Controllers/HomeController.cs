using System.Linq;
using Microsoft.AspNet.Mvc;
using OrchardVNext.Data;
using OrchardVNext.Demo.Models;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly DataContext _dataContext;

        public HomeController(ITestDependency testDependency,
            DataContext dataContext) {
            _testDependency = testDependency;
            _dataContext = dataContext;
        }

        public ActionResult Index() {

            var testRecord = new TestRecord { TestLine = "foo"};
            _dataContext.Set<TestRecord>().Add(testRecord);
            _dataContext.SaveChanges();

            // Always returning 0!?
            var p = _dataContext.Set<TestRecord>().Where(x => x.TestLine == "foo").ToList();
            Logger.Debug("Records returned {0}", p.Count());






            return View("Index", _testDependency.SayHi());
        }
    }
}