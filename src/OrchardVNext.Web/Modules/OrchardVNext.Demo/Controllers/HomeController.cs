using System.Linq;
using Microsoft.AspNet.Mvc;
using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data;
using OrchardVNext.Demo.Services;
using OrchardVNext.Test1;

namespace OrchardVNext.Demo.Controllers {
    public class HomeController : Controller {
        private readonly ITestDependency _testDependency;
        private readonly IContentStorageProvider _contentStorageProvider;
        private readonly DataContext _dataContext;

        public HomeController(ITestDependency testDependency,
            IContentStorageProvider contentStorageProvider,
            DataContext dataContext) {
            _testDependency = testDependency;
            _contentStorageProvider = contentStorageProvider;
            _dataContext = dataContext;
            }

        public ActionResult Index()
        {

            var contentItem = new ContentItem
            {
                VersionRecord = new ContentItemVersionRecord
                {
                    ContentItemRecord = new ContentItemRecord(),
                    Number = 1,
                    Latest = true,
                    Published = true
                }
            };

            contentItem.VersionRecord.ContentItemRecord.Versions.Add(contentItem.VersionRecord);

            _contentStorageProvider.Store(contentItem);

            var retrievedRecord = _contentStorageProvider.Get(contentItem.Id);

            var indexedRetrievedRecords = _contentStorageProvider.GetMany(x => x.Id == 1);



            





            return View("Index", _testDependency.SayHi());
        }
    }

    [Persistent]
    public class Foo
    {
        public int Id { get; set; }
    }
}