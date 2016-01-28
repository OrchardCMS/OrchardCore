using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Demo.Controllers
{
    public class ContentApiController : Controller
    {
        [FromServices]
        public IContentManager ContentManager { get; set; }

        public async Task<IActionResult> GetById(int id)
        {
            var contentItem = await ContentManager.GetAsync(id);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(contentItem);
        }
    }
}
