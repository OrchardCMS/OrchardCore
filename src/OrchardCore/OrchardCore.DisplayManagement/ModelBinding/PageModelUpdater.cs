using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class PageModelUpdater : PageModel, IUpdateModel
    {
        private readonly PageModel _page;

        public PageModelUpdater(PageModel page)
        {
            _page = page;
            PageContext = page.PageContext;
        }

        public new ModelStateDictionary ModelState => _page.ModelState;

        public new Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            return base.TryUpdateModelAsync<TModel>(model);
        }

        public new Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            return base.TryUpdateModelAsync<TModel>(model, prefix);
        }

        public new Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            return base.TryUpdateModelAsync<TModel>(model, prefix, includeExpressions);
        }

        public new bool TryValidateModel(object model)
        {
            return base.TryValidateModel(model);
        }

        public new bool TryValidateModel(object model, string prefix)
        {
            return base.TryValidateModel(model, prefix);
        }
    }
}
