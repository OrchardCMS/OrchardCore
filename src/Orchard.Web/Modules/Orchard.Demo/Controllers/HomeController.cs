using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Demo.Models;
using Orchard.Demo.Services;
using Orchard.Demo.TestEvents;
using Orchard.Events;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IContentManager _contentManager;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        public HomeController(
            ITestDependency testDependency,
            IContentManager contentManager,
            IEventBus eventBus,
            ISession session) {
            _session = session;
            _testDependency = testDependency;
            _contentManager = contentManager;
            _eventBus = eventBus;
            }

        public async Task<ActionResult> Index()
        {
            await _eventBus.NotifyAsync<ITestEvent>(e => e.Talk("Bark!"));

            var contentItem = _contentManager.New("Foo");
            contentItem.As<TestContentPartA>().Line = "Orchard VNext Rocks";
            _contentManager.Create(contentItem);
            
            var retrieveContentItem = await _contentManager.Get(contentItem.Id);
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