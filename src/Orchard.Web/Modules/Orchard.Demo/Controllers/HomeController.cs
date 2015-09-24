using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Demo.Models;
using Orchard.Demo.Services;
using Orchard.Demo.TestEvents;
using Orchard.Events;

namespace Orchard.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IContentManager _contentManager;
        private readonly IEventNotifier _eventNotifier;

        public HomeController(ITestDependency testDependency,
            IContentManager contentManager,
            IEventNotifier eventNotifier
            ) {
            _testDependency = testDependency;
            _contentManager = contentManager;
            _eventNotifier = eventNotifier;
           
            }

        public ActionResult Index()
        {

            _eventNotifier.Notify<ITestEvent>(e => e.Talk("Bark!"));

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