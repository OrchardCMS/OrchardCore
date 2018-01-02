using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public class ApiUpdateModel : IApiUpdateModel
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly IModelBinderFactory _modelBinderFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IObjectModelValidator _objectModelValidator;

        private JObject _model;

        public ModelStateDictionary ModelState => new ModelStateDictionary();

        public ApiUpdateModel(IModelMetadataProvider metadataProvider, 
            IModelBinderFactory modelBinderFactory,
            IHttpContextAccessor httpContextAccessor,
            IObjectModelValidator objectModelValidator) {
            _metadataProvider = metadataProvider;
            _modelBinderFactory = modelBinderFactory;
            _httpContextAccessor = httpContextAccessor;
            _objectModelValidator = objectModelValidator;
        }

        public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        {
            var modelMetadata = _metadataProvider.GetMetadataForType(model.GetType());

            return TryUpdateModelAsync(model, null, m => null);
        }

        public async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        {
            var modelMetadata = _metadataProvider.GetMetadataForType(model.GetType());

            var modelState = new ModelStateDictionary();

            //var name = model.GetType().Name;
            var token = _model[prefix.ToGraphQLStringFormat()];

            if (token == null)
            {
                return false;
            }

            foreach (var content in token.Children())
            {
                modelState
                    .SetModelValue(
                        content.Path.Replace(prefix + ".", ""), content.ToString(), content.ToString());
                modelState
                    .SetModelValue(
                        content.Path, content.ToString(), content.ToString());

                modelState.MarkFieldValid(content.Path.Replace(prefix + ".", ""));
                modelState.MarkFieldValid(content.Path);
            }

            var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
                 _httpContextAccessor.HttpContext,
                 new Microsoft.AspNetCore.Routing.RouteData(),
                 new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                 modelState
                );

            var updateModel = await ModelBindingHelper.TryUpdateModelAsync<TModel>(
                model,
                prefix,
                actionContext,
                _metadataProvider,
                _modelBinderFactory,
                new ApiValueProvider(_model),
                _objectModelValidator);

            return updateModel;
        }

        public async Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        {
            //var expression = ModelBindingHelper.GetPropertyFilterExpression(includeExpressions);
            //var propertyFilter = expression.Compile();

            var modelMetadata = _metadataProvider.GetMetadataForType(model.GetType());

            var modelState = new ModelStateDictionary();

            //var name = model.GetType().Name;
            var token = _model[prefix.ToGraphQLStringFormat()];

            if (token == null)
            {
                return false;
            }

            

            foreach (var child in token.Children()) {

                foreach (var content in child.Children())
                {
                    var property = modelMetadata.Properties.FirstOrDefault(x => String.Equals(
                        (x.ContainerType.Name + "." + x.PropertyName), content.Path, StringComparison.InvariantCultureIgnoreCase));

                    if (property != null)
                    {
                        modelState.SetModelValue((property.ContainerType.Name + "." + property.PropertyName),  content.ToString(), content.ToString());
                        modelState.SetModelValue((property.PropertyName), content.ToString(), content.ToString());
                    }
                }
            }

            var actionContext = new Microsoft.AspNetCore.Mvc.ActionContext(
                 _httpContextAccessor.HttpContext,
                 new Microsoft.AspNetCore.Routing.RouteData(),
                 new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                 modelState
                );

            var updateModel = await ModelBindingHelper.TryUpdateModelAsync<TModel>(
                model,
                prefix,
                actionContext,
                _metadataProvider,
                _modelBinderFactory,
                new ApiValueProvider(_model),
                _objectModelValidator,
                includeExpressions);

            return updateModel;
        }

        public bool TryValidateModel(object model)
        {
            return true;
        }

        public bool TryValidateModel(object model, string prefix)
        {
            return true;
        }

        public IApiUpdateModel WithModel(JObject jObject)
        {
            _model = jObject;
            return this;
        }
    }

    public class ApiValueProvider : IValueProvider
    {
        private readonly JObject _values;

        public ApiValueProvider(JObject values)
        {
            _values = values;
        }

        public bool ContainsPrefix(string prefix)
        {
            return true;
        }

        public ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var splitKey = key.Split('.').Select(x => x.ToGraphQLStringFormat());
            var path = String.Join(".", splitKey);

            var token = _values.SelectToken(path);
            if (token == null)
            {
                return ValueProviderResult.None;
            }
            else
            {
                return new ValueProviderResult(new[] { token.ToString() });
            }
        }
    }
}
