using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Mvc.ModelBinding;

/// <summary>
/// Model binder to produce a validation error when a Boolean field contains a value that's not a valid bool. The
/// default model binder would throw a <see cref="FormatException"/>. That's an issue for e.g. the Users module, see
/// <see href="https://github.com/OrchardCMS/OrchardCore/issues/14792"/>.
/// </summary>
internal sealed class SafeBoolModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        if (bool.TryParse(valueProviderResult.FirstValue, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }

        var localizer = bindingContext.HttpContext.RequestServices.GetService<IStringLocalizer<SafeBoolModelBinder>>();

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, localizer["Invalid Boolean value."]);

        return Task.CompletedTask;
    }
}
