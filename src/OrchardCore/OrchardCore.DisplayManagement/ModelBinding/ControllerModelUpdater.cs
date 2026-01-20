using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class ControllerModelUpdater : IUpdateModel
    {
        private readonly Controller _controller;

        public ControllerModelUpdater(Controller controller)
        {
            _controller = controller;
        }

        public ModelStateDictionary ModelState => _controller.ModelState;

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            return _controller.TryUpdateModelAsync<TModel>(model);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            return _controller.TryUpdateModelAsync<TModel>(model, prefix);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            return _controller.TryUpdateModelAsync<TModel>(model, prefix, includeExpressions);
        }

        public bool TryValidateModel(object model)
        {
            return _controller.TryValidateModel(model);
        }

        public bool TryValidateModel(object model, string prefix)
        {
            return _controller.TryValidateModel(model, prefix);
        }
    }
}
