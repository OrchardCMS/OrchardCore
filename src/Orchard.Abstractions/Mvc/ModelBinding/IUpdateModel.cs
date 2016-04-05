using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.DisplayManagement.ModelBinding
{
    public interface IUpdateModel
    {
        Task<bool> TryUpdateModelAsync(object model, Type modelType, string prefix);
        Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class;
        bool TryValidateModel(object model);
        bool TryValidateModel(object model, string prefix);
        ModelStateDictionary ModelState { get; }
    }
}
