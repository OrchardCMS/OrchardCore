using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Demo.Models;
using Orchard.Demo.Services;
using Orchard.Demo.TestEvents;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Events;
using System;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITestDependency _testDependency;
        private readonly IContentManager _contentManager;
        private readonly IEventBus _eventBus;
        private readonly IDisplayManager _displayManager;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ISession _session;
		
        public HomeController(
            ITestDependency testDependency,
            IContentManager contentManager,
            IEventBus eventBus,
            IShapeFactory shapeFactory,
            IDisplayManager displayManager,
            IShapeDisplay shapeDisplay,
            ISession session)
        {
            _session = session;
            _testDependency = testDependency;
            _contentManager = contentManager;
            _eventBus = eventBus;
            _displayManager = displayManager;
            _shapeDisplay = shapeDisplay;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string text)
        {
            var contentItem = _contentManager.New("Foo");
            contentItem.As<TestContentPartA>().Line = text;
            _contentManager.Create(contentItem);

            var shape = Shape.Foo();

            return RedirectToAction("Display", "Home", new { area = "Orchard.Demo", id = contentItem.ContentItemId });
        }

        public async Task<ActionResult> Display(int id)
        {
            var contentItem = await _contentManager.Get(id);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            return View(contentItem);
        }
		
        public ActionResult Raw()
        {
            return View();
        }

        public string GCCollect()
        {
            GC.Collect();
            return "OK";
        }

        public ActionResult IndexError()
        {
            throw new System.Exception("ERROR!!!!");
        }
    }

    public class TestContentPartAHandler : ContentHandlerBase
    {
        public override void Activating(ActivatingContentContext context)
        {
            context.Builder.Weld<TestContentPartA>();
        }
    }
}