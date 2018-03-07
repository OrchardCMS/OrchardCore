using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DeferredTasks;
using OrchardCore.Demo.Models;
using OrchardCore.Demo.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITestDependency _testDependency;
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly ILogger _logger;
        private readonly ITagCache _tagCache;
        private readonly IContentItemDisplayManager _contentDisplay;
        private readonly IDeferredTaskEngine _deferredTaskEngine;

        public HomeController(
            ITestDependency testDependency,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            ISession session,
            ILogger<HomeController> logger,
            ITagCache tagCache,
            IContentItemDisplayManager contentDisplay,
            IDeferredTaskEngine processingQueue)
        {
            _deferredTaskEngine = processingQueue;
            _session = session;
            _testDependency = testDependency;
            _contentManager = contentManager;
            Shape = shapeFactory;
            _logger = logger;
            _tagCache = tagCache;
            _contentDisplay = contentDisplay;
        }

        dynamic Shape { get; set; }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string text)
        {
            var contentItem = await _contentManager.NewAsync("Foo");

            // Dynamic syntax
            contentItem.Content.TestContentPartA.Line = text + "blah";

            // Explicit syntax
            var testPart = contentItem.As<TestContentPartA>();
            testPart.Line = text;
            contentItem.Apply(testPart);

            // "Alter" syntax
            contentItem.Alter<TestContentPartA>(x => x.Line = text);

            await _contentManager.CreateAsync(contentItem);

            _logger.LogInformation("This is some log");

            return RedirectToAction("Display", "Home", new { area = "OrchardCore.Demo", contentItemId = contentItem.ContentItemId });
        }

        public ActionResult Tag()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Tag(string tag)
        {
            await _tagCache.RemoveTagAsync(tag);
            return RedirectToAction("Tag", "Home", new { area = "OrchardCore.Demo" });
        }

        public async Task<ActionResult> Display(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            return View("Display", contentItem);
        }


        public async Task<ActionResult> DisplayShape(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
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

        public ActionResult Cache()
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
            throw new Exception("ERROR!!!!");
        }

        public string CreateTask()
        {
            _deferredTaskEngine.AddTask(context =>
            {
                var logger = context.ServiceProvider.GetService<ILogger<HomeController>>();
                logger.LogError("Task deferred successfully");
                return Task.CompletedTask;
            });

            return "Check for logs";
        }

        public IActionResult ShapePerformance()
        {
            return View();
        }
    }
}