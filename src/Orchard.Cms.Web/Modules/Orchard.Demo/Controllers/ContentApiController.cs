using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;
using System.Threading.Tasks;

namespace Orchard.Demo.Controllers
{
    public class ContentApiController : Controller
    {
        public async Task<IActionResult> GetById([FromServices] IContentManager contentManager, string id)
        {
            var contentItem = await contentManager.GetAsync(id);

            if (contentItem == null)
            {
                return NotFound();
            }

            return new ObjectResult(contentItem);
        }
    }
}
