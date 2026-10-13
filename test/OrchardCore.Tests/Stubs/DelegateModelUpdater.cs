using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Tests.Stubs;

/// <summary>
/// An <see cref="IUpdateModel"/> that binds models with a delegate instead of a form, to drive display drivers from tests.
/// </summary>
internal sealed class DelegateModelUpdater : IUpdateModel
{
    private readonly Action<object> _bind;

    public DelegateModelUpdater(Action<object> bind)
    {
        _bind = bind;
    }

    public ModelStateDictionary ModelState { get; } = new();

    public Task<bool> TryUpdateModelAsync<TModel>(TModel model) where TModel : class
        => BindAsync(model);

    public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix) where TModel : class
        => BindAsync(model);

    public Task<bool> TryUpdateModelAsync<TModel>(TModel model, string prefix, params Expression<Func<TModel, object>>[] includeExpressions) where TModel : class
        => BindAsync(model);

    public bool TryValidateModel(object model)
        => true;

    public bool TryValidateModel(object model, string prefix)
        => true;

    private Task<bool> BindAsync(object model)
    {
        _bind(model);

        return Task.FromResult(true);
    }
}
