using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class PageModelUpdater : PageModel, IUpdateModel
    {
        public PageModelUpdater(PageModel page) => PageContext = page.PageContext;

        public new Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
            => base.TryUpdateModelAsync(model);

        public new Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
            => base.TryUpdateModelAsync(model, prefix);

        public new Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
            => base.TryUpdateModelAsync(model, prefix, includeExpressions);
    }
}
