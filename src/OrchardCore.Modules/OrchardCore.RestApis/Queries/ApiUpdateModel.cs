using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.RestApis.Queries
{
    public class ApiUpdateModel : IUpdateModel
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly JObject _model;

        public ModelStateDictionary ModelState => new ModelStateDictionary();

        public ApiUpdateModel(IModelMetadataProvider metadataProvider,  JObject model) {
            _metadataProvider = metadataProvider;
            _model = model;
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            return Task.FromResult(false);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            return Task.FromResult(false);
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            var expression = ModelBindingHelper.GetPropertyFilterExpression(includeExpressions);
            var propertyFilter = expression.Compile();

            var modelMetadata = _metadataProvider.GetMetadataForType(model.GetType());

            return Task.FromResult(false);
        }

        public bool TryValidateModel(object model)
        {
            return false;
        }

        public bool TryValidateModel(object model, string prefix)
        {
            return false;
        }
    }
}
