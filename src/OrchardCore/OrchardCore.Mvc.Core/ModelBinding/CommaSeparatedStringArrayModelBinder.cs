using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding;

/// <summary>
/// An <see cref="IModelBinder"/> that binds a <see cref="string"/> array from both
/// repeated query-string values (<c>?key=a&amp;key=b</c>) and comma-separated values
/// (<c>?key=a,b</c>), or a mix of both.
/// </summary>
public sealed class CommaSeparatedStringArrayModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        if (bindingContext.ModelType != typeof(string[]))
        {
            return Task.CompletedTask;
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var values = valueProviderResult.Values
            .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToArray();

        bindingContext.Result = ModelBindingResult.Success(values.Length > 0 ? values : null);

        return Task.CompletedTask;
    }
}
