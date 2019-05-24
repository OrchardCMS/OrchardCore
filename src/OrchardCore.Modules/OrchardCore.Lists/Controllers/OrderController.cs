using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Services;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Controllers
{
    [Admin]
    public class OrderController : Controller
    {
        private readonly IListPartQueryService _listPartQueryService;
        //private readonly IContentManager _contentManager;

        public OrderController(
            IListPartQueryService listPartQueryService,
            IContentManager contentManager
            )
        {
            _listPartQueryService = listPartQueryService;
            //_contentManager = contentManager;
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrders(string containerId, int oldIndex, int newIndex, PagerSlimParameters pagerSlimParameters, int pageSize)
        {

            var pager = new PagerSlim(pagerSlimParameters, pageSize);
            var pageOfContentItems = (await _listPartQueryService.QueryListItemsAsync(containerId, true, pager, true)).ToList();
            //var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            //var query = OrderByPosition(GetListContentItemQuery(containerId));
            //if (query == null)
            //{
            //    return HttpNotFound();
            //}
            //var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            var contentItem = pageOfContentItems[oldIndex];
            pageOfContentItems.Remove(contentItem);
            pageOfContentItems.Insert(newIndex, contentItem);

            //pager.Page) - PageDefault) * PageSize;
            //var index = pager.GetStartIndex() + pageOfContentItems.Count;
            //foreach (var item in pageOfContentItems.Select(x => x.As<ContainablePart>()))
            //{
            //    item.Position = --index;
            //    RePublish(item);
            //}
            return Ok();
        }
    }
}
