using Microsoft.AspNet.Mvc;
using Orchard.DisplayManagement.ModelBinding;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Orchard.Hosting.Web.Mvc.ModelBinding
{
    public class ControllerModelUpdater : IModelUpdater
    {
        private readonly Controller _controller;

        public ControllerModelUpdater(Controller controller)
        {
            _controller = controller;
        }

        public ModelStateDictionary ModelState => _controller.ModelState;

        public Task<bool> TryUpdateModelAsync(object model, Type modelType, string prefix)
        {
            return _controller.TryUpdateModelAsync(model, modelType, prefix);
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
