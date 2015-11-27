using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Demo.Models;
using Orchard.Demo.Services;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
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
        private readonly IShapeDisplay _shapeDisplay;
        private readonly ISession _session;

        public HomeController(
            ITestDependency testDependency,
            IContentManager contentManager,
            IEventBus eventBus,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay,
            ISession session)
        {
            _session = session;
            _testDependency = testDependency;
            _contentManager = contentManager;
            _eventBus = eventBus;
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

        public async Task<ActionResult> DisplayShape(int id)
        {
            var contentItem = await _contentManager.Get(id);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            var shape = Shape
                .Foo()
                .Line(contentItem.As<TestContentPartA>().Line);

            return View(shape);
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

        private DisplayContext CreateDisplayContext(Shape shape)
        {
            return new DisplayContext
            {
                Value = shape,
                ViewContext = new ViewContext()
            };
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