using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class ModelStateWrapperUpdater : IUpdateModel
    {
        private readonly IUpdateModel _updater;
        private readonly ModelStateDictionary _commonModelState;

        public ModelStateWrapperUpdater(IUpdateModel updateModel)
        {
            _updater = updateModel;
            _commonModelState = new ModelStateDictionary();
        }

        public ModelStateDictionary ModelState => _updater.ModelState;

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            return PreserveModelState(() => _updater.TryUpdateModelAsync(model));
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            return PreserveModelState(() => _updater.TryUpdateModelAsync(model, prefix));
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            return PreserveModelState(() => _updater.TryUpdateModelAsync(model, prefix, includeExpressions));
        }

        public bool TryValidateModel(object model)
        {
            return PreserveModelState(() => _updater.TryValidateModel(model));
        }

        public bool TryValidateModel(object model, string prefix)
        {
            return PreserveModelState(() => _updater.TryValidateModel(model, prefix));
        }

        private T PreserveModelState<T>(Func<T> action)
        {
            // Passing a clean ModelState to child contentItem.
            _commonModelState.Merge(_updater.ModelState);
            _updater.ModelState.Clear();

            var result = action();

            _updater.ModelState.Merge(_commonModelState);

            return result;
        }
    }
}
