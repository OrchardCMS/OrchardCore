using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class NullModelUpdater : IUpdateModel
    {
        public NullModelUpdater()
        {
        }

        public ModelStateDictionary ModelState { get; } = new ModelStateDictionary();
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class => Task.FromResult(true);
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class => Task.FromResult(true);
        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class => Task.FromResult(true);
        public bool TryValidateModel(object model) => true;
        public bool TryValidateModel(object model, string prefix) => true;
    }
}
