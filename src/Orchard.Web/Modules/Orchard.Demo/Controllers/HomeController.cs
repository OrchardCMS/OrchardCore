using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Demo.Models;
using Orchard.Test1;

namespace Orchard.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IContentManager _contentManager;

        public HomeController(ITestDependency testDependency,
            IContentManager contentManager) {
            _testDependency = testDependency;
            _contentManager = contentManager;
            }

        public ActionResult Index()
        {
            var contentItem = _contentManager.New("Foo");
            contentItem.As<TestContentPartA>().Line = "Orchard VNext Rocks";
            _contentManager.Create(contentItem);

            var retrieveContentItem = _contentManager.Get(contentItem.Id);
            var lineToSay = retrieveContentItem.As<TestContentPartA>().Line;

            return View("Index", _testDependency.SayHi(lineToSay));
        }

        public ActionResult IndexError() {
            throw new System.Exception("ERROR!!!!");
        }
    }

    public class TestContentPartAHandler : ContentHandlerBase {
        public override void Activating(ActivatingContentContext context) {
            context.Builder.Weld<TestContentPartA>();
        }
    }
}