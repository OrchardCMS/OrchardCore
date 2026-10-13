using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Tests.Stubs;

/// <summary>
/// An <see cref="IUpdateModel"/> that binds the values of the provided model to the model of the same type
/// being updated. Intended for unit testing display drivers.
/// </summary>
public sealed class StubModelUpdater<TSource> : IUpdateModel
    where TSource : class
{
    private readonly TSource _source;

    public StubModelUpdater(TSource source)
    {
        _source = source;
    }

    public ModelStateDictionary ModelState { get; } = new();

    public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        => BindAsync(model);

    public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        => BindAsync(model);

    public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        => BindAsync(model);

    public bool TryValidateModel(object model) => true;

    public bool TryValidateModel(object model, string prefix) => true;

    private Task<bool> BindAsync<TModel>(TModel model)
    {
        if (model is TSource target)
        {
            foreach (var property in typeof(TSource).GetProperties().Where(property => property.CanRead && property.CanWrite))
            {
                property.SetValue(target, property.GetValue(_source));
            }
        }

        return Task.FromResult(true);
    }
}
