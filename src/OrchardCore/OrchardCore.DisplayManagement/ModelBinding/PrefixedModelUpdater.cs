using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.DisplayManagement.ModelBinding
{
    public class PrefixedModelUpdater : IUpdateModel
    {
        private readonly IUpdateModel _updateModel;

        public PrefixedModelUpdater(IUpdateModel updateModel) : this(updateModel, x => x)
        {
        }

        public PrefixedModelUpdater(IUpdateModel updateModel, Func<string, string> prefix)
        {
            _updateModel = updateModel;
            Prefix = prefix;
        }

        public ModelStateDictionary ModelState => _updateModel.ModelState;

        public Func<string, string> Prefix { get; set; }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
            => _updateModel.TryUpdateModelAsync(model);

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
            => _updateModel.TryUpdateModelAsync(Prefix(prefix));

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
            => _updateModel.TryUpdateModelAsync(model, Prefix(prefix), includeExpressions);

        public bool TryValidateModel(object model) => _updateModel.TryValidateModel(model);

        public bool TryValidateModel(object model, string prefix)
            => _updateModel.TryValidateModel(model, Prefix(prefix));
    }
}
