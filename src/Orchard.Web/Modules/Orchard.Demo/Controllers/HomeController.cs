using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Demo.Models;
using Orchard.Demo.Services;
using Orchard.Demo.TestEvents;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Events;

namespace Orchard.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IContentManager _contentManager;
        private readonly IEventNotifier _eventNotifier;
        private readonly IDisplayManager _displayManager;
        private readonly IShapeDisplay _shapeDisplay;

        public HomeController(ITestDependency testDependency,
            IContentManager contentManager,
            IEventNotifier eventNotifier,
            IShapeFactory shapeFactory,
            IDisplayManager displayManager,
            IShapeDisplay shapeDisplay
            ) {
            _testDependency = testDependency;
            _contentManager = contentManager;
            _eventNotifier = eventNotifier;
            _displayManager = displayManager;
            _shapeDisplay = shapeDisplay;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        public ActionResult Index()
        {
            var foo = Shape.Foo();
            var foostring = _shapeDisplay.Display(foo);


            _eventNotifier.Notify<ITestEvent>(e => e.Talk("Bark!"));

            var contentItem = _contentManager.New("Foo");
            contentItem.As<TestContentPartA>().Line = "Orchard VNext Rocks";
            _contentManager.Create(contentItem);

            var retrieveContentItem = _contentManager.Get(contentItem.Id);
            var lineToSay = retrieveContentItem.As<TestContentPartA>().Line;

            return View("Index", new HomeViewModel { Text =_testDependency.SayHi(lineToSay), Foo = foostring });
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