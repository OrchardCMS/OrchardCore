using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class RazorPageModelUpdater : IUpdateModel
    {
        private readonly Page _page;

        public RazorPageModelUpdater(Page page)
        {
            _page = page;
        }

        public ModelStateDictionary ModelState => _page.ModelState;

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            return _page.TryUpdateModelAsync<TModel>(model);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            return _page.TryUpdateModelAsync<TModel>(model, prefix);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            return _page.TryUpdateModelAsync<TModel>(model, prefix, includeExpressions);
        }

        public bool TryValidateModel(object model)
        {
            return _page.TryValidateModel(model);
        }

        public bool TryValidateModel(object model, string prefix)
        {
            return _page.TryValidateModel(model, prefix);
        }
    }
}