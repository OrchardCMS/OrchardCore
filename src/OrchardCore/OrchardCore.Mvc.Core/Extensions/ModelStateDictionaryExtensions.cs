using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding;

public static class ModelStateDictionaryExtensions
{
    public static void AddModelError(this ModelStateDictionary modelState, ModelError error)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        ArgumentNullException.ThrowIfNull(error);

        modelState.AddModelError(error.Key, error.Message);
    }

    public static void AddModelErrors(this ModelStateDictionary modelState, IEnumerable<ModelError> errors)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Any())
        {
            foreach (var error in errors)
            {
                modelState.AddModelError(error);
            }
        }
    }
}
